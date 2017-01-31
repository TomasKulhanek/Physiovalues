using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributeSimulationService
{
    class Program
    {
        static void Main(string[] args)
        {
            MyUtils.parseCommandLine(typeof(NodeServerParams), args);
            startHttpServer();
            runHttpServer();
            stopHttpServer();
        }

        private static void stopHttpServer()
        {
            appHost.Stop();
        }

        private static void runHttpServer()
        {
            Console.WriteLine("Press any key to stop the server ...");
            Console.ReadKey();

        }

        private static AppHost appHost;
        private static void startHttpServer()
        {
            var listeningOn = "http://localhost:" + NodeServerParams.httpport + "/";
            appHost = new AppHost();
            appHost.Init();
            appHost.Start(listeningOn);
            Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, listeningOn);
        }
    }
}
