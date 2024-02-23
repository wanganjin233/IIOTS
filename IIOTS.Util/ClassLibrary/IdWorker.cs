namespace IIOTS.Util
{
    public class IdWorker
    {
        public const long Twepoch = 1288834974657L;

        private const int WorkerIdBits = 10;

        private const int DatacenterIdBits = 0;

        private const int SequenceBits = 12;

        private const long MaxWorkerId = long.MaxValue;

        private const long MaxDatacenterId = 0L;

        private const int WorkerIdShift = 12;

        private const int DatacenterIdShift = 22;

        public const int TimestampLeftShift = 22;

        private const long SequenceMask = 4095L;

        private long _sequence = 0L;

        private long _lastTimestamp = -1L;

        private readonly object _lock = new object();

        public long WorkerId { get; protected set; }

        public long DatacenterId { get; protected set; }

        public long Sequence
        {
            get
            {
                return _sequence;
            }
            internal set
            {
                _sequence = value;
            }
        }

        public IdWorker(long workerId, long sequence = 0L)
        {
            WorkerId = workerId;
            DatacenterId = 0L;
            _sequence = sequence;
            if (workerId > MaxWorkerId || workerId < 0)
            {
                throw new ArgumentException($"worker Id can't be greater than {1023L} or less than 0");
            }
        }

        public virtual long NextId()
        {
            lock (_lock)
            {
                long num = TimeGen();
                if (num < _lastTimestamp)
                {
                    throw new Exception($"Clock moved backwards.  Refusing to generate id for {_lastTimestamp - num} milliseconds");
                }

                if (_lastTimestamp == num)
                {
                    _sequence = (_sequence + 1) & 0xFFF;
                    if (_sequence == 0)
                    {
                        num = TilNextMillis(_lastTimestamp);
                    }
                }
                else
                {
                    _sequence = 0L;
                }

                _lastTimestamp = num;
                return (num - 1288834974657L << 22) | (DatacenterId << 22) | (WorkerId << 12) | _sequence;
            }
        }

        protected virtual long TilNextMillis(long lastTimestamp)
        {
            long num;
            for (num = TimeGen(); num <= lastTimestamp; num = TimeGen())
            {
            }

            return num;
        }

        protected virtual long TimeGen()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        }
    }
}
