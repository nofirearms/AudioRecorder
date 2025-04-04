using Microsoft.Win32;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace AudioRecorder.Service
{
    public class AudioService
    {
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------DEVICES---------------//
        #region DEVICES
        private MMDeviceEnumerator _deviceEnumerator = new MMDeviceEnumerator();
        /// <summary>
        /// Get list of Wasapi devices, inputs and outputs
        /// </summary>
        public IEnumerable<MMDevice> GetRecordDevices()
        {
            var input_devices = _deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            var output_devices = _deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active); 

            List<MMDevice> devices = [..input_devices, ..output_devices];

            return devices;
        }
        /// <summary>
        /// Get default input device
        /// </summary>
        public MMDevice GetDefaultInputDevice() => _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
        /// <summary>
        /// Get default outpute device for loopback recording
        /// </summary>
        public MMDevice GetDefaultOutputDevice() => _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
        /// <summary>
        /// Get device by ID
        /// </summary>
        public MMDevice GetDeviceById(string id) => _deviceEnumerator.GetDevice(id);
        /// <summary>
        /// Initializes a new istance of Wasapi capture class
        /// </summary>
        /// <param name="device"></param>
        /// <returns>Capture class instance, input or loopback</returns>
        public WasapiCapture GetWasapiCaptureInstance(MMDevice device) => device.DataFlow == DataFlow.Capture ? new WasapiCapture(device) : new WasapiLoopbackCapture(device);
        #endregion


        //----------------------------------------------------------------------------------------------------------------------------------------------------------------GRAPH----------------//
        #region GRAPH

        

        /// <summary>
        /// Getting peak sample from buffer
        /// </summary>
        public double PeakSampleFromBuffer(byte[] bytes, int count)
        {

            double max = 0;
            var buffer = new WaveBuffer(bytes);

            for (int index = 0; index < count / 4; index++)
            {
                var sample = buffer.FloatBuffer[index];
                sample = Math.Abs(sample);
                if (sample > max) max = sample;
            }

            return max;
        }
        /// <summary>
        /// Get points collection from stream to create a graph
        /// </summary>
        public PointCollection GetPointsFromWaveStream(WaveStream source)
        {
            int samples_read;
            source.Position = 0;
            var points = new PointCollection();

            var buffer = new byte[source.WaveFormat.AverageBytesPerSecond / 10];
            while ((samples_read = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                var max = PeakSampleFromBuffer(buffer, buffer.Length);
                points.Add(new Point(points.Count, max));
            }
            //Добавляем -1, -1, чтобы знать где конец списка
            points.Add(new Point(-1, -1));
            return points;
        }

        /// <summary>
        /// Get points collection from stream to create a graph
        /// </summary>
        public PointCollection GetPointsFromAudio(string path)
        { 
            var points = new PointCollection();

            using (var reader = new AudioFileReader(path))
            {
                //TODO переделать под float
                reader.Position = 0;
                //var buffer = new byte[reader.WaveFormat.AverageBytesPerSecond / 10];
                var length = reader.Length / 200;
                var buffer = new byte[length - length % reader.WaveFormat.BlockAlign ];
                while (reader.Read(buffer, 0, buffer.Length) > 0)
                {
                    var max = PeakSampleFromBuffer(buffer, buffer.Length);
                    points.Add(new Point(points.Count, max));
                }
                //Добавляем -1, -1, чтобы знать где конец списка
                points.Add(new Point(-1, -1));
            }

            return points;
        }

        #endregion

        //----------------------------------------------------------------------------------------------------------------------------------------------------------------AUDIO-HELPERS--------//

        #region AUDIO
        /// <summary>
        /// Open audio from path or picker
        /// </summary>
        /// <param name="path">Path can be null, so that audio will be open from a file</param>
        public async Task<WaveStream> OpenAudioFileToWaveStreamAsync(string path = null)
        {

            if (string.IsNullOrEmpty(path))
            {
                var picker = new OpenFileDialog();
                picker.Filter = "Audio files|*.mp3;*.wav|*.MP3|*.mp3|*.WAV|*.wav";
                if (picker.ShowDialog() == true)
                {
                    path = picker.FileName;
                }
                else
                {
                    return null;
                }

            }
            //TODO вывод ошибки
            if (!File.Exists(path)) return null;

            var reader = new AudioFileReader(path);

            var memory_stream = new MemoryStream();

            await reader.CopyToAsync(memory_stream);
            reader.Close();
            reader.Dispose();
            var out_stream = new NamedRawSourceWaveStream(memory_stream, reader.WaveFormat, System.IO.Path.GetFileNameWithoutExtension(reader.FileName));

            return out_stream;
        }

        public string OpenAudioFile(string path = null)
        {

            if (string.IsNullOrEmpty(path))
            {
                var picker = new OpenFileDialog();
                picker.Filter = "Audio files|*.mp3;*.wav|*.MP3|*.mp3|*.WAV|*.wav";
                if (picker.ShowDialog() == true)
                {
                    path = picker.FileName;
                }
                else
                {
                    return null;
                }

            }
            //TODO вывод ошибки
            if (!File.Exists(path)) return null;

            return path;
        }

        public async Task RenderWaveStreamToMp3Async(string path, WaveStream waveStream, int bitRate = 128000)
        {
            await Task.Run(() =>
            {

                using (var resampler = new MediaFoundationResampler(waveStream, waveStream.WaveFormat))
                {
                    waveStream.Position = 0;
                    MediaFoundationEncoder.EncodeToMp3(resampler, path, bitRate);
                }
            });

        }

        public async Task RenderToMp3Async(string inputPath, string outputPath, int bitRate = 192000)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var reader = new AudioFileReader(inputPath))
                    {
                        MediaFoundationEncoder.EncodeToMp3(reader, outputPath, bitRate); //var a = new LameMP3FileWriter()
                    }
                }
                catch(Exception e)
                {
                    Debug.WriteLine(e.Message);
                }

            });

        }


        /// <summary>
        /// Crop wave stream
        /// </summary>
        /// <param name="source"></param>
        /// <param name="startPosition">Position to crop 0 - start, 1 - end</param>
        /// <param name="endPosition">Position to crop 0 - start, 1 - end</param>
        /// <returns>Cropped wave stream</returns>
        public WaveStream CropWaveStream(WaveStream source, float startPosition, float endPosition)
        {

            source.Position = 0;

            var new_start_position = (long)(source.Length * startPosition);
            var new_end_position = (long)(source.Length * endPosition);

            //newStartPosition = newStartPosition - newStartPosition % raw_source.WaveFormat.BlockAlign;
            //newStopPosition = newStopPosition - newStopPosition % raw_source.WaveFormat.BlockAlign;

            var outStream = new MemoryStream();


            source.Position = new_start_position;
            byte[] buffer = new byte[source.WaveFormat.AverageBytesPerSecond];

            while (source.Position < new_end_position)
            {
                int bytesRequired = (int)(new_end_position - source.Position);
                if (bytesRequired > 0)
                {
                    int bytesToRead = Math.Min(bytesRequired, buffer.Length);
                    int bytesRead = source.Read(buffer, 0, bytesToRead);

                    if (bytesRead <= 0) break;
                    outStream.Write(buffer, 0, bytesRead);
                }
            }
            
            var out_source = new RawSourceWaveStream(outStream, source.WaveFormat);

            source.Dispose();

            return out_source;
        }

        public async Task CropAudioFileAsync(string inputPath, string tempPath, float startPosition = 0, float endPosition = 1)
        {
            Helpers.PathHelper.CreateDirectory(tempPath);
            await CopyAudioAsync(inputPath, tempPath);

            using (var reader = new AudioFileReader(tempPath))
            using (var writer = new WaveFileWriter(inputPath, reader.WaveFormat))
            {
                var new_start_position = (long)((reader.Length * startPosition) - (reader.Length * startPosition) % reader.WaveFormat.BlockAlign);
                var new_end_position = (long)((reader.Length * endPosition) - (reader.Length * endPosition) % reader.WaveFormat.BlockAlign);

                reader.Position = new_start_position;
                byte[] buffer = new byte[reader.WaveFormat.AverageBytesPerSecond];

                while (reader.Position < new_end_position)
                {
                    int bytesRequired = (int)(new_end_position - reader.Position);
                    if (bytesRequired > 0)
                    {
                        int bytesToRead = Math.Min(bytesRequired, buffer.Length);
                        int bytesRead = reader.Read(buffer, 0, bytesToRead/* - (buffer.Length % reader.WaveFormat.BlockAlign)*/);

                        if (bytesRead <= 0) break;
                        writer.Write(buffer, 0, bytesRead);
                    }
                }
            }
            await RemoveAudioAsync(tempPath);
        }

        public async Task NormalizeAudioAsync(string inputPath, string tempPath, float peak = 1.0f)
        {
            await Task.Run(async() =>
            {
                Helpers.PathHelper.CreateDirectory(tempPath);
                await CopyAudioAsync(inputPath, tempPath);

                float max = 0f;
                using (var reader = new AudioFileReader(tempPath))
                {
                    // find the max peak
                    float[] buffer = new float[reader.WaveFormat.SampleRate];
                    int read;
                    do
                    {
                        read = reader.Read(buffer, 0, buffer.Length);
                        for (int n = 0; n < read; n++)
                        {
                            var abs = Math.Abs(buffer[n]);
                            if (abs > max) max = abs;
                        }
                    } while (read > 0);
#if DEBUG
                    Debug.WriteLine($"Max sample value: {max}");
#endif
                    //if (max == 0 || max > 1.0f)
                    //    throw new InvalidOperationException("File cannot be normalized");

                    // rewind and amplify
                    reader.Position = 0;
                    reader.Volume = peak / max;

                    // write out to a new WAV file
                    WaveFileWriter.CreateWaveFile(inputPath, reader);
                }
                await RemoveAudioAsync(tempPath);
            });
        }

        public async Task CopyAudioAsync(string inputAudio, string outputAudio)
        {
            await Task.Run(async() =>
            {
                Helpers.PathHelper.CreateDirectory(outputAudio);
                using (var reader = new AudioFileReader(inputAudio))
                using (var writer = new WaveFileWriter(outputAudio, reader.WaveFormat))
                {
                    var buffer = new byte[reader.WaveFormat.AverageBytesPerSecond];
                    int bytes_read = 0;
                    while ((bytes_read = await reader.ReadAsync(buffer, 0, buffer.Length/* - (buffer.Length % reader.WaveFormat.BlockAlign)*/)) > 0)
                    {
                        await writer.WriteAsync(buffer, 0, bytes_read);
                    }
                }
            });
        }

        public async Task RemoveAudioAsync(string path)
        {
            await Task.Run(() =>
            {
                try
                {
                    File.Delete(path);
                }
                catch(Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            });
        }


        public TimeSpan GetAudioDuration(string path)
        {
            try
            {
                using(var reader = new AudioFileReader(path))
                {
                    return reader.TotalTime;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return default;
            }
            
        }

        #endregion

        //----------------------------------------------------------------------------------------------------------------------------------------------------------------HELP-STUFF-----------//
    }
    #region HELP STUFF
    public enum AudioState
    {
        Default, Record, Play, Stop, Pause, Loading
    }

    public class NamedRawSourceWaveStream : RawSourceWaveStream
    {
        public string Name { get; set; }
        public NamedRawSourceWaveStream(Stream sourceStream, WaveFormat waveFormat, string name) : base(sourceStream, waveFormat)
        {
            Name = name;
        }
    }
    #endregion
}
