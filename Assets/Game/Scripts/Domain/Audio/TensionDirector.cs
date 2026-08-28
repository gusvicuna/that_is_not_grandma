namespace Game.Domain
{
    public class TensionDirector
    {
        private float _lieMotifSeconds;
        private float _lieMotifTimer;

        public TensionLevel Level { get; private set; }

        public TensionDirector(float lieMotifSeconds)
        {
            if (lieMotifSeconds <= 0f)
                throw new System.ArgumentOutOfRangeException(nameof(lieMotifSeconds), lieMotifSeconds, "Lie motif seconds must be positive.");
            _lieMotifSeconds = lieMotifSeconds;
        }

        public void SetTension(TensionLevel level)
        {
            //throws if the level is invalid
            if (!System.Enum.IsDefined(typeof(TensionLevel), level))
                throw new System.ArgumentOutOfRangeException(nameof(level), level, "Invalid tension level.");
            Level = level;
        }

        public void PulseLieMotif()
        {
            _lieMotifTimer = _lieMotifSeconds;
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime < 0f)
                throw new System.ArgumentOutOfRangeException(nameof(deltaTime), deltaTime, "Delta time cannot be negative.");
            if (_lieMotifTimer > 0f)
                _lieMotifTimer -= deltaTime;
            if (_lieMotifTimer < 0f)
                _lieMotifTimer = 0f;
        }

        public bool IsLieMotifActive => _lieMotifTimer > 0f;

        public float GetTarget(MusicLayerId layer)
        {
            return layer switch
            {
                MusicLayerId.Bed => 1f,
                MusicLayerId.Approach => Level switch
                {
                    TensionLevel.Calm => 0f,
                    TensionLevel.Uneasy => 0.5f,
                    TensionLevel.Alert => 1f,
                    _ => throw new System.ArgumentOutOfRangeException(nameof(Level), Level, "Invalid tension level."),
                },
                MusicLayerId.Lie => IsLieMotifActive ? 1f : 0f,
                _ => throw new System.ArgumentOutOfRangeException(nameof(layer), layer, "Invalid music layer."),
            };
        }

    }
}
