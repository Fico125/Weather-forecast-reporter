using System;
using System.ServiceProcess;

namespace WeatherReport
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main(string[] args)
        {
            if (args != null && args.Length == 1 && args[0].Length > 1
                && (args[0][0] == '-' || args[0][0] == '/'))
            {
                switch (args[0].Substring(1).ToLower())
                {
                    default:
                        break;
                    case "install":
                    case "i":
                        ServiceInstaller.InstallService();
                        break;
                    case "uninstall":
                    case "u":
                        ServiceInstaller.UninstallService();
                        break;
                }
            }

            if (Environment.UserInteractive)
            {
                var service1 = new WeatherService();
                service1.TestStartupAndStop();
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new WeatherService()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}