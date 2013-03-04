using System;
using System.Globalization;
using System.Linq;
using Microsoft.WindowsAzure;
using Quartz;
using Quartz.Impl;

namespace Brisebois.WindowsAzure.Jobs
{
    /// <summary>
    /// Details: 
    /// </summary>
    public class JobSchedule
    {
        private readonly IScheduler sched;

        public JobSchedule()
        {
            var schedFact = new StdSchedulerFactory();

            sched = schedFact.GetScheduler();
            sched.Start();
        }

        /// <summary>
        /// Will schedule jobs in Eastern Standard Time
        /// </summary>
        /// <param name="scheduleConfig">Setting Key from your CloudConfigurations, value format "hh:mm;hh:mm;"</param>
        /// <param name="jobType">must inherit from IJob</param>
        public void ScheduleDailyJob(string scheduleConfig, Type jobType)
        {
            ScheduleDailyJob(scheduleConfig, jobType, "Eastern Standard Time");
        }

        /// <param name="scheduleConfig">Setting Key from your CloudConfigurations, value format "hh:mm;hh:mm;"</param>
        /// <param name="jobType">must inherit from IJob</param>
        /// <param name="timeZoneId">See http://alexandrebrisebois.wordpress.com/2013/01/20/using-quartz-net-to-schedule-jobs-in-windows-azure-worker-roles/ for Ids</param>
        public void ScheduleDailyJob(string scheduleConfig, Type jobType, string timeZoneId)
        {
            var schedule = CloudConfigurationManager.GetSetting(scheduleConfig);
            if (schedule == "-")
                return;

            schedule.Split(';').Where(s => !string.IsNullOrWhiteSpace(s)).ToList().ForEach(h =>
            {
                var index = h.IndexOf(':');
                var hour = h.Substring(0, index);
                var minutes = h.Substring(index + 1, h.Length - (index + 1));

                var job = new JobDetailImpl(jobType.Name + hour + minutes, null,
                                            jobType);

                var trigger = TriggerBuilder.Create()
                                            .StartNow()
                                            .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(Convert.ToInt32(hour,CultureInfo.InvariantCulture),
                                                                                                   Convert.ToInt32(minutes, CultureInfo.InvariantCulture))
                                                                             .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById(timeZoneId)))
                                            .Build();

                sched.ScheduleJob(job, trigger);
            });
        }


        /// <summary>
        /// Will schedule jobs in Eastern Standard Time
        /// </summary>
        /// <param name="scheduleConfig">Setting Key from your CloudConfigurations, value format "hh:mm;hh:mm;"</param>
        /// <param name="jobType">must inherit from IJob</param>
        public void ScheduleWeeklyJob(string scheduleConfig, Type jobType)
        {
            ScheduleWeeklyJob(scheduleConfig, jobType, "Eastern Standard Time");
        }


        /// <param name="scheduleConfig">Setting Key from your CloudConfigurations, value format "hh:mm;hh:mm;"</param>
        /// <param name="jobType">must inherit from IJob</param>
        /// <param name="timeZoneId">See http://alexandrebrisebois.wordpress.com/2013/01/20/using-quartz-net-to-schedule-jobs-in-windows-azure-worker-roles/#more-658 for Ids</param>
        public void ScheduleWeeklyJob(string scheduleConfig, Type jobType, string timeZoneId)
        {
            var schedule = CloudConfigurationManager.GetSetting(scheduleConfig);

            schedule.Split(';').Where(s => !string.IsNullOrWhiteSpace(s)).ToList().ForEach(h =>
            {
                var index = h.IndexOf(':');
                var hour = h.Substring(0, index);
                var minutes = h.Substring(index + 1, h.Length - (index + 1));

                var job = new JobDetailImpl(jobType.Name + hour + minutes, null,
                                            jobType);

                var trigger = TriggerBuilder.Create()
                                            .StartNow()
                                            .WithSchedule(CronScheduleBuilder.WeeklyOnDayAndHourAndMinute(DayOfWeek.Monday, 
                                                                                                          Convert.ToInt32(hour, CultureInfo.InvariantCulture), 
                                                                                                          Convert.ToInt32(minutes, CultureInfo.InvariantCulture))
                                            .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById(timeZoneId)))
                                            .Build();

                sched.ScheduleJob(job, trigger);
            });
        }
    }
}
