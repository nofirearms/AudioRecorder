using NAudio.CoreAudioApi;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using AudioRecorder.NAudio;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace AudioRecorder.Service
{
    public class PlayerRecorder
    {

        private readonly AudioService _audioService;

        public PlayerRecorder()
        {
            _audioService = App.Current.Services.GetRequiredService<AudioService>();
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------------------------PLAYBACK-------------//

        #region PLAYBACK

        private WaveOutEvent _waveOut = new WaveOutEvent();
        private AudioFileReader _reader;

        private float _volume = 1.0f;
        public float Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                if (_reader is not null)
                    _reader.Volume = value;
            }
        }
        /// <summary>
        /// Playback audio, start position from 0 to 1
        /// </summary>
        /// <param name="path"></param>
        /// <param name="startPosition">From 0 to 1</param>
        public void StartPlayback(string path, double startPosition = 0, float volume = float.NaN, float fileVolume = 1f)
        {
            if (string.IsNullOrEmpty(path)) return;

            if (State == AudioState.Record || State == AudioState.Default)
            {
                return;
            }
            else if (State == AudioState.Play)
            {
                OnPlaybackStopped(null, null);
                StopPlayback();
                StartPlayback(path, startPosition);
            }
            else if (State == AudioState.Pause)
            {
                _waveOut.Play();
                State = AudioState.Play;
            }
            else if (State == AudioState.Stop)
            {
                _reader = new AudioFileReader(path);
                _reader.Position = (long)(_reader.Length * startPosition);
                //меняем уровень ридера если надо
                Volume = volume is float.NaN ? Volume : volume;

                var notify = new NotifyingSampleProvider(_reader);
                notify.Sample += (s, a) =>
                {
                    if (State == AudioState.Play)
                    {
                        //CurrentPlaybackPositionDouble = (double)_reader.Position / _reader.Length;
                        //CurrentPlaybackPositionTimeSpan = _reader.CurrentTime;
                    }
                };

                var file_volume = new VolumeSampleProvider(notify);
                file_volume.Volume = fileVolume;


                _waveOut.PlaybackStopped += OnPlaybackStopped;
                _waveOut.Init(file_volume);
                _waveOut.Play();

                State = AudioState.Play;
            }
        }

        private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
        {
            State = AudioState.Stop;
            if (_waveOut != null) _waveOut.PlaybackStopped -= OnPlaybackStopped;
            _reader?.Dispose();
        }

        public void StopPlayback()
        {
            _waveOut?.Stop();
        }

        public void PausePlayback()
        {
            if (State == AudioState.Record || State == AudioState.Default)
            {
                return;
            }
            else if (State == AudioState.Play)
            {
                _waveOut.Pause();

                State = AudioState.Pause;
                return;
            }
            else if (State == AudioState.Pause)
            {
                _waveOut.Play();

                State = AudioState.Play;
                return;
            }
            else if (State == AudioState.Stop)
            {
                return;
            }
        }
        #endregion

        //----------------------------------------------------------------------------------------------------------------------------------------------------------------RECIRDING------------//

        #region RECORDING

        private WasapiCapture _captureSource;
        private MMDevice _captureDevice;
        private WaveFileWriter _writer;
        public event EventHandler<AudioState> StateChanged;
        public event EventHandler<Point> GraphChanged;

        private AudioState _state = AudioState.Stop;
        

        public AudioState State
        {
            get => _state;
            set
            {
                _state = value;
                StateChanged?.Invoke(this, value);
            }
        }

        public void StartRecording(string path, string deviceId)
        {
            if (State == AudioState.Play)
            {
                return;
            }
            else if (State == AudioState.Pause)
            {
                StopPlayback();
                StartRecording(path, deviceId);
            }
            else if (State == AudioState.Record)
            {
                StopRecording();
                StartRecording(path, deviceId);
            }
            else if (State == AudioState.Stop || State == AudioState.Default)
            {
                _captureDevice = _audioService.GetDeviceById(deviceId);
                _captureSource = _audioService.GetWasapiCaptureInstance(_captureDevice);

                _writer = new WaveFileWriterWithCounter(path, _captureSource.WaveFormat);

                _captureSource.DataAvailable += OnCaptureDataAvailable;
                _captureSource.RecordingStopped += OnRecordingStopped;

                _captureSource.StartRecording();

                State = AudioState.Record;
            }
        }

        public void StopRecording()
        {
            _captureSource.StopRecording();
            State = AudioState.Stop;
        }


        private void OnCaptureDataAvailable(object source, WaveInEventArgs e)
        {
            _writer.Write(e.Buffer, 0, e.BytesRecorded);
            var peak = _audioService.PeakSampleFromBuffer(e.Buffer, e.BytesRecorded);
            GraphChanged?.Invoke(this, new Point((_writer as WaveFileWriterWithCounter).Counter, peak));
            //CurrentPlaybackPositionTimeSpan = _writer.TotalTime;
        }

        private void OnRecordingStopped(object s, StoppedEventArgs a)
        {
            _captureSource.DataAvailable -= OnCaptureDataAvailable;
            _captureSource.RecordingStopped -= OnRecordingStopped;

            _writer.Dispose();
            _captureSource.Dispose();
            //добавляем точку чтобы знать где конец списка, для графа
            GraphChanged?.Invoke(this, new Point(-1, -1));
        }



        #endregion

        public void Dispose()
        {
            _writer?.Dispose();
            _writer = null;

            _reader?.Dispose();
            _reader = null;

            _captureSource?.Dispose();
            _captureSource = null;

            _captureDevice?.Dispose();
            _captureDevice = null;

            _waveOut?.Dispose();
            _waveOut = null;
        }
    }
}
