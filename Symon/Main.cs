using System;
using System.Configuration.Install;
using System.Diagnostics;
using System.Reflection;
using System.ServiceProcess;

namespace Symon
{
    class SymonMain
    {
        public const string serviceName = "SymonService";

        #region static method Main

        /// <summary>
        /// Application main entry point.
        /// </summary>
        /// <param name="args">Command line argumnets.</param>
        public static void Main(string[] args)
        {
            try
            {
                if (args.Length > 0 && args[0].ToLower() == "/i")
                {
                    ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });

                    ServiceController c = new ServiceController(serviceName);
                    c.Start();
                }
                else if (args.Length > 0 && args[0].ToLower() == "/u")
                {
                    ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                }
                else if (args.Length > 0 && args[0].ToLower() == "/test")
                {
                    using (DataSender sender = new DataSender())
                    {
                        sender.Start();

                        Console.WriteLine("The {0} is ready.", serviceName);
                        Console.WriteLine("Press <ENTER> to terminate service.");
                        Console.WriteLine();
                        Console.ReadLine();

                        sender.Stop();
                    }
                }
                else if (args.Length > 0 && args[0].ToLower() == "/list")
                {
                    Console.WriteLine(IFInfo.getNetIF());

                    return;
                }
                else
                {
                    ServiceBase[] servicesToRun = new ServiceBase[] { new SymonService() };
                    ServiceBase.Run(servicesToRun);
                }
            }
            catch (Exception x)
            {
                Trace.TraceError("Error: {0}", x.ToString());
            }
        }

        #endregion

    }
}
