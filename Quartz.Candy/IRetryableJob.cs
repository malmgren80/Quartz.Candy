using System;

namespace Quartz.Candy
{
    public interface IRetryableJob : IJob
    {
        int MaxNumberTries { get; }
        DateTime StartTimeRetryUtc { get; }
        DateTime? EndTimeRetryUtc { get; }
    }
}