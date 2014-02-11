using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimulatorBalancerLibrary
{
    public class RemoteSimulatorBalancer : IBalancerBehavior
    {
        public struct SimulatorModel
        {
            public string ModelName;
            public List<string> Variables;
            public List<string> Parameters;
        }

        public struct SimulatorWorker
        {
            public string Url;
            public List<SimulatorModel> ComputableModels;
        }

        public static List<SimulatorWorker> workers { set; get; }
        public static List<SimulatorModel> models { set; get; }

        public static void TestSimulate()
        {
            
        }

        public void Balance()
        {
            throw new NotImplementedException();
        }
    }
}
