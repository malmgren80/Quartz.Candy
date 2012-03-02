using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz.Impl;

namespace Quartz.Candy.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ISchedulerFactory schedFact = new StdSchedulerFactory();
            IScheduler scheduler = schedFact.GetScheduler();
            IJobListener retryableJobListener = new RetryableJobListener(scheduler);
            scheduler.AddGlobalJobListener(retryableJobListener);
            scheduler.Start();

            // construct job info
            JobDetail jobDetail = new JobDetail("DummyJob", null, typeof(DummyJob));
            
            // fire only once
            Trigger trigger = TriggerUtils.MakeImmediateTrigger(0, TimeSpan.Zero);
            trigger.Name = "Chuck Norris";
            
            // start
            scheduler.ScheduleJob(jobDetail, trigger); 
        }
    }

    public class DummyJob : IRetryableJob
    {
        public void Execute(JobExecutionContext context)
        {
            int tryNumber = context.JobDetail.JobDataMap.GetIntValue(RetryableJobListener.NumberTriesJobDataMapKey);
            Console.WriteLine("{0} - Executing dummy job. This is try number {1} of {2}", DateTime.UtcNow, tryNumber, MaxNumberTries);

            throw new JobExecutionException("This job is too dumb to execute!");
        }

        public int MaxNumberTries
        {
            get { return 3; }
        }

        public DateTime StartTimeRetryUtc
        {
            get { return DateTime.UtcNow.AddSeconds(10); }
        }

        public DateTime? EndTimeRetryUtc
        {
            get { return null; /* Don't bother about end time */ }
        }
    }
}
