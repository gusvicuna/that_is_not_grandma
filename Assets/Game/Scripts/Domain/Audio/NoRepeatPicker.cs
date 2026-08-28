using System;

namespace Game.Domain
{
    public class NoRepeatPicker
    {
        private readonly int _count;
        private readonly Random _rng;
        private int _last = -1;

        public NoRepeatPicker(int count, Random rng)
        {
            if (count < 1) throw new ArgumentOutOfRangeException(nameof(count), "count must be >= 1");
            _count = count;
            _rng = rng ?? throw new ArgumentNullException(nameof(rng));
        }

        public int Next()
        {
            if (_count == 1) return 0;

            int pick;
            do { pick = _rng.Next(_count); } while (pick == _last);
            _last = pick;
            return pick;
        }
    }
}
