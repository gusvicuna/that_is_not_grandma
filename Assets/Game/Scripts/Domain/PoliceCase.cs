using System;

namespace Game.Domain
{
    public class PoliceCase
    {
        private readonly int _firstAvailableDay;
        private bool _callUsedToday;

        public int TrustRemaining { get; private set; }
        public int CurrentDay { get; private set; }
        public bool IsResolved { get; private set; }

        public bool IsPhoneAvailable =>
            !IsResolved && CurrentDay >= _firstAvailableDay;

        public bool CanCall =>
            IsPhoneAvailable && !_callUsedToday;

        public PoliceCase(int startingTrust = 2, int firstAvailableDay = 2)
        {
            if (startingTrust < 1)
                throw new ArgumentOutOfRangeException(nameof(startingTrust));

            if (firstAvailableDay < 1)
                throw new ArgumentOutOfRangeException(nameof(firstAvailableDay));

            TrustRemaining = startingTrust;
            CurrentDay = 0;
            IsResolved = false;

            _firstAvailableDay = firstAvailableDay;
            _callUsedToday = false;
        }

        public void StartDay(int day)
        {
            if (day < 1)
                throw new ArgumentOutOfRangeException(nameof(day));

            CurrentDay = day;
            _callUsedToday = false;
        }

        public PoliceCallOutcome Call(bool clueIsEvidence)
        {
            if (!CanCall)
                return PoliceCallOutcome.Unavailable;

            _callUsedToday = true;

            if (clueIsEvidence)
            {
                IsResolved = true;
                return PoliceCallOutcome.Won;
            }

            TrustRemaining--;

            if (TrustRemaining <= 0)
            {
                TrustRemaining = 0;
                IsResolved = true;
                return PoliceCallOutcome.TrustLost;
            }

            return PoliceCallOutcome.WrongEvidence;
        }
    }
}
