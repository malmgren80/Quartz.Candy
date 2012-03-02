using System;

namespace Quartz.Candy
{
    public class RetryableJobListener : IJobListener
    {
        public const string NumberTriesJobDataMapKey = "RetryableJobListener.TryNumber";

        private readonly IScheduler _scheduler;

        public RetryableJobListener(IScheduler scheduler)
        {
            if (scheduler == null) throw new ArgumentNullException("scheduler");

            _scheduler = scheduler;
        }

        public void JobToBeExecuted(JobExecutionContext context)
        {
            var retryableJob = context.JobInstance as IRetryableJob;
            if (retryableJob == null)
                return;

            if (!context.JobDetail.JobDataMap.Contains(NumberTriesJobDataMapKey))
                context.JobDetail.JobDataMap[NumberTriesJobDataMapKey] = 0;

            int numberTries = context.JobDetail.JobDataMap.GetIntValue(NumberTriesJobDataMapKey);
            context.JobDetail.JobDataMap[NumberTriesJobDataMapKey] = ++numberTries;
        }

        public void JobExecutionVetoed(JobExecutionContext context)
        {
        }

        public void JobWasExecuted(JobExecutionContext context, JobExecutionException jobException)
        {
            if (jobException == null)
                return; 

            var retryableJob = context.JobInstance as IRetryableJob;
            if (retryableJob == null)
                return; 

            int numberTries = context.JobDetail.JobDataMap.GetIntValue(NumberTriesJobDataMapKey);
            if (numberTries >= retryableJob.MaxNumberTries)
                return; // Max number tries reached

            // Schedule next try
            ScheduleRetryableJob(context, retryableJob);
        }

        private void ScheduleRetryableJob(JobExecutionContext context, IRetryableJob retryableJob)
        {
            var oldTrigger = context.Trigger;

            // Unschedule old trigger
            _scheduler.UnscheduleJob(oldTrigger.Name, oldTrigger.Group);

            // Create and schedule new trigger
            var retryTrigger = new SimpleTrigger(oldTrigger.Name, oldTrigger.Group, retryableJob.StartTimeRetryUtc, retryableJob.EndTimeRetryUtc, 0, TimeSpan.Zero);
            _scheduler.ScheduleJob(context.JobDetail, retryTrigger);
        }

        public string Name
        {
            get { return this.GetType().FullName; }
        }
    }
}
