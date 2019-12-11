namespace NHMCore.Mining
{
    public enum AlgorithmStatus
    {
        Disabled = 0,
        MissingSMA,
        NoBenchmark,
        Benchmarked,
        ReBenchmark,
        Unprofitable,
        // pending states
        BenchmarkPending,
        Benchmarking,
        Mining,
        // errors
        ErrorNegativeSMA = 1000,
        ErrorBenchmark,
        Error // ???
    }
}
