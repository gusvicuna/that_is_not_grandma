using System;

namespace Game.Domain
{
    /// <summary>
    /// The day's budget: a countdown that also gets charged for significant actions. Knows nothing
    /// about frames — Presentation decides what a tick is worth.
    /// </summary>
    public class DayClock
    {
        public DayClock(float secondsPerDay)
        {
            if (secondsPerDay <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(secondsPerDay), "A day must last more than zero seconds.");
            }
            SecondsPerDay = secondsPerDay;
            Remaining = secondsPerDay;
        }

        public float SecondsPerDay { get; }

        public float Remaining { get; private set; }

        /// <summary>1 at dawn, 0 at nightfall. Drives the UI bar.</summary>
        public float NormalizedRemaining => Remaining / SecondsPerDay;

        public bool IsExpired => Remaining <= 0f;

        /// <summary>Time passing.</summary>
        public void Tick(float deltaSeconds)
        {
            if (deltaSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds), "Time does not run backwards.");
            }
            Reduce(deltaSeconds);
        }

        /// <summary>The fixed cost of an action: talking, trading, searching.</summary>
        public void Spend(float cost)
        {
            if (cost < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(cost), "An action cannot give time back.");
            }
            Reduce(cost);
        }

        public void ResetForNewDay()
        {
            Remaining = SecondsPerDay;
        }

        private void Reduce(float amount)
        {
            Remaining -= amount;
            if (Remaining < 0f)
            {
                Remaining = 0f;
            }
        }
    }
}
