using ServiceStack.ServiceHost;

namespace SimulatorBalancerLibrary
{
    public class GenericSimulator
    {
        public ISimulatorBehavior SimulatorBehavior { get; set; }
        //public IBalancerBehavior BalancerBEhavior { get; set; }

        public GenericSimulator(string modelName, string updateURL)
        {
            SimulatorBehavior = SimulatorBehaviorFactory.CreateSimulatorBehavior(modelName,updateURL);
        }

        public GenericSimulator(ISimulatorBehavior _simulator)
        {
            SimulatorBehavior = _simulator;
        }

        public double[][][] Simulate(string[] parameternames, double[][] parametervalues, string[] variablenamesinresult, double[] timepoints)
        {
            return SimulatorBehavior.Simulate(parameternames, parametervalues, variablenamesinresult, timepoints);
        }

        public string Description()
        {
            return SimulatorBehavior.Description();
        }

        public long GetComputationCycles()
        {
            return SimulatorBehavior.GetComputationCycles();
        }

        public void FinishSimulate()
        {
            SimulatorBehavior.FinishSimulate();
        }



        /*public double[][] Simulate(string[] p, double[] result, string[] variable_names, double[] timepoints)
        {
            return SimulatorBehavior.Simulate(p, result, variable_names, timepoints);
        }*/

        public double[][] Simulate(string wurl,string[] p, double[] result, string[] variable_names, double[] timepoints)
        {
            return SimulatorBehavior.Simulate(wurl,p, result, variable_names, timepoints);
        }
    }
}
