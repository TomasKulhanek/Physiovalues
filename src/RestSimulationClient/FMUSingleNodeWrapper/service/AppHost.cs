using System;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using FMUSingleNodeWrapper.Properties;
//using FMUSingleNodeWrapper.service;
using ServiceStack.WebHost.Endpoints;

namespace FMUSingleNodeWrapper
{
  
    internal class AppHost : AppHostHttpListenerBase
    {
        public AppHost() : base("StarterTemplate HttpListener", 
            typeof (SimulateService).Assembly
        
    ) { }

        public override void Configure(Funq.Container container)
        {
            Routes
                .Add<SimulateList>("/simulation")
                .Add<Simulate>("/simulation/{ModelName}/{VariableNames*}");

        }

        public override void Stop()
        {
            base.Stop();
            FMUSimulator.StopSimulator();
        }

        public override void Start(string urlBase)
        {
            try
            {
                base.Start(urlBase);
                //FMUSimulator.MyInitSimulator(NodeServerParams.FMUFile, NodeServerParams.TempDir);
            } catch (HttpListenerException e)
            {
                Console.WriteLine("\n" + e.Message);
                Console.WriteLine(Resources.AppHost_Start_,NodeServerParams.httphost,NodeServerParams.httpport);
                Environment.Exit(1);
            }

        }
    }



}
