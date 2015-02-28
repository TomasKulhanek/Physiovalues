using System.Collections.Generic;
using System.Text;

namespace SimulatorBalancerLibrary
{
    public interface ISimulatorBehavior
    {
        //to simulate vector of parameters - can be parallelized
        double[][][] Simulate(string [] parameternames, double [][] parametervalues, string[] variablenamesinresult,double[]timepoints);
        //to simulate only one parameter values - serial
        double[][] Simulate(string workerurl, string[] parameternames, double[] parametervalues, string[] variablenamesinresult, double[] timepoints);
        //to simulate only one parameter values - serial without worker url
        //double[][] Simulate(string[] parameternames, double[] parametervalues, string[] variablenamesinresult, double[] timepoints);
        string Description();
        long GetComputationCycles();
        void FinishSimulate();
        long GetSimulationTime();
    }
}
