namespace esm
{

    public enum TemperatureMode
    {
        Fixed,
        Seasonal,
        Annual
    }

    public static class Config
    {
        // Mod config menu
        public const TemperatureMode        DefaultTargetMode = TemperatureMode.Annual;
        public const float                  DefaultFixedTarget = 10.0f;

        public static TemperatureMode       TargetMode = DefaultTargetMode;
        public static float                 FixedTarget = DefaultFixedTarget;

    }

}
