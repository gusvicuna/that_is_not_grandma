using System;

namespace Game.Domain
{
    /// <summary>
    /// Picks clip variations without ever repeating the previous one — the difference between
    /// footsteps and a machine gun. Randomness is injected so a sequence is reproducible.
    /// </summary>
    public class NoRepeatPicker
    {
        private readonly int _count;
        private readonly Random _random;
        private int _lastIndex = -1;

        public NoRepeatPicker(int count, Random random)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be at least 1.");

            _count = count;
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public int Next()
        {
            // A single variation must not send the retry loop spinning forever.
            if (_count == 1)
            {
                return 0;
            }

            int index;
            do
            {
                index = _random.Next(_count);
            }
            while (index == _lastIndex);

            _lastIndex = index;
            return index;
        }
    }
}
