using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Test.CommandLineParsing;

namespace FMUSingleNodeWrapper
{
    internal static class MyUtils
    {
 
        public static void parseCommandLine (Type t, string[] args)
        {
            try {
              CommandLineDictionary d = CommandLineDictionary.FromArguments(args);
              //var p = new Program.NodeServerParams();
              //Type t = typeof(paramclass);
            
            
                foreach (KeyValuePair<string, string> param in d)
                    t.InvokeMember("Param" + param.Key.ToUpper(), BindingFlags.InvokeMethod,
                        null, null, new Object[] { param.Value });
            }
            catch (Exception e)
            {
                Console.WriteLine("unknown parameter " + e.Message);
                try
                {
                    t.InvokeMember("ParamH", BindingFlags.InvokeMethod, null, null, new object[] { null });
                } catch(Exception e2)
                {
                    //Console.WriteLine("No help is presented. "+ e2.Message);
                }
            }
        }

        public static void tryFindFile(Type t, string filePattern, string fileParam)
        {
            DirectoryInfo dir = new DirectoryInfo(".");
            var files = dir.GetFiles(filePattern);
            foreach (var file in files)//.Length>0)
            {
                t.InvokeMember(fileParam,BindingFlags.InvokeMethod,null,null,new Object[] {file.Name});
            };
            //parse current directory 
        }
    }
}