using NAudio.Wave;

namespace AudioRecorder.NAudio
{
    public class WaveFileWriterWithCounter : WaveFileWriter
    {
        public WaveFileWriterWithCounter(string filename, WaveFormat format) : base(filename, format)
        {
        }

        public int Counter { get; set; } = 0;

        public void ResetCounter() => Counter = 0;

        public override void Write(byte[] data, int offset, int count)
        {
            base.Write(data, offset, count);
            ++Counter;
        }
    }
}
