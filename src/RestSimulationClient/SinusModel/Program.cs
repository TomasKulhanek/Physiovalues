using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Test.CommandLineParsing;

namespace DymolaModel
{
    class Program
    {
        private static string HELP = "REST wrapper for Dymola model instance \n" +
                                     "DymolaModelInstance.exe [options] [modelparameters]\n" +
                                     "\n" +
                                     "where [options] can be:\n\n" +
                                     "  /help      this help\n  /meta={identifier in meta database} only metainformation (path to this exe) will be sent to REST web service\n"+
                                     "  /metacommand={command line}   simulation command line, which will be executed when no data are stored in REST web service, use only with /meta \n"+
                                     "  /restart   the data will be deleted and simulation started from initial dsin.txt\n"+
                                     "  /modelid={identifier in model database} simulation will be performed and result data will be sent to REST web service\n" +
                                     "  /start     the simulation will be start from initial dsin.txt\n"+
                                     "  /continue  the simulation will be continued from the previous simulation (dsfinal.txt)\n"+
                                     "  /resturl={url of rest web service}  default is http://localhost/websim/rest\n\n" +
                                     "  /readdata no simulation performed reads dsres.mat file and registers it on server\n"+
                                     "  /iterate={iteration time in format [d.]hh:mm[:ss[.ff]] iterate the simulation until the specified time is reached e.g. /iterate=1:30:15 iterates for 1 hour 30 minutes 15 seconds \n"+
                                     "  /small register model to non-persistent url, only for small models\n"+
                                     "\nwhere [modelparameters] can be\n" +
                                     "  /big register model to non-persistent url, only for small models\n" +
                                     "\nwhere [modelparameters] can be\n" +
                                     "  /{parameter name}={parameter value} before simulation the parameters from dsin.txt will be modified with the values \n";

        private static double iterationStep = 10;

        static void Main(string[] args)
        {
            //from http://blogs.msdn.com/b/ivo_manolov/archive/2008/12/17/9230331.aspx
            CommandLineDictionary d = CommandLineDictionary.FromArguments(args);
            //d.ParseArguments(args);


            //registers model meta information under the modelid into the server - no simulation is performed
            if (d.ContainsKey("help"))
            {
                Console.WriteLine(HELP);
                return;
            }
            if (d.ContainsKey("resturl"))
            {
                string myurl;
                if (d.TryGetValue("resturl", out myurl)) DymosimJSONProxy.SetRESTURL(myurl);
                Console.WriteLine("resturl" + myurl);
                //remove this to prevent parsing it through parameter files in dsintermediate.txt ...
                d.Remove("resturl");
            }
            if (d.ContainsKey("meta"))
            {
                string myid;

                if (d.TryGetValue("meta", out myid))
                {
                    string mycommand;
                    if (d.TryGetValue("metacommand", out mycommand))
                        DymosimJSONProxy.RegisterModelMeta(mycommand, myid);
                    else
                        DymosimJSONProxy.RegisterModelMeta(
                            System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName, myid);
                }
                else Console.WriteLine("needs parameter with id of the model e.g. /meta=ECMOModel");
                return;

            }
            if (d.ContainsKey("modelid"))
            {
                string myid;
                if (d.TryGetValue("modelid", out myid)) DymosimJSONProxy.ModelId = myid;
                //remove this to prevent parsing it through parameter files in dsintermediate.txt ...
                d.Remove("modelid");
            }
            //makes the dsintermediate.txt - starting point of this simulation instance - either from previous simulation to continue (dsfinal.txt) or from begining (dsin.txt)
            bool continueSimulation = false;
            if (d.ContainsKey("continue"))
            {
                File.Copy("dsfinal.txt", "dsintermediate.txt", true);
                //remove this to prevent parsing it through parameter files in dsintermediate.txt ...
                d.Remove("continue");
                continueSimulation = true;
            }
            if (d.ContainsKey("start") | !File.Exists("dsintermediate.txt"))
            {
                File.Copy("dsin.txt", "dsintermediate.txt", true);
                d.Remove("start");
            }
            if (d.ContainsKey("restart"))
            {
                File.Copy("dsin.txt", "dsintermediate.txt", true);
                DymosimJSONProxy.Delete();
                d.Remove("restart");
            }
            if (d.ContainsKey("small"))
            {
                DymosimJSONProxy.persistent = false;
                d.Remove("small");
            }
            if (d.ContainsKey("big"))
            {
                DymosimJSONProxy.persistent = true;
                d.Remove("small");
            }
            
            if (d.ContainsKey("readdata"))
            {
                DymosimJSONProxy.RegisterModel(continueSimulation);
                return;
            }
            bool iterate = false;
            string iterationTime = "1";
            if (d.ContainsKey("iterate"))
            {
                iterate = true;
                d.TryGetValue("iterate", out iterationTime);
                d.Remove("iterate");
            }
            //if (!File.Exists("dsintermediate"))

            HashSet<string> dsParameters = new HashSet<string>();
            dsParameters.Add("StartTime");
            dsParameters.Add("StopTime");
            dsParameters.Add("nInterval");

            DymosimFile.ParseSimulationFile("dsintermediate.txt", d, dsParameters);
                //,d["starttime"], d["stoptime"], d["ninterval"]);

            //DymosimJSONProxy.InitializeSimulator("dsintermediate.txt");            
            if (iterate)
            {
                IterateSimulation(continueSimulation,iterationTime);
            }
            else
                SimulationStep(continueSimulation);
        }

//            Console.WriteLine("registered to "+ DymosimJSONProxy.client.BaseUrl+" as model id "+DymosimJSONProxy.ModelId);
        

        static void IterateSimulation(bool continueSimulation, string iterationTime)
        {
            DateTime endTime = DateTime.Now.Add(TimeSpan.Parse(iterationTime));
            //DateTime startWaiting = DateTime.Now;
            while (DateTime.Now < endTime)
            {
                DateTime enditeration = DateTime.Now.AddSeconds(iterationStep);
                SimulationStep(continueSimulation);
                //after first run set it to true, append to current data stream 
                continueSimulation = true;
                File.Copy("dsfinal.txt", "dsintermediate.txt", true);

                DateTime startWaiting = DateTime.Now;
                if (startWaiting > endTime) break; //finish loop
                Console.WriteLine("waiting:"+(enditeration - startWaiting)+" ms. Current time:"+startWaiting+" end time:"+endTime);
                Thread.Sleep((enditeration - startWaiting).Duration());
            }
        }

        static void SimulationStep(bool continueSimulation)
        {
            DymosimJSONProxy.GetNextSimulationParameters();
            DymosimJSONProxy.RunDymolaSimulator("dsintermediate.txt");
            //the first run can continuesimulation true=data will append the current|or false=data will create new instance of data stream
            DymosimJSONProxy.RegisterModel(continueSimulation);
        }
    }
}
