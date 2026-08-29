using System;

namespace Game.Domain
{
    /// <summary>
    /// The day's budget: a countdown that also gets charged for significant actions. Knows nothing
    /// about frames — Presentation decides what a tick is worth.
    /// </summary>
    public class DayClock
    {
        private readonly float _minimumAfterSpend;

        /// <param name="minimumAfterSpend">
        /// Floor an action's cost may not push the clock below. Losing the night in the same instant
        /// you finished a conversation reads as the game cheating, so a charge leaves at least this
        /// much on the clock. Only time actually passing can reach zero.
        /// </param>
        public DayClock(float secondsPerDay, float minimumAfterSpend = 0f)
        {
            if (secondsPerDay <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(secondsPerDay), "A day must last more than zero seconds.");
            }
            if (minimumAfterSpend < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumAfterSpend), "The floor cannot be negative.");
            }
            if (minimumAfterSpend >= secondsPerDay)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumAfterSpend),
                    "A floor at or above the length of the day would make every action free.");
            }
            SecondsPerDay = secondsPerDay;
            Remaining = secondsPerDay;
            _minimumAfterSpend = minimumAfterSpend;
        }

        public float SecondsPerDay { get; }

        public float Remaining { get; private set; }

        /// <summary>1 at dawn, 0 at nightfall. Drives the UI bar.</summary>
        public float NormalizedRemaining => Remaining / SecondsPerDay;

        public bool IsExpired => Remaining <= 0f;

        /// <summary>Time passing. This is the only thing that can run the day out.</summary>
        public void Tick(float deltaSeconds)
        {
            if (deltaSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds), "Time does not run backwards.");
            }
            Remaining = Clamp(Remaining - deltaSeconds, 0f);
        }

        /// <summary>
        /// The fixed cost of an action: talking, trading, searching. Never drops the clock below the
        /// floor — and never raises it either, so an action taken with less than the floor left
        /// simply costs nothing rather than handing time back.
        /// </summary>
        public void Spend(float cost)
        {
            if (cost < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(cost), "An action cannot give time back.");
            }
            float floor = Math.Min(Remaining, _minimumAfterSpend);
            Remaining = Clamp(Remaining - cost, floor);
        }

        public void ResetForNewDay()
        {
            Remaining = SecondsPerDay;
        }

        private static float Clamp(float value, float floor)
        {
            return value < floor ? floor : value;
        }
    }
}
