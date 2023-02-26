﻿using System;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace WeatherReport
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        private static async Task Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                var service1 = new WeatherService(args);
                service1.TestStartupAndStopAsync();
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