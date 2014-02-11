using System;

namespace SimulatorBalancerLibrary
{
    public class SimulatorBehaviorFactory
    {
        public static ISimulatorBehavior CreateSimulatorBehavior(string modelname,string updateURL)
        {
            //creates simulator implementation based on the modelname - some simulators might be implemented directly e.g. mysinc as a .NET class
            if (modelname.StartsWith("mysinc"))
            { //default test Simulator
                return new MySincSimulatorBehavior();
            }
            else 
            { //REST FMU Simulator
                return new RestFMUSimulator(modelname,updateURL);
            } 
            
        }
    }
}