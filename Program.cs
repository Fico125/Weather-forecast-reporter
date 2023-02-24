using System;
using System.ServiceProcess;

namespace WeatherReport
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                WeatherService service1 = new WeatherService(args);
                service1.TestStartupAndStop();
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new WeatherService(args)
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
