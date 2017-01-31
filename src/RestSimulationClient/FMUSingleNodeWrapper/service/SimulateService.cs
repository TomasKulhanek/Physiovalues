using System;
using System.Collections.Generic;
using ServiceStack.ServiceInterface;

namespace FMUSingleNodeWrapper
{
    public class SimulateService : Service
    {
        //private readonly FMUSimulator Simulator = new FMUSimulator();
        /*
        public object Get(Simulate request)
        {
            var myresults = new List<double[]>();
            //if (request.VariableNames.Length > 0)  Console.WriteLine("# variables " + request.VariableNames.Length + " "+ request.VariableNames[0]);
            //Console.WriteLine("simulation params:"+request.Start+" "+request.Stop+" "+request.Steps);
            NodeServerParams.sw.Reset(); //resets time elapsed from list simulation //TODO some pattern or SPRING.NET
            
            if ((request.TimePoints != null) && (request.TimePoints.Length > 0))
                //split variablenames also by slash e.g. ph/time will be 'ph' and 'time'
                //call simulation in specific timepoints
                FMUSimulator.RunSimulator(ref myresults, SplitSlash(request.VariableNames), request.TimePoints, request.ParameterNames, request.ParameterValues);                                                                        
            else 
                //call simulation step by step                
                FMUSimulator.RunSimulator(ref myresults,SplitSlash(request.VariableNames),request.Start,request.Steps,request.Stop,request.ParameterNames, request.ParameterValues); 
                


            return new SimulateResponse { Result = myresults.ToArray()};
        }*/

        private static Object thisLock = new Object();

        public object Any(Simulate request)
        {
            NodeServerParams.sw.Restart(); //resets time elapsed from list simulation //TODO some pattern or SPRING.NET
            if (! NodeServerParams.FMUFiles.Contains(request.ModelName))
            {
                throw new Exception("Model '" + request.ModelName + "' is not in the list of known models.");
            }
            //simulates specific model - the temp directory is created with tempdir prefix and modelname suffix

            FMUSimulator.MyInitSimulator(NodeServerParams.FmuNamePath[request.ModelName], NodeServerParams.GetTempDir(request.ModelName));

            var myresults = new List<double[]>();
            //if (request.VariableNames.Length > 0)  Console.WriteLine("# variables " + request.VariableNames.Length + " "+ request.VariableNames[0]);
            //TODO do some simulator preparation
            if ((request.TimePoints != null) && (request.TimePoints.Length > 0))
                //split variablenames also by slash e.g. ph/time will be 'ph' and 'time'
                //call simulation in specific timepoints
                lock(thisLock)
                FMUSimulator.RunSimulator(ref myresults, SplitSlash(request.VariableNames), request.TimePoints,
                                          request.ParameterNames, request.ParameterValues);
            else
                lock (thisLock)
                //call simulation step by step                
                FMUSimulator.RunSimulator(ref myresults, SplitSlash(request.VariableNames), request.Start, request.Steps,
                                          request.Stop, request.ParameterNames, request.ParameterValues);

//            FMUSimulator.RunSimulator(ref myresults, SplitSlash(request.VariableNames),request.Start,request.Steps,request.Stop); //split variablenames also by slash e.g. ph/time will be 'ph' and 'time'

            return new SimulateResponse {Result = myresults.ToArray()};
        }


        private string[] SplitSlash(IEnumerable<string> variableNames)
        {
            var newVariableNames = new List<string>();
            var separators = new[] {'/'};
            foreach (string variableName in variableNames)
                newVariableNames.AddRange(variableName.Split(separators, 20));
            return newVariableNames.ToArray();
        }
    }

    //DTO response
    public class SimulateResponse
    {
        public double[][] Result { get; set; }
    }

    //DTO request

    public class Simulate
    {
        //default simulation values
        public Simulate()
        {
            Start = -1;
            Stop = 2592000;
            Steps = 200;
        }

        public string ModelName { get; set; }
        public string[] VariableNames { get; set; }
        public string[] ParameterNames { get; set; } //names of parameter in POST
        public double[] ParameterValues { get; set; }
        //values of the parameter in POST in the same order as ParameterNames
        public double[] TimePoints { get; set; }

        //        [System.ComponentModel.DefaultValue(0)]
        public int Start { get; set; }

        //[System.ComponentModel.DefaultValue(2592000)]
        public int Stop { get; set; }

        //[System.ComponentModel.DefaultValue(200)]
        public int Steps { get; set; }
    }
}