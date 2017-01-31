using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Test.CommandLineParsing;

namespace DymolaModel
{
    class DymosimFile
    {
        /* replaces the values in the string content
double experiment(7,1)
       0                   # StartTime    Time at which integration starts
                           #              (and linearization and trimming time)
       1                   # StopTime     Time at which integration stops
       0                   # Increment    Communication step size, if > 0
     100                   # nInterval    Number of communication intervals, if > 0

         */
        //change the DS file parameter values

        public static void ParseSimulationFile(string filename, CommandLineDictionary d, HashSet<string> dsParameters)
        {
            var reader = new StreamReader(filename);
            string content = reader.ReadToEnd();
            reader.Close();
            foreach (var key in d.Keys)
            {
                if (dsParameters.Contains(key))
                {
                    string value;
                    if (d.TryGetValue(key, out value)) ReplaceDS(value, ref content, key);
                }
                else
                {
                    string value;
                    if (d.TryGetValue(key, out value)) ReplaceVariableDS(value, ref content, key);
                }

            }
            //            ReplaceDS(s, ref content, "StartTime");
            //            ReplaceDS(s1, ref content, "StopTime");
            //            ReplaceDS(s2, ref content, "nInterval");
            var writer = new StreamWriter(filename);
            writer.Write(content);
            writer.Close();

        }

        private static void parseVariablesAndDescriptions(ref string content,ModelMeta mm)
        {
            string searchText = "initialName\\(([0-9]+),[0-9]+\\)";
            Match names = Regex.Match(content, searchText);
            // number of variables is in $1 
            var numberofnames = Int32.Parse(names.Groups[1].Value);
            //go through all 
            string namesstr = content.Substring(names.Index);
            string[] namesarr = namesstr.Split(new char[]{'\r','\n'},numberofnames+2,StringSplitOptions.RemoveEmptyEntries); //+1 first row contains searchtext, +1 last row contains rest

            searchText = "initialValue\\(";
            names = Regex.Match(content, searchText);
            string valuesstr = content.Substring(names.Index);
            string[] valuesarr = valuesstr.Split(new char[] { '\r', '\n' }, (numberofnames*2)+2, StringSplitOptions.RemoveEmptyEntries); //each value is in 2 rows

            searchText = "initialDescription\\(";
            names = Regex.Match(content, searchText);
            string descrstr = content.Substring(names.Index);
            string[] descrarr = descrstr.Split(new char[] { '\n'  }, numberofnames+2, StringSplitOptions.None); //there may be empty descriptions - each 2 row is empty because of \r\n

            //for i=0 the array contains also the searchtext row
            if (mm.VariableMetas==null) mm.VariableMetas = new Dictionary<string, List<string>>();
            bool extraline = !(valuesarr[1].Contains('#')); //some of dsin contains extra line ending if some of the variable name is too long
            for (int i=1;i<=numberofnames;i++)
            {
                //VariableMeta vm = new VariableMeta();
                List<string> vm = new List<string>();// VariableMeta();
                //vm.InitialValue = Double.Parse(valuesarr[i*2-1].Substring(3,24)); // the value is on each 2nd row from 3rd character 24 chars
                if (extraline) vm.Add(valuesarr[i*2 - 1].Substring(3, 24).Trim());
                else vm.Add(valuesarr[i].Substring(3,24).Trim());
                //vm.Description=((descrarr[i].Length >0) && (descrarr[i][descrarr[i].Length-1]=='\r'))?descrarr[i].Substring(0,descrarr[i].Length-1):descrarr[i]; //removes ending \r if there is
                vm.Add(((descrarr[i].Length > 0) && (descrarr[i][descrarr[i].Length - 1] == '\r')) ? descrarr[i].Substring(0, descrarr[i].Length - 1) : descrarr[i]); //removes ending \r if there is
                //vm.Units = Regex.Match(descrarr[i], "\\[[^\\]]+\\]").Value;
                vm.Add(Regex.Match(descrarr[i], "\\[[^\\]]+\\]").Value);
                if (extraline) vm.Add(valuesarr[i * 2 - 1].Substring(0, 3).Trim()); //type
                else vm.Add(valuesarr[i].Substring(0,3).Trim());

                mm.VariableMetas.Add(namesarr[i],vm);
            }

            
        }

        public static void ReplaceDS(string s, ref string content, string what)
        {
            if (s != null)
            {
                string searchText = "[ 0-9E\\+\\-\\.]{27}(?=\\# " + what + ")";//^[ 0-9E\+\-\.]*\# StartTime
                string replaceText = s.PadRight(27);
                content = Regex.Replace(content, searchText, replaceText);
                //                Match match = Regex.Match(content, searchText);
                //                if (match.Success) Console.WriteLine(match.Value);
                //                else Console.WriteLine("doesn't match:"+searchText);
            }
        }

        /*
  0  7.400000000000000E+000  0  0  6  292   # outputBusConnector.PHA
 -1  1.000000000000000E-002  0  0  1  280   # controllerOfRenalFunctionBlock.controllerOfRenalFuncion.CPAL
 -1       1                       0                       0                
  2     0   # _dummy
  0       0                       0                       0                
  3     0   # _derdummy
 -1  4.400000000000000E-001       0                  1.000000000000000E+100
  1   280   # c[1]
 -1       0                       0                  1.000000000000000E+100
  1   280   # c[2]
 -1  6.600000000000000E-001       0                  1.000000000000000E+100
  1   280   # c[3]

       0                  1.000000000000000E+100

         */
        public static void ReplaceVariableDS(string s, ref string content, string what)
        {
            if (s != null)
            {
                //                Console.Write("searching: "+ what);
                what = Regex.Replace(what, "([\\[\\]])", "\\$1");
                //Console.WriteLine(" in regex:"+what);
                string searchText = "(\r\n[^#]{3})([^#]{24})([^#]*# " + what + "\r\n)";
                string replaceText = "$1 " + s.PadRight(23) + "$3";
                //                Console.WriteLine("found:"+Regex.Match(content,searchText).Captures.Count);
                content = Regex.Replace(content, searchText, replaceText);
                //Match match = Regex.Match(content, searchText);
                //if (match.Success) Console.WriteLine(match.Value);
                //else Console.WriteLine("doesn't match:" + searchText);
            }

        }

        internal static void ParseMetaSimulationFile(string filename, ModelMeta mm)
        {
            var reader = new StreamReader(filename);
            string content = reader.ReadToEnd();
            reader.Close();
            parseVariablesAndDescriptions(ref content, mm);
        }
    }
}
