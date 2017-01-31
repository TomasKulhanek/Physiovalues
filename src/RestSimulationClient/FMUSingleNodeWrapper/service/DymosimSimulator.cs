using System;
using System.Runtime.InteropServices;

namespace FMUSingleNodeWrapper
{
    internal class DymosimSimulator
    {
        [DllImport("dymosim.dll")]
        public static extern IntPtr DymosimModelFunctions();

        [DllImport("libdsdll.dll")]
        public static extern Int32 dymosimMainModel(Int32 argc, String[] argv, IntPtr fPtr);

        public static int RunSimulator()
        {
            return RunDymolaSimulator();
        }

        public void SetProperty(string key, string value)
        {
            inputFile = value; //TODO change it to implement key/value assignment
        }

        public static string inputFile = "dsin.txt";
        public static string MatlabResFile = "dsres.mat";

        public static int RunDymolaSimulator()
        {
            return dymosimMainModel(3, new[]
                {   "dymosim.exe",
                    inputFile,
                    MatlabResFile
                }, DymosimModelFunctions());

        }
       
    }

}