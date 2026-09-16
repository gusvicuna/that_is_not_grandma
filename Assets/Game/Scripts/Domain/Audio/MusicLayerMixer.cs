using System;

namespace Game.Domain
{
    /// <summary>
    /// Current-vs-target weight per music layer, with asymmetric fades: music should sneak in faster
    /// than it leaves, or the change reads as a bug.
    /// </summary>
    public class MusicLayerMixer
    {
        private const float _snapThreshold = 0.001f;

        private readonly float _fadeInPerSecond;
        private readonly float _fadeOutPerSecond;
        private readonly float[] _currentWeights;
        private readonly float[] _targetWeights;

        public MusicLayerMixer(float fadeInPerSecond, float fadeOutPerSecond)
        {
            if (fadeInPerSecond <= 0f)
                throw new ArgumentOutOfRangeException(nameof(fadeInPerSecond), fadeInPerSecond, "Fade in per second must be positive.");
            if (fadeOutPerSecond <= 0f)
                throw new ArgumentOutOfRangeException(nameof(fadeOutPerSecond), fadeOutPerSecond, "Fade out per second must be positive.");

            _fadeInPerSecond = fadeInPerSecond;
            _fadeOutPerSecond = fadeOutPerSecond;

            int layerCount = Enum.GetValues(typeof(MusicLayerId)).Length;
            _currentWeights = new float[layerCount];
            _targetWeights = new float[layerCount];
        }

        public void SetTarget(MusicLayerId layer, float weight01)
        {
            _targetWeights[IndexOf(layer)] = Clamp01(weight01);
        }

        public float GetWeight(MusicLayerId layer)
        {
            return _currentWeights[IndexOf(layer)];
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime), deltaTime, "Delta time cannot be negative.");

            for (int i = 0; i < _currentWeights.Length; i++)
            {
                _currentWeights[i] = MoveToward(_currentWeights[i], _targetWeights[i], deltaTime);
            }
        }

        public void SnapToTargets()
        {
            Array.Copy(_targetWeights, _currentWeights, _targetWeights.Length);
        }

        private float MoveToward(float current, float target, float deltaTime)
        {
            float difference = target - current;

            if (difference > _snapThreshold)
            {
                return Math.Min(target, current + _fadeInPerSecond * deltaTime);
            }
            if (difference < -_snapThreshold)
            {
                return Math.Max(target, current - _fadeOutPerSecond * deltaTime);
            }
            return target;
        }

        private static int IndexOf(MusicLayerId layer)
        {
            if (!Enum.IsDefined(typeof(MusicLayerId), layer))
                throw new ArgumentOutOfRangeException(nameof(layer), layer, "Invalid music layer.");

            return (int)layer;
        }

        private static float Clamp01(float value)
        {
            if (value < 0f) return 0f;
            return value > 1f ? 1f : value;
        }
    }
}
