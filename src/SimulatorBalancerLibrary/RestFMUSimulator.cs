using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NLog;
using RestMasterService.ComputationNodes;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceHost;
//
//using System;
using System.Linq;
using PostSharp.Patterns.Diagnostics;
using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
//using PostSharp.Extensibility;
/*using System.Configuration;
using System.Collections.Generic;
//using RestMasterService.WebApp;
using ServiceStack.Configuration;
using ServiceStack.Common;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.ServiceInterface.ServiceModel;
using ServiceStack.WebHost.Endpoints;
*/

namespace SimulatorBalancerLibrary
{
    public class RestFMUSimulator : ISimulatorBehavior
    {
        public string[] RestURLs;
        private string updateIdentifyProcessURL;
        private string updateWorkerListURL;
        private string ModelName;
        private long computationcycle = 0;
        private long computationtime;
        private long lastcomputationcycle= 0 ;
        private long identid;
        private Stopwatch stopwatch=new Stopwatch();
        private Logger logger = LogManager.GetLogger("RestFMUSimulator");
        public RestFMUSimulator(string modelName,string updateURL)
        {
            ModelName = modelName;
            //MasterServeice host both worker list service and identify processes service
            updateWorkerListURL = updateURL;
            updateIdentifyProcessURL = updateURL;
            computationcycle = 0;
            computationtime = 0;
            identid = 0;
//            RestURLs = getWorkerURLs(modelName);
        }


        
        public double[][][] Simulate(string[] parameternames, double[][] parametervalues, string[] variablenamesinresult, double[] timepoints)
        {
            if ((RestURLs==null) || (RestURLs.Length<1))
            {
                getWorkerURLs(this.ModelName);
                if (RestURLs.Length < 1)
                    throw new Exception("No workers accesible via RestURL to compute the simulation.");
            }
        

            if (parametervalues[0].Length != parameternames.Length)
                throw new Exception("Number of parameters mismatch. Parameter names is " + parameternames.Length +
                                    ",but values in one parameter row is " + parametervalues[0].Length);
            //if (parameternames.Length != 4) throw new Exception("expected 4 parameters for this specific model");
            //if (variablenamesinresult.Length != 2) throw new Exception("expected 2 variables in result for this specific model");
            //howmany independent computation should be done
            var result = new double[parametervalues.Length][][]; //parametervalues.Length x timepoints.Length x variablenamesinresult.Length
            //fix bug of "Time    " not found in simulator, MATLAB MWArray adds some whitespace characters.
            for (int i=0;i<variablenamesinresult.Length; i++)
            {
                variablenamesinresult[i] = variablenamesinresult[i].Trim();
            }
            for (int i = 0; i < parameternames.Length; i++)
            {
                parameternames[i] = parameternames[i].Trim();
            }
            //serial computation
            /*
            for (int j = 0; j < parametervalues.Length; j++)
            {
                //allocate array for computation instance
                //result[j] = new double[timepoints.Length][];

                //computation specific for model
                result[j] = Compute(RestURLs[0],parameternames,parametervalues[j],timepoints,variablenamesinresult);
            }*/
            //parallel computation
            int j = 0;
            var sw = new Stopwatch();
            sw.Start();
            while (j<parametervalues.Length)
            {

                if (RestURLs.Length == 1) //sequential computing for 1 worker
                {
                    result[j] = Compute(RestURLs[0], parameternames, parametervalues[j], timepoints,
                                        variablenamesinresult);
                    j++;
                    computationcycle++;
                }
                else //parallel computing 
                {                    
                    //TODO introduce balancerbehavior as a strategy pattern to change algorithm
                    int howmanythreads = Math.Min(RestURLs.Length, parametervalues.Length - j);
                    //int j1 = j;
                    int l2 = parametervalues.Length/howmanythreads; //length of each parallel loop
                    int l3 = parametervalues.Length%howmanythreads; // modulo
                    int l4 = parametervalues.Length / howmanythreads; //= parametervalues.Length % l2;
                    if (l3>0) { l2++; l4=parametervalues.Length % l2;}
                    
                    
                    Parallel.For(0, howmanythreads, k =>
                                                        {
                        if ((k-1)==howmanythreads) //last loop
                        {
                            
                                for (int k3 = 0; k3 < l4; k3++)
                                    result[j + k*l2 + k3] = Compute(RestURLs[k], parameternames,
                                                                    parametervalues[j + k*l2 + k3], timepoints,
                                                                    variablenamesinresult);
                            
                        }
                        else
                        {

                            for (int k2 = 0; k2 < l2;k2++ )
                                result[j + k * l2 + k2] = Compute(RestURLs[k], parameternames, parametervalues[j + k * l2 + k2], timepoints,
                                                        variablenamesinresult);
                        }
                                                        });
                    //TODO handle exceptions
                    if (repeatComp) repeatComputation();
                    j += (howmanythreads - 1) * l2 + l4; //TODO test whether (howmanythreads-1)*l2+l4 == parametervalues.Length
                    computationcycle += (howmanythreads - 1)*l2 + l4;// howmanythreads;
                }
                

            }
            sw.Stop();
            computationtime += sw.ElapsedMilliseconds;
            //update
            UpdateIdentifyProcess(parameternames,parametervalues,result);
            //getWorkerURLs(this.ModelName); update worker list moved to updateidentifyprocess - to update each 5 seconds - otherwise excessive calling of this method seems to cause memory consumption

            return result;
        }

        private struct repeatCompStruct
        {
            public int wi;
            public int indexmin;
            public int indexmax;
        };

        private struct repeatCompParams
        {
            public string[] parameternames;
            public double[][] parametervalues;
            public string[] variablenamesinresult;
            public double[] timepoints;
            public double[][][] result;
        }

        private bool repeatComp = false;
        private List<repeatCompStruct> repeatList = new List<repeatCompStruct>();
        private repeatCompParams rp;
        private void setRepeatComputation(int wrongServiceIndex, int paramvaluesmin, int paramvaluesmax, ref string[] parameternames, ref double[][] parametervalues, ref string[] variablenamesinresult, ref double[] timepoints,ref double[][][] result)
        {
            lock (repeatList)
            {
                if (!repeatComp)
                {
                    repeatComp = true;
                    rp= new repeatCompParams(){parametervalues = parametervalues,parameternames = parameternames,timepoints = timepoints,variablenamesinresult = variablenamesinresult,result=result};
                }
                repeatList.Add(new repeatCompStruct()
                                   {wi = wrongServiceIndex, indexmax = paramvaluesmax, indexmin = paramvaluesmin});
            }
        }

        private void repeatComputation()
        {
            foreach (var compStruct in repeatList)
            {
                if (compStruct.wi == 0) throw new Exception("First worker failed, cannot restore computation.");                
                for (int i = compStruct.indexmin; i < compStruct.indexmax;i++ )
                {
                    rp.result[i] = Compute(RestURLs[0], rp.parameternames, rp.parametervalues[i], rp.timepoints,
                                           rp.variablenamesinresult);
                }
            }
        }

        private void getWorkerURLs(string modelName)
        {
            var client = new JsonServiceClient(updateWorkerListURL);
            //gets workers which can compute this model from MasterService
            var workers = client.Get(new Workers() {ModelName = modelName});
            //updates RESTURLs of the returned workers
            RestURLs = workers.Select(w => w.RestUrl).ToArray();
        }

        //TODO move to another object to keep POCO
        [Log]
        private void UpdateIdentifyProcess(string[] parameternames, double[][] parametervalues, double[][][] result)
        {
            if (!stopwatch.IsRunning) { stopwatch.Start(); lastcomputationcycle = computationcycle; }
            if (stopwatch.ElapsedMilliseconds > 5000) //update each 5 seconds                                        
            try
            {

                var elapsedmilis = stopwatch.ElapsedMilliseconds;
                stopwatch.Restart();//restart sw to measure another 5 seconds
                //servicestack client
                //update worker list
                var workerspercycle = RestURLs.Length;
                getWorkerURLs(this.ModelName);
                var client = new JsonServiceClient(updateIdentifyProcessURL);
                client.Timeout = new TimeSpan(0,5,0);
                IdentifyDTO identifyDto;
                if (identid == 0)
                {
                    identifyDto = client.Post<IdentifyDTO>(new IdentifyDTO()
                                                               {
                                                                   model = ModelName,
                                                                   name = "Computation of "+ModelName+" at "+DateTime.Now, 
                                                                   countcycles = computationcycle,
                                                                   Parameternames = parameternames,
                                                                   Parametervalues = parametervalues[0],
                                                                   Variablevalues = result[0],
                                                                   timepercycle = elapsedmilis,
                                                                   simulationtime = computationtime,
                                                                   countpercycle = computationcycle-lastcomputationcycle,
                                                                   workerspercycle = workerspercycle
                                                               });
                    identid = identifyDto.Id;
                }
                else
                {
                    identifyDto = new IdentifyDTO()
                                      {
                                          Id = identid,
                                          countcycles = computationcycle,
                                          Parameternames = parameternames,
                                          Parametervalues = parametervalues[parametervalues.Length-1], //vrati posledni parametry ze sekvence
                                          Variablevalues = result[result.Length-1], //vrati vysledek vztahujici se k poslednim parametrum parametervalues.length==result.Length
                                          timepercycle = elapsedmilis,
                                          simulationtime = computationtime,
                                          countpercycle = computationcycle - lastcomputationcycle,
                                          workerspercycle = workerspercycle
                                      };

                    client.Put(identifyDto);
                }
            } catch (Exception e)
            {
                logger.Log(NLog.LogLevel.Error, "computation eception",e);//TODO log the error only
            }
            finally
            {
                lastcomputationcycle = computationcycle;
            }
        }



        [Log]
        public double[][] Simulate(string wurl,string[] parameternames, double[] parametervalues, string[] variablenamesinresult, double[] timepoints)
        {
            return Compute(wurl, parameternames, parametervalues, timepoints,variablenamesinresult);
        }

        [Log]
        public double[][] Simulate(string[] parameternames, double[] parametervalues, string[] variablenamesinresult, double[] timepoints)
        {
            getWorkerURLs(this.ModelName);
            var wurl = RestURLs[0]; //gets first worker - expected that it is localhost or something near.
            return Compute(wurl, parameternames, parametervalues, timepoints, variablenamesinresult);
        }





        //[LogException]
        public double[][] Compute(string simulateworkerurl, string[] parameternames, double[] doubles, double[] timepoints, string[] variablenamesinresult)
        {            
            var client = new JsonServiceClient(simulateworkerurl);
            client.Timeout = new TimeSpan(0, 15, 0);
            string vnames = "";
            foreach (var vn in variablenamesinresult)
            {
                vnames += "/"+vn;

            }

            try
            {
                var result = client.Post<SimulateResponse>(vnames,
                                                           new Simulate()
                                                               {
                                                                   ParameterNames = parameternames,
                                                                   ParameterValues = doubles,
                                                                   TimePoints = timepoints
                                                               });

                //TODO catch exception and try to work around - sent to another worker, manage workers 
                return result.Result;
            } catch (Exception e) //computation node error
                            {
                                //setRepeatComputation(k, j + k * l2, j + k * l2 + l4, ref parameternames, ref parametervalues, ref variablenamesinresult,ref timepoints,ref result);
                logger.Log(NLog.LogLevel.Error,"computation error:",e);
                                return null;
                            }

        }

        public string Description()
        {
            return "FMU Simulator computation on REST URL:" + RestURLs;
        }

        public long GetComputationCycles()
        {
            return computationcycle;
        }

        public long GetSimulationTime()
        {
            return computationtime;
        }

        public void FinishSimulate()
        {
            //delete identid
            var client = new JsonServiceClient(updateWorkerListURL);
            //IReturnVoid request;
            var ip = new IdentifyProcesses() {Ids = new long[]{identid}};
            client.Delete(ip);
        }
    }

    public class IdentifyDTO : IReturn<IdentifyDTO>
    {
        public long Id { get; set; }
        public string model { get; set; }
        public string name { get; set; }
        public long countcycles { get; set; }
        public string elapsedtime { get; set; }
        public string[] Variablenames { get; set; }
        public double[][] Variablevalues { get; set; }
        public double[][] Experimentalvalues { get; set; }
        public string[] Parameternames { get; set; }
        public double[] Parametervalues { get; set; }
        public double Ssq { get; set; }
        public long timepercycle { get; set; }
        public long simulationtime { get; set; }
        public long countpercycle { get; set; }
        public int workerspercycle { get; set; }

    }
/*
    public class IdentifyDTO: IReturn<IdentifyDTO>
    {
        public long Id { get; set; }
        public string model { get; set; }
        public long countcycles { get; set; }
        public string elapsedtime { get; set; }
        public string[] Variablenames { get; set; }
        public double[][] Variablevalues { get; set; }
        public string[] Parameternames { get; set; }
        public double[] Parametervalues { get; set; }
        public double Ssq { get; set; }
    }
    */
    public class IdentifyProcesses : IReturn<List<IdentifyDTO>>
    {
        public long[] Ids { get; set; }
        public IdentifyProcesses(params long[] ids)
        {
            this.Ids = ids;
        }
    }
    public class Workers : IReturn<List<Worker>>
    {
        public string ModelName { get; set; }
        public long[] Ids { get; set; }
        public Workers(params long[] ids)
        {
            this.Ids = ids;
        }
    }

    public class Worker : IReturn<Worker>
    {
        public long Id { get; set; }
        public string ModelName { get; set; }
        public string RestUrl { get; set; } //endpoint of web service - 
        public string WorkerType { get; set; } //the type of the worker - could be cloud/grid - REST web service, or boinc - SOAP web service
        public int Priority { get; set; } //for future usage
    }
}