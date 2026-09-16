using System;
using System.Collections.Generic;

namespace Game.Domain
{
    /// <summary>
    /// The story's rules table. It is told what happened and answers which beats fired — nothing
    /// else. Applying a beat's effects is Presentation's job, which is why this whole class runs
    /// in an EditMode test without a scene.
    /// </summary>
    public class StoryDirector
    {
        private static readonly string[] _nothingFired = new string[0];

        private readonly List<StoryBeat> _beats = new();
        private readonly HashSet<string> _firedBeatIds = new(StringComparer.Ordinal);
        private readonly HashSet<string> _flags = new(StringComparer.Ordinal);

        public StoryDirector(IEnumerable<StoryBeat> beats)
        {
            if (beats == null)
            {
                throw new ArgumentNullException(nameof(beats));
            }
            var seenIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (StoryBeat beat in beats)
            {
                if (beat == null)
                {
                    throw new ArgumentException("The beat list contains a null beat.", nameof(beats));
                }
                if (!seenIds.Add(beat.Id))
                {
                    throw new ArgumentException($"Duplicate beat id '{beat.Id}'.", nameof(beats));
                }
                _beats.Add(beat);
            }
        }

        /// <summary>0 until the first DayStarted event.</summary>
        public int CurrentDay { get; private set; }

        public bool HasFlag(string flag)
        {
            return _flags.Contains(flag);
        }

        public void SetFlag(string flag)
        {
            if (string.IsNullOrWhiteSpace(flag))
            {
                throw new ArgumentException("Flag cannot be null, empty or whitespace.", nameof(flag));
            }
            _flags.Add(flag);
        }

        public bool HasFired(string beatId)
        {
            return _firedBeatIds.Contains(beatId);
        }

        /// <summary>
        /// Ids of every beat that fires on this event, in declaration order. Flags set by those
        /// beats' effects land after this returns, so a beat can never trigger another one within
        /// the same call — deliberate, and the reason the returned order is the only thing to
        /// reason about.
        /// </summary>
        public IReadOnlyList<string> Notify(StoryEvent evt)
        {
            if (evt.Trigger == StoryTrigger.DayStarted)
            {
                CurrentDay = evt.Number;
            }

            List<string> fired = null;
            foreach (StoryBeat beat in _beats)
            {
                if (!CanFire(beat, evt))
                {
                    continue;
                }
                _firedBeatIds.Add(beat.Id);
                fired ??= new List<string>();
                fired.Add(beat.Id);
            }
            return fired ?? (IReadOnlyList<string>)_nothingFired;
        }

        private bool CanFire(StoryBeat beat, StoryEvent evt)
        {
            if (!beat.Repeatable && _firedBeatIds.Contains(beat.Id))
            {
                return false;
            }
            if (!beat.Matches(evt))
            {
                return false;
            }
            return MeetsCondition(beat.Condition);
        }

        private bool MeetsCondition(StoryCondition condition)
        {
            if (CurrentDay < condition.MinDay)
            {
                return false;
            }
            for (int i = 0; i < condition.RequiredFlags.Count; i++)
            {
                if (!_flags.Contains(condition.RequiredFlags[i]))
                {
                    return false;
                }
            }
            for (int i = 0; i < condition.ForbiddenFlags.Count; i++)
            {
                if (_flags.Contains(condition.ForbiddenFlags[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
