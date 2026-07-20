namespace DVHAnalysis
{
    public enum MetricUnit
    {
        Invalid,  // Indicates an invalid value
        Gy,
        cGy,
        cc,
        Percent,  // e.g., 39
        Fraction  // e.g., 0.39 (same value as above)
    }
}
