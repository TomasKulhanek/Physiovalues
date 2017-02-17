using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Web;
using FMUSingleNodeWrapper.Properties;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceHost;

namespace FMUSingleNodeWrapper
{
    class Program
    {
        static void Main(string[] args)
        {
            MyUtils.parseCommandLine( typeof(NodeServerParams),args);
            
            if (NodeServerParams.ArgumentsToContinue)
            {
                MyUtils.tryFindFile(typeof(NodeServerParams), "*.fmu", "ParamF");
                if (NodeServerParams.httphost.Length == 0) NodeServerParams.httphost = getCurrentIPAddress();
                startHttpServer();
                runHttpServer();
                stopHttpServer();
            } else
            {
                Console.WriteLine(Resources.Program_Main_Missing_required_parameter__type__h_for_help_);
                Console.ReadKey();
            }
        }

        private static void stopHttpServer()
        {
            unregisterRestWorker();
            appHost.Stop();
            //unregisterRestWorker();
        }

        private static void runHttpServer()
        {
            AppDomain.CurrentDomain.DomainUnload += new EventHandler(CurrentDomain_DomainUnload);
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);

            Console.WriteLine(Resources.Program_runHttpServer_Press_any_key_to_stop_the_server____);
            var registerThread = new Thread(reregisterSimulator);
            registerThread.Start();
            Console.ReadKey();
            registerThread.Abort();
        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            unregisterRestWorker();
            //throw new NotImplementedException();
        }

        static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            unregisterRestWorker();
            //throw new NotImplementedException();
        }

        private static AppHost appHost;

        //infinite loop of thread reregistering the worker each 30 minutes
        private static void reregisterSimulator()
        {
            watch();
            while (true) 
            {
                Thread.Sleep(50*60*1000); //sleep for 50 minutes
                if (NodeServerParams.sw.ElapsedMilliseconds > (40 * 60 * 1000)) //last work was done 4 minutes ago
                { 
                    OnChanged(null,null);
                }
                NodeServerParams.sw.Restart(); //reset the timer
            }            
        }

        private static void watch()
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = ".";
            watcher.NotifyFilter = NotifyFilters.LastWrite
                                   | NotifyFilters.FileName;
            watcher.Filter = "*.fmu";

            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnChanged);


            watcher.EnableRaisingEvents = true;
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            //no action, when the change is a new directory with *.fmu - created by driver

            if (e != null)
            {
                if (Directory.Exists(e.FullPath)) return;
                Console.WriteLine("change in directory detected or restart triggered on " + e.FullPath);
            }
            unregisterRestWorker();
            NodeServerParams.FMUFiles.Clear();
            MyUtils.tryFindFile(typeof(NodeServerParams), "*.fmu", "ParamF");
            registerEachFMU();
        }

        private static void startHttpServer()
        {
            //if hostname not set then detect automatically
            string localIP = NodeServerParams.httphost;
            //listening URL
            NodeServerParams.listeningOn = "http://"+localIP+":"+NodeServerParams.httpport+"/worker/";
            appHost = new AppHost();
            appHost.Init();
            Console.Write(Resources.Program_startHttpServer_AppHost_starting_to_listen_on__0_,NodeServerParams.listeningOn);
            appHost.Start(NodeServerParams.listeningOn);
            registerEachFMU(true);
            Console.WriteLine(Resources.Program_startHttpServer_AppHost_Created_at__0___listening_on__1_, DateTime.Now, NodeServerParams.listeningOn);
            //register worker on server
        }
        private static void registerEachFMU() { registerEachFMU(false); }

        private static void registerEachFMU(bool deletetempdir)
        {
            //string listeningOn = NodeServerParams.listeningOn;
            foreach (var FMUFile in NodeServerParams.FMUFiles)
            {
                Console.WriteLine(Resources.Program_startHttpServer_Loaded_model__0__, FMUFile);
                if (deletetempdir) try{Directory.Delete(NodeServerParams.GetTempDir(FMUFile),true);} catch(Exception e)
                {//ignoring this exception, probably another process use it
                }
                if (!NodeServerParams.FmuNamePath.ContainsKey(FMUFile)) NodeServerParams.FmuNamePath.Add(FMUFile, Directory.GetCurrentDirectory() + "/"+FMUFile);
                registerRESTWorker(NodeServerParams.listeningOn + "simulation/" + FMUFile + "/", FMUFile);
            }
        }

        private static string getCurrentIPAddress()
        {
            string localIP = "?";
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    Console.WriteLine(Resources.Program_getCurrentIPAddress_Detected_IP_address__0__, localIP);
                }
            }
            return localIP;
        }
        /*private static void registerRESTWorker()
        {
            registerRESTWorker(NodeServerParams.thisurl);
        }*/

        private static void registerRESTWorker(string URL, string mname)
        {
            //NodeServerParams.thisurl = URL;
            try
            {
                var client = new JsonServiceClient(NodeServerParams.MasterServiceWorkerURL);
                Worker response;
                if (!NodeServerParams.Id.ContainsKey(mname)) //register new 
                {
                    response =
                        client.Post(new Worker {ModelName = mname, RestUrl = URL, WorkerType = "FMU"});
                    NodeServerParams.Id[mname] = response.Id;
                } else
                { //try to reregister with same id
                    response =
                           client.Post(new Worker { ModelName = mname, RestUrl = URL, WorkerType = "FMU",Id = NodeServerParams.Id[mname]});                    
                }
                Console.WriteLine(Resources.Program_registerRESTWorker_Registered_worker_id__0__on__1__,response.Id,NodeServerParams.MasterServiceWorkerURL ); 
            } catch (Exception e)
            {
                //NodeServerParams.Id = 0;
                Console.WriteLine(Resources.Program_registerRESTWorker_Cannot_register_this_worker_on_URL_+NodeServerParams.MasterServiceWorkerURL+"\n"+e.Message);
            }
        }

        private static void unregisterRestWorker()
        {
            try
            {
                var client = new JsonServiceClient(NodeServerParams.MasterServiceWorkerURL);
                var workerids = NodeServerParams.Id.Values;
                foreach (var workerid in workerids)
                {
                    var response = client.Delete(new Workers(workerid));
                    Console.WriteLine(Resources.Program_degisterRESTWorker_Deregistered_worker_id__0__on__1_, workerid, NodeServerParams.MasterServiceWorkerURL);
                    Thread.Sleep(500);
                }
                //List<Worker> response = client.Delete(new Workers(NodeServerParams.Id.Values.ToArray()));
                //NodeServerParams.Id = response.;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(Resources.Program_degisterRESTWorker_Cannot_deregister_this_worker_on_URL__0__,NodeServerParams.MasterServiceWorkerURL);
            }
        }

        //DTO
        public class Workers : IReturn<List<Worker>>
        {
            public long[] Ids { get; set; }
            public Workers(params long[] ids)
            {
                this.Ids = ids;
            }
        }
        //DTO
        public class Worker : IReturn<Worker>
        {
            public long Id { get; set; }
            public string ModelName { get; set; }
            public string RestUrl { get; set; } //endpoint of web service - 
            public string WorkerType { get; set; } //the type of the worker - could be cloud/grid - REST web service, or boinc - SOAP web service
            public int Priority { get; set; } //for future usage
        }
    }
}
