namespace Spartha.Data
{
    public enum SparkFamily
    {
        Canine,
        Feline,
        Bird,
        Rabbit,
        Reptile,
        Dragon
    }

    public enum TrustTier
    {
        Unbroken,    // 0-19: 50% disobey, may attack self
        Acknowledged, // 20-39: 20% disobey
        Aligned,      // 40-59: 10% disobey, evo gate
        Anchored,     // 60-79: always obeys, evo trigger
        Resonant      // 80-99: always obeys, decay resist, Last Stand
    }

    public enum VitalityState
    {
        Healthy,   // TP 0-29
        Stressed,  // TP 30-49
        Critical,  // TP 50-69
        Broken,    // TP 70-89
        BrokenPlus // TP 90-99
    }
}
