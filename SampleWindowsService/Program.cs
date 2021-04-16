using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Topshelf;
using log4net;
using log4net.Config;
using System.IO;
using System.Reflection;
using Topshelf;
using Topshelf.Quartz;
using Quartz; 

namespace SampleWindowService
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(hostConfig =>
            {
                var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
                XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
                log4net.Util.LogLog.InternalDebugging = true;
                hostConfig.UseLog4Net();

                hostConfig.SetDescription("Sample Windows Service");

                hostConfig.EnableServiceRecovery(serviceRecovery =>
                {
                    // first failure, 5 minute delay
                    serviceRecovery.RestartService(5);

                    // second failure, 10 minute delay
                    serviceRecovery.RestartService(5); 

                    // subsequent failures, 15 minute delay
                    serviceRecovery.TakeNoAction();
                });

                hostConfig.DependsOn("Spooler");
                hostConfig.DependsOnEventLog();

                hostConfig.UseAssemblyInfoForServiceInfo();

                hostConfig.StartAutomatically();

                /*Service (RunAsNetworkService), Local System (RunAsLocalSystem), 
                 * Local Service (RunAsLocalService) or a specified username and password (RunAs). 
                 * There is also an option available to prompt the user for an username and password during installation (RunAsPrompt).
                 * Just like previous customizations, the credentials can also be set from the command line when installing*/
                hostConfig.RunAsLocalSystem();

                hostConfig.Service<SampleService>(serviceConfig =>
                {

                    serviceConfig.WhenStarted(s => s.Start());
                    serviceConfig.WhenStopped(s => s.Stop());
                    serviceConfig.ConstructUsing(() => new SampleService());
                    serviceConfig.WhenPaused(s => s.Pause());
                    serviceConfig.WhenContinued(s => s.Resume());
                    serviceConfig.WhenShutdown(s => s.Shutdown());
                    serviceConfig.ScheduleQuartzJob(job =>
                            job.WithJob(() =>
                                    JobBuilder.Create<SampleJob>().Build())
                                    .AddTrigger(() => TriggerBuilder.Create() 
                                        .WithSimpleSchedule(b => b
                                            .WithIntervalInMinutes(10)
                                            .RepeatForever())
                                            .Build()));

                });
            });
        }
    }
}
