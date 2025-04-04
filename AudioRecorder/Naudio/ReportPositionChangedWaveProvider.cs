using NAudio.Wave;

namespace AudioRecorder.NAudio
{
    public class ReportPositionChangedWaveProvider : IWaveProvider
    {
        private readonly IWaveProvider _source;
        public event EventHandler<int> PositionChanged;
        public WaveFormat WaveFormat => _source.WaveFormat;


        public ReportPositionChangedWaveProvider(IWaveProvider source)
        {
            _source = source;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            PositionChanged?.Invoke(this, count);
            return _source.Read(buffer, offset, count);
        }
    }
}
