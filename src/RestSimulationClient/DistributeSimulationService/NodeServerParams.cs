using System;

namespace DistributeSimulationService 
{
    class NodeServerParams
    {
        public static string httpport = "48046";
        public static void ParamH(string param)
        {
            string helpfile = FMUSingleNodeWrapper.Properties.Resources.readme;
            Console.WriteLine(helpfile);
        }
        public static void ParamP(string param)
        {
            httpport = param;
        }
    }
}