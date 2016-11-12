using System;
using System.Linq;
using System.ServiceProcess;

namespace AutomaticReminderCommon
{
    public static class ServiceAuxiliary
    {
        public static bool ServiceExist(string serviceName)
        {
            ServiceController[] services = ServiceController.GetServices();
            var service =
                services.FirstOrDefault(
                    s =>
                        s.ServiceName.ToLower().Equals(serviceName.ToLower(), StringComparison.CurrentCultureIgnoreCase));
            return service != null;
        }

        public static void StartService(string serviceName, int timeoutMilliseconds)
        {
            var service = new ServiceController(serviceName);
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            catch (Exception e)
            {
                Logger.LogFormat("StartService: {0}",e.Message);
            }
        }

        public static void StopService(string serviceName, int timeoutMilliseconds)
        {
            var service = new ServiceController(serviceName);
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
            }
            catch (Exception e)
            {
                Logger.LogFormat("StopService: {0}", e.Message);
            }
        }
    }
}