namespace NHMCore.Mining
{
    public enum AlgorithmStatus
    {
        MissingSMA = 0,
        NoBenchmark,
        Benchmarked,
        ReBenchmark,
        Unprofitable,
        // pending states
        BenchmarkPending,
        Benchmarking,
        // errors
        ErrorNegativeSMA = 1000,
        ErrorBenchmark,
        Error // ???
    }
}
