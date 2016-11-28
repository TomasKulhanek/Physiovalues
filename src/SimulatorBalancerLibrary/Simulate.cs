using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack;
using ServiceStack.ServiceHost;

namespace RestMasterService.ComputationNodes
{
    //DTO response
    public class SimulateResponse
    {
        public double[][] Result { get; set; }
    }

    //DTO request
    public class Simulate : IReturn<SimulateResponse>
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
        public double[] TimePoints { get; set; }

        //        [System.ComponentModel.DefaultValue(0)]
        public int Start { get; set; }

        //[System.ComponentModel.DefaultValue(2592000)]
        public int Stop { get; set; }

        //[System.ComponentModel.DefaultValue(200)]
        public int Steps { get; set; }
    }
}