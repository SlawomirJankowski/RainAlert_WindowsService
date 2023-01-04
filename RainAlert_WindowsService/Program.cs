using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace RainAlert_WindowsService
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 

        static void Main(string[] args)
        {
            if (Environment.UserInteractive) // żeby łatwiej instalować usługę z exe + parametr
            {
                if (args[0] == "--install")
                {
                    ManagedInstallerClass.InstallHelper(new[] { Assembly.GetExecutingAssembly().Location });
                }
                else if (args[0] == "--uninstall")
                {
                    ManagedInstallerClass.InstallHelper(new[] { "/u", Assembly.GetExecutingAssembly().Location });
                }
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new RainAlert()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
