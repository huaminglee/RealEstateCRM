using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OUDAL;
using Quartz;
using Quartz.Impl;
using Common.Logging;
using Quartz.Impl.Triggers;

namespace RMIS.Web.BLL.SynchronismWorkflow
{
    public class DepartUserView
    {
        public int DepartmentId;
        public int UserId;
 
    }
    public class SynchronismWorkflow
    {  
        public string  Import()
        {
            return "";

        }
    }
    public class SynchronishJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            //SynchronismWorkflow sw = new SynchronismWorkflow();
            //string result = sw.Import();
            //LogHelper.LogHelper.Info(result);
        }
        public static void Schedule()
        {
            ISchedulerFactory factory = new StdSchedulerFactory();
            IScheduler schedule = factory.GetScheduler();
            schedule.Start();
            IJobDetail job = JobBuilder.Create<SynchronishJob>().WithIdentity("SynochronishOU", "JobGroup1").Build();

            SimpleTriggerImpl trigger = new SimpleTriggerImpl();
            trigger.RepeatInterval =new System.TimeSpan(2,0,0);
            trigger.StartTimeUtc = DateTime.UtcNow;
            trigger.Name = "Syn";
            schedule.ScheduleJob(job, trigger);
        }
    }
   
}
