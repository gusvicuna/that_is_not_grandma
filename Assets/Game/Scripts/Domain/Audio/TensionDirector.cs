using System;

namespace Game.Domain
{
    /// <summary>
    /// The rules that turn game state into music layer targets — the only place that knows what
    /// "Alert" sounds like.
    /// </summary>
    public class TensionDirector
    {
        private readonly float _lieMotifSeconds;
        private float _lieMotifTimer;

        public TensionLevel Level { get; private set; }

        public TensionDirector(float lieMotifSeconds)
        {
            if (lieMotifSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(lieMotifSeconds), lieMotifSeconds, "Lie motif seconds must be positive.");

            _lieMotifSeconds = lieMotifSeconds;
        }

        public bool IsLieMotifActive => _lieMotifTimer > 0f;

        public void SetTension(TensionLevel level)
        {
            if (!Enum.IsDefined(typeof(TensionLevel), level))
                throw new ArgumentOutOfRangeException(nameof(level), level, "Invalid tension level.");

            Level = level;
        }

        /// <summary>Starts or refreshes the motif, so two marked conversations don't cut it short.</summary>
        public void PulseLieMotif()
        {
            _lieMotifTimer = _lieMotifSeconds;
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime), deltaTime, "Delta time cannot be negative.");

            _lieMotifTimer = Math.Max(0f, _lieMotifTimer - deltaTime);
        }

        public float GetTarget(MusicLayerId layer) => layer switch
        {
            MusicLayerId.Bed => 1f,
            MusicLayerId.Approach => ApproachTarget(),
            MusicLayerId.Lie => IsLieMotifActive ? 1f : 0f,
            _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "Invalid music layer.")
        };

        private float ApproachTarget() => Level switch
        {
            TensionLevel.Calm => 0f,
            TensionLevel.Uneasy => 0.5f,
            TensionLevel.Alert => 1f,
            _ => throw new ArgumentOutOfRangeException(nameof(Level), Level, "Invalid tension level.")
        };
    }
}
