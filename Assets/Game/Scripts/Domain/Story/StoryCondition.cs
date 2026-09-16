using System;
using System.Collections.Generic;

namespace Game.Domain
{
    /// <summary>
    /// The gate in front of a beat: story flags that must (or must not) be set, and the earliest
    /// day it may fire. Immutable.
    /// </summary>
    public class StoryCondition
    {
        private static readonly string[] _noFlags = new string[0];

        /// <summary>A condition that never blocks anything.</summary>
        public static readonly StoryCondition Always = new StoryCondition();

        public StoryCondition(
            IReadOnlyList<string> requiredFlags = null,
            IReadOnlyList<string> forbiddenFlags = null,
            int minDay = 0)
        {
            if (minDay < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minDay), "Min day cannot be negative.");
            }
            RequiredFlags = requiredFlags ?? _noFlags;
            ForbiddenFlags = forbiddenFlags ?? _noFlags;
            MinDay = minDay;
        }

        public IReadOnlyList<string> RequiredFlags { get; }

        public IReadOnlyList<string> ForbiddenFlags { get; }

        /// <summary>0 = any day.</summary>
        public int MinDay { get; }
    }
}
