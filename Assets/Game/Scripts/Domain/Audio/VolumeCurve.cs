namespace Game.Domain
{
    public static class VolumeCurve
    {
        public const float MinDecibels = -80f;
        public static float ToDecibels(float linear01) => linear01 switch
        {
            <= 0.0001f => MinDecibels,
            > 1f => 0f,
            _ => 20f * (float)System.Math.Log10(linear01),
        };

        public static float FromDecibels(float decibels)
        {
            return decibels <= MinDecibels ? 0f : (float)System.Math.Pow(10f, decibels / 20f);
        }
    }
}
