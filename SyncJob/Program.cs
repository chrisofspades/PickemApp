using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using Quartz.Impl;
using System.Net;

namespace SyncJob
{
    class Program
    {
        static void Main(string[] args)
        {
            // construct a scheduler factory
            ISchedulerFactory schedFact = new StdSchedulerFactory();

            // get a scheduler
            IScheduler sched = schedFact.GetScheduler();
            sched.Start();

            var job = JobBuilder.Create<SyncWithNFL>().Build();

            var trigger = TriggerBuilder.Create()
                            .WithSimpleSchedule(x => x.WithIntervalInSeconds(30).RepeatForever())
                            .Build();

            sched.ScheduleJob(job, trigger);

            Console.ReadLine();
        }
    }

    public class SyncWithNFL : IJob
    {
        public SyncWithNFL()
        {
        }

        public void Execute(IJobExecutionContext context)
        {
            Console.WriteLine("Executing " + DateTime.Now.ToString());

            //this is bad. should not hardcode domain here.
            string domain = "rcpickem.apphb.com";
#if DEBUG
            domain = "localhost:64848";
#endif

            // Initialize the WebRequest.
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create("http://" + domain + "/Home/Sync?x=http://www.nfl.com/liveupdate/scorestrip/ss.xml");

            // Return the response. 
            HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();

            // Code to use the WebResponse goes here. 
            if (myResponse.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine("Successfully synced at " + DateTime.Now.ToString());
            }
            else
            {
                Console.WriteLine("Failed at " + DateTime.Now.ToString());
            }
            // Close the response to free resources.
            myResponse.Close();
        }
    }
}
