namespace RealVis.Visualization
{
    /// <summary>
    /// Decibel scaling converts the frequencies to decibel levels.
    /// Linear scaling increases higher frequencies with a linear regression.
    /// Sqrt scaling uses square root to create a balanced spectrum of frequencies.
    /// </summary>
    public enum ScalingStrategy
    {
        Decibel,
        Linear,
        Sqrt
    }
}
