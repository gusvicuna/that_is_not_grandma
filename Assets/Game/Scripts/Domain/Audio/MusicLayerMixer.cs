namespace Game.Domain
{
    public class MusicLayerMixer
    {
        public float BedLayer { get; private set; }
        public float ApproachLayer { get; private set; }
        public float LieLayer { get; private set; }
        private readonly float _fadeInPerSecond;
        private readonly float _fadeOutPerSecond;
        private float _bedTarget;
        private float _approachTarget;
        private float _lieTarget;

        public MusicLayerMixer(float fadeInPerSecond, float fadeOutPerSecond)
        {
            if (fadeInPerSecond <= 0f)
                throw new System.ArgumentOutOfRangeException(nameof(fadeInPerSecond), fadeInPerSecond, "Fade in per second must be positive.");
            if (fadeOutPerSecond <= 0f)
                throw new System.ArgumentOutOfRangeException(nameof(fadeOutPerSecond), fadeOutPerSecond, "Fade out per second must be positive.");
            _fadeInPerSecond = fadeInPerSecond;
            _fadeOutPerSecond = fadeOutPerSecond;
        }

        public void SetTarget(MusicLayerId layer, float weight01)
        {
            if (weight01 < 0f) weight01 = 0f;
            else if (weight01 > 1f) weight01 = 1f;

            switch (layer)
            {
                case MusicLayerId.Bed:
                    _bedTarget = weight01;
                    break;
                case MusicLayerId.Approach:
                    _approachTarget = weight01;
                    break;
                case MusicLayerId.Lie:
                    _lieTarget = weight01;
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(layer), layer, null);
            }
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime < 0f)
                throw new System.ArgumentOutOfRangeException(nameof(deltaTime), deltaTime, "Delta time cannot be negative.");

            BedLayer = MoveToward(BedLayer, _bedTarget, deltaTime);
            ApproachLayer = MoveToward(ApproachLayer, _approachTarget, deltaTime);
            LieLayer = MoveToward(LieLayer, _lieTarget, deltaTime);
        }

        public float GetWeight(MusicLayerId layer)
        {
            return layer switch
            {
                MusicLayerId.Bed => BedLayer,
                MusicLayerId.Approach => ApproachLayer,
                MusicLayerId.Lie => LieLayer,
                _ => throw new System.ArgumentOutOfRangeException(nameof(layer), layer, null),
            };
        }

        public void SnapToTargets()
        {
            BedLayer = _bedTarget;
            ApproachLayer = _approachTarget;
            LieLayer = _lieTarget;
        }

        private float MoveToward(float current, float target, float deltaTime)
        {
            float diff = target - current;
            if (diff > 0.001f)
            {
                float next = current + _fadeInPerSecond * deltaTime;
                return (next > target) ? target : next;
            }
            if (diff < -0.001f)
            {
                float next = current - _fadeOutPerSecond * deltaTime;
                return (next < target) ? target : next;
            }
            return target;
        }
    }
}
