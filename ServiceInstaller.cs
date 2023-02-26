using System.Configuration.Install;
using System.Reflection;

namespace WeatherReport
{
    public static class ServiceInstaller
    {
        private static readonly string _exePath =
            Assembly.GetExecutingAssembly().Location;
        public static bool InstallService()
        {
            try
            {
                ManagedInstallerClass.InstallHelper(
                    new string[] { _exePath });
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static bool UninstallService()
        {
            try
            {
                ManagedInstallerClass.InstallHelper(
                    new string[] { "/u", _exePath });
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
