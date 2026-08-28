using System;

namespace Game.Domain
{
    public class Crossfader
    {
        private readonly float _durationSeconds;
        private float _progress; // 0 = start of fade, 1 = fully faded in
        private string _incomingTrackId;
        private string _outgoingTrackId;

        public string IncomingTrackId => _incomingTrackId;
        // null once fade completes so callers can stop playing the outgoing source
        public string OutgoingTrackId => _progress >= 1f ? null : _outgoingTrackId;
        public float IncomingWeight => _progress;
        public float OutgoingWeight => 1f - _progress;
        public bool IsFading => _progress < 1f;

        public Crossfader(float durationSeconds)
        {
            if (durationSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(durationSeconds), "Must be positive.");
            _durationSeconds = durationSeconds;
            _progress = 1f; // silent and idle
        }

        public void To(string trackId)
        {
            if (trackId == _incomingTrackId) return;
            _outgoingTrackId = _incomingTrackId;
            _incomingTrackId = trackId;
            _progress = 1f - _progress; // outgoing keeps its current audible level
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime), "Must be non-negative.");
            _progress = Math.Min(1f, _progress + deltaTime / _durationSeconds);
        }
    }
}

