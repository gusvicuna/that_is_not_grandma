using System;

namespace Game.Domain
{
    /// <summary>
    /// The linear-slider ↔ decibel conversion, in one place: hearing is logarithmic, so a slider
    /// wired straight to decibels is useless over most of its travel.
    /// </summary>
    public static class VolumeCurve
    {
        public const float MinDecibels = -80f;

        private const float _silenceThreshold = 0.0001f;

        public static float ToDecibels(float linear01) => linear01 switch
        {
            <= _silenceThreshold => MinDecibels,
            > 1f => 0f,
            _ => 20f * (float)Math.Log10(linear01)
        };

        public static float FromDecibels(float decibels)
        {
            if (decibels <= MinDecibels)
            {
                return 0f;
            }
            return Math.Min(1f, (float)Math.Pow(10f, decibels / 20f));
        }
    }
}
