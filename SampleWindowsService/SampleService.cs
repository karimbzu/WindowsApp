using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using log4net; 
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Data;  
using Quartz; 
using System.Reflection;
using System.Configuration;

namespace SampleWindowService
{
    public class SampleService
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(SampleService));

        // Boolean to determine if trigger should reoccur
        public volatile bool _requestServiceStop = false;


        public void Start()
        {
            _log.Info("Starting...");


            _log.Info("Started succesfully.");
        }
        public void Stop()
        {
            _log.Info("Stopping...");
            _requestServiceStop = true;

            _log.Info("Stopped succesfully.");
        }
        public void Pause()
        {
            _log.Info("Pause Service...");

            _log.Info("Pause Service succesfully.");
        }

        public void Resume()
        {
            _log.Info("Resuming Service...");

            _log.Info("Resume Service succesfully.");
        }

        public void Shutdown()
        {
            _log.Info("Shutting down...");

            _log.Info("Shutdown succesfully.");
        }


    }

    public class SampleJob : IJob
    { 
        private static readonly ILog _log = LogManager.GetLogger(typeof(SampleJob));
        
        private int MaxProcessor = Convert.ToInt32(ConfigurationManager.AppSettings["MaxProcessor"].ToString());
        private string ProcessName = ConfigurationManager.AppSettings["ProcessName"].ToString();
        private string ProcessFileName= ConfigurationManager.AppSettings["ProcessFileName"].ToString();
        private string ProcessPath = ConfigurationManager.AppSettings["ProcessPath"].ToString();
        public void Execute(IJobExecutionContext context)
        {

            // Log Process Start 
            _log.Info($"Sample Service Start - {DateTime.Now}"); 
            //if (!IsLock)
            {
                try
                {
                    ScheduleProcess(ProcessName, MaxProcessor,ProcessFileName,ProcessPath);
                }
                catch (Exception ex)
                {
                    _log.Info("Sample Scheduler -" + ex.Message.ToString());
                } 
            }            
            _log.Info($"Sample Service End - {DateTime.Now}");
        }

         
        public void ScheduleProcess(string ProcessName, int MaxProcessor, string ProcessFileName, string ProcessWorkingDirectory)
        {
            int ProcessCount = Process.GetProcessesByName(ProcessName).Count<Process>();

            //process running in the background reaches maximum limit then will process in next schedule 
            if (ProcessCount < MaxProcessor)
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.FileName = ProcessFileName;
                processStartInfo.WorkingDirectory = Path.GetDirectoryName(ProcessWorkingDirectory);
                Process result = null;
                try
                {
                    result = Process.Start(processStartInfo);
                }
                catch (Exception ex)
                {
                    _log.Info("Windows Sevice Scheduler - " + ex.Message.ToString());
                }
                if (processStartInfo == null)
                {
                    _log.Info("Windows Sevice Scheduler - Unknown Error");
                }
                ProcessCount++;
            }
            else
            {
                _log.Info("Windows Sevice Scheduler - Maximum number of process");
            }
        }
    }

}
