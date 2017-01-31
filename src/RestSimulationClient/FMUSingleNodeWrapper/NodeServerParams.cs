using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using FMUSingleNodeWrapper.Properties;

namespace FMUSingleNodeWrapper
{
    internal class NodeServerParams
    {
        public static string httphost = "";
        public static string httpport = "48048";

        public static string listeningOn;
        //holds url where this server is listenning, should be set during server start.

        //public static string FMUFile = "HumMod_HumMod_0GolemEdition.fmu";
        public static List<string> FMUFiles = new List<string>();
        public static string TempDir; // = "tempfmu"; //TODO bug in directory declaration - needs to be absolute
        public static string MasterServiceWorkerURL = "http://physiome.lf1.cuni.cz/identifikace/";
        public static Dictionary<string, long> Id = new Dictionary<string, long>();
        public static Dictionary<string, string> FmuNamePath = new Dictionary<string, string>(); 

        public static bool ArgumentsToContinue = true;

        public static Stopwatch sw = new Stopwatch();
        public static string thisurl = "";

        static NodeServerParams()
        {
            TempDir = Directory.GetCurrentDirectory() + "/tfmu";
        }

        public static void ParamH(string param)
        {
            string helpfile = Resources.readme;
            Console.WriteLine(helpfile);
            ArgumentsToContinue = false;
        }

        public static void ParamP(string param)
        {
            httpport = param;
        }

        public static void ParamF(string param)
        {
            //FMUFile = param;
            //Console.WriteLine("argument /f deprecated, ignored, do not use anymore");//deprecated);
            if (! FMUFiles.Contains(param)) FMUFiles.Add(param);
            //ArgumentsToContinue = true;
        }

        public static void ParamT(string param)
        {
            TempDir = Directory.GetCurrentDirectory()+ "/"+param;//fix bug with accessviolationexception - more directory is created
        }

        public static void ParamU(string param)
        {
            httphost = param;
        }

        public static void ParamW(string param)
        {
            MasterServiceWorkerURL = param;
        }

        public static string GetTempDir(String modelname)
        {
            return TempDir + modelname;
        }
    }
}