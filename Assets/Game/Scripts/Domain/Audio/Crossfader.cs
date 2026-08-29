using System;

namespace Game.Domain
{
    /// <summary>
    /// Two-slot ambience fade. A new track always enters through the same door, so re-entering a
    /// room mid-fade can't stack a third source or restart what is already playing.
    /// </summary>
    public class Crossfader
    {
        private readonly float _durationSeconds;
        private string _outgoingTrackId;

        /// <summary>0 = the incoming track is silent, 1 = it fully replaced the outgoing one.</summary>
        private float _progress;

        public Crossfader(float durationSeconds)
        {
            if (durationSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(durationSeconds), durationSeconds, "Duration must be positive.");

            _durationSeconds = durationSeconds;
            _progress = 1f;
        }

        public string IncomingTrackId { get; private set; }

        /// <summary>Null once the fade completes, so callers know when to stop that source.</summary>
        public string OutgoingTrackId => _progress >= 1f ? null : _outgoingTrackId;

        public float IncomingWeight => _progress;
        public float OutgoingWeight => 1f - _progress;
        public bool IsFading => _progress < 1f;

        /// <summary>Null is legal and means silence.</summary>
        public void To(string trackId)
        {
            if (trackId == IncomingTrackId)
            {
                return;
            }

            _outgoingTrackId = IncomingTrackId;
            IncomingTrackId = trackId;

            // Restarting from the inverse keeps the previously incoming track at its exact
            // audible level as it becomes the outgoing one: no jump, no pop.
            _progress = 1f - _progress;
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime), deltaTime, "Delta time cannot be negative.");

            _progress = Math.Min(1f, _progress + deltaTime / _durationSeconds);
        }
    }
}
