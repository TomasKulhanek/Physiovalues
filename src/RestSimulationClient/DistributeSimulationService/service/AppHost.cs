using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using FMUSingleNodeWrapper.service;
using ServiceStack.ServiceInterface;
using ServiceStack.WebHost.Endpoints;

namespace DistributeSimulationService 
{
  
    internal class AppHost : AppHostHttpListenerBase
    {
        public AppHost() : base("StarterTemplate HttpListener", 
            typeof (HelloService).Assembly//, 
            //typeof (SimulateService).Assembly
        
    ) { }

        public override void Configure(Funq.Container container)
        {
            Routes
                .Add<Hello>("/hello")
                .Add<Hello>("/hello/{Name}")
                .Add<DistributedSimulation>("/distributedsimulation/{ModelName}")
                .Add<SimulationNode>("/simulationnode/{ModelName}");
        }

        public override void Stop()
        {
            base.Stop();
            FMUSimulator.StopSimulator();
        }

        public override void Start(string urlBase)
        {
            FMUSimulator.InitSimulator();
            base.Start(urlBase);
            
        }
    }


    internal class SimulateService : Service
    {
        //private readonly FMUSimulator Simulator = new FMUSimulator();

        public object Get(Simulate request)
        {
            var myresults = new List<double[]>();
            //if (request.VariableNames.Length > 0)  Console.WriteLine("# variables " + request.VariableNames.Length + " "+ request.VariableNames[0]);
            Console.WriteLine("simulation params:"+request.Start+" "+request.Stop+" "+request.Steps);
            FMUSimulator.RunSimulator(ref myresults,SplitSlash(request.VariableNames),request.Start,request.Steps,request.Stop); //split variablenames also by slash e.g. ph/time will be 'ph' and 'time'

            return new SimulateResponse { Result = myresults.ToArray()};
        }
        
        public object Post(Simulate request)
        {
            var myresults = new List<double[]>();
            //if (request.VariableNames.Length > 0)  Console.WriteLine("# variables " + request.VariableNames.Length + " "+ request.VariableNames[0]);
            //TODO do some simulator preparation
            FMUSimulator.RunSimulator(ref myresults, SplitSlash(request.VariableNames),request.Start,request.Steps,request.Stop); //split variablenames also by slash e.g. ph/time will be 'ph' and 'time'

            return new SimulateResponse { Result = myresults.ToArray() };
        }

        private string[] SplitSlash(IEnumerable<string> variableNames)
        {
            var newVariableNames = new List<string>();
            var separators = new char[]{'/'};
            foreach (var variableName in variableNames) newVariableNames.AddRange(variableName.Split(separators,20));
            return newVariableNames.ToArray();
        }
    }

    //DTO response
    internal class SimulateResponse
    {
        public double[][] Result { get; set; }
    }

    //DTO request
    public class Simulate
    {
       public Simulate()
       {
           Start = 0;
           Stop = 2592000;
           Steps = 200;
       }

        public string[] VariableNames { get; set; }
        public string[] ParameterNames { get; set; } //names of parameter in POST
        public double[] ParameterValues { get; set; } //values of the parameter in POST in the same order as ParameterNames
        
//        [System.ComponentModel.DefaultValue(0)]
        public int Start { get; set; }

        //[System.ComponentModel.DefaultValue(2592000)]
        public int Stop { get; set; }

        //[System.ComponentModel.DefaultValue(200)]
        public int Steps { get; set; }
    }

}
