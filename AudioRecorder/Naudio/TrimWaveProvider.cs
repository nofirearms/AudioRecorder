using NAudio.Wave;

namespace AudioRecorder.NAudio
{
    public class TrimWaveProvider : IWaveProvider
    {

        private WaveStream _source;

        private long _stopPosition = 0;
        public WaveFormat WaveFormat => _source.WaveFormat;

        public TrimWaveProvider(WaveStream source, long startPosition, long stopPosition)
        {
            _stopPosition = stopPosition;
            _source = source;
            _source.Position = startPosition;
        }

        public TrimWaveProvider(WaveStream source)
        {
            _source = source;
            _source.Position = 0;
            _stopPosition = _source.Length;
        }

        public long StartPosition
        {
            set
            {
                _source.Position = value;
            }
        }

        public long StopPosition
        {
            set
            {
                _stopPosition = value;
            }
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            if (_source.Position <= _stopPosition)
            {
                var bytes_required = _stopPosition - _source.Position;
                int bytes_to_read = (int)Math.Min(bytes_required, count);
                return _source.Read(buffer, offset, bytes_to_read);
            }

            return 0;
        }
    }
}
