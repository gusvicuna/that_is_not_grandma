using System;

namespace Game.Domain
{
    /// <summary>
    /// One authored rule: when this happens and the conditions hold, the beat fires. What it then
    /// *does* is not here — effects are data applied by Presentation. Immutable.
    /// </summary>
    public class StoryBeat
    {
        /// <summary>Wildcard for <see cref="MatchNumber"/>.</summary>
        public const int AnyNumber = -1;

        public StoryBeat(
            string id,
            StoryTrigger trigger,
            string matchPrimaryId = null,
            string matchSecondaryId = null,
            int matchNumber = AnyNumber,
            StoryCondition condition = null,
            bool repeatable = false)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Beat id cannot be null, empty or whitespace.", nameof(id));
            }
            Id = id;
            Trigger = trigger;
            MatchPrimaryId = matchPrimaryId;
            MatchSecondaryId = matchSecondaryId;
            MatchNumber = matchNumber;
            Condition = condition ?? StoryCondition.Always;
            Repeatable = repeatable;
        }

        public string Id { get; }

        public StoryTrigger Trigger { get; }

        /// <summary>Null or empty = any clue / item / dialogue.</summary>
        public string MatchPrimaryId { get; }

        /// <summary>Null or empty = any NPC.</summary>
        public string MatchSecondaryId { get; }

        /// <summary><see cref="AnyNumber"/> = any room / day / outcome.</summary>
        public int MatchNumber { get; }

        public StoryCondition Condition { get; }

        /// <summary>False = fires at most once per run.</summary>
        public bool Repeatable { get; }

        /// <summary>Trigger and payload only — day, flags and "already fired" belong to the director.</summary>
        public bool Matches(StoryEvent evt)
        {
            if (evt.Trigger != Trigger)
            {
                return false;
            }
            if (!MatchesId(MatchPrimaryId, evt.PrimaryId))
            {
                return false;
            }
            if (!MatchesId(MatchSecondaryId, evt.SecondaryId))
            {
                return false;
            }
            return MatchNumber == AnyNumber || MatchNumber == evt.Number;
        }

        private static bool MatchesId(string expected, string actual)
        {
            return string.IsNullOrEmpty(expected) || string.Equals(expected, actual, StringComparison.Ordinal);
        }
    }
}
