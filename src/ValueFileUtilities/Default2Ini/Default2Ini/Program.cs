using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Default2Ini
{
    class Program
    {
        static void Main(string[] args)
        {
            
            if (args.Length > 0)
            {
                //var myIniParser = new readIniFile(args[0]);
                if (args[0].Equals("c"))
                {
                    if (args.Length > 2) DefaultFile2IniFile(args[1], args[2]);
                }
                else if (args[0].Equals("m"))
                {
                    if (args.Length > 3) IniFile2ModelFile(args[1], args[2],args[3]);
                }
                else Help();
            }
            else Help();
        
        }

        private static void Help()
        {
            Console.WriteLine(
                "Usage: \nc [defaultfile] [inifile]\n    converts default file to inifile\nm [inifile] [modelfile]\nm     converts inifile to modelica");
        }

        private static void IniFile2ModelFile(string inifile, string modelfile1, string modelfile2)
        {
            //throw new NotImplementedException();
            var pairs = ReadIniFile(inifile);
            WriteModel(modelfile1,modelfile2, pairs);
        }

        private static void WriteModel(string mf1,string mf2, Dictionary<string, string> pairs)
        {
         //   throw new NotImplementedException();
            TextReader defaultFile = null;
            TextWriter iniFile = null;
            if (File.Exists(mf1))
            {
                try
                {
                    defaultFile = new StreamReader(mf1);
                    iniFile = new StreamWriter(mf2);
                    var strLine = defaultFile.ReadLine();
                    while (strLine != null)
                    {
                        //strLine = strLine.Trim(); //.ToUpper();
                        //iniFile.WriteLine(strLine + "=" + defaultFile.ReadLine());
                        if (CheckVariables(strLine)) strLine = ConvertVariables(strLine, pairs);
                        else if (CheckConstant(strLine)) strLine = convertConstant(strLine, pairs);
                        iniFile.WriteLine(strLine);
                        strLine = defaultFile.ReadLine();

                    }

                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (iniFile != null)
                        iniFile.Close();
                    if (defaultFile != null)
                        defaultFile.Close();
                }
            }
        }

        private static string convertConstant(string strLine, Dictionary<string, string> pairs)
        {
            var regex = new Regex("varName = \"([^\"]*)[^\\)]*");
            var match = regex.Match(strLine);
            if (match.Success)
            {
                var varValue = pairs[match.Groups[1].Value];//Regex.Find(strLine, regex)];
                return regex.Replace(strLine, "varName = \"$1\", varValue = \"" + varValue + "\"");
            }
            else return strLine;
        }

        private static string ConvertVariables(string strLine, Dictionary<string, string> pairs)
        {
            var regex = new Regex("varName = \"([^\"]*)[^\\)]*");
            var match = regex.Match(strLine);
            if (match.Success)
            {
                var varValue = pairs[match.Groups[1].Value];//Regex.Find(strLine, regex)];
                return regex.Replace(strLine, "varName = \"$1\",varValue = \"" + varValue + "\"");
            }
            else return strLine;
            //var varValue = pairs[Regex.Find(strLine,regex)];
            //return Regex.Replace(strLine, regex,"varname = \"$1\" varValue = \"" + varValue + "\"");
        }

        private static bool CheckConstant(string strLine)
        {
            return Regex.IsMatch(strLine, "[ ]*Physiolibrary\\.Utilities\\.ConstantFromPhysiovalues");
        }

        private static bool CheckVariables(string strLine)
        {
            return Regex.IsMatch(strLine, "[ ]*Variable");
        }

        /// <summary>
        /// Opens the INI file at the given path and enumerates the values in the IniParser.
        /// </summary>
        /// <param name="iniPath">Full path to INI file.</param>
        public static Dictionary<string,string> ReadIniFile(String iniPath)
        {
            Dictionary<string,string> keyPairs = new Dictionary<string, string>();
            TextReader iniFile = null;
            String strLine = null;
            String[] keyPair = null;
            if (File.Exists(iniPath))
            {
                try
                {
                    iniFile = new StreamReader(iniPath);

                    strLine = iniFile.ReadLine();

                    while (strLine != null)
                    {
                        strLine = strLine.Trim(); //.ToUpper();

                        if (strLine != "")
                        {
                            keyPair = strLine.Split(new char[] {'='}, 2);

                            String value = null;

                            if (keyPair.Length > 1)
                                value = keyPair[1];

                            keyPairs.Add(keyPair[0], value);
                        }
                        strLine = iniFile.ReadLine();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (iniFile != null)
                        iniFile.Close();
                }
            }
            else
                throw new FileNotFoundException("Unable to locate " + iniPath);
            return keyPairs;
        }

        //read default file odd rows are varnames, even rows are values
        private static void DefaultFile2IniFile(string arg1, string arg2)
        {
            //throw new Exception("The method or operation is not implemented.");
            TextReader defaultFile = null;
            TextWriter iniFile = null;
            if (File.Exists(arg1))
            {
                try
                {
                    defaultFile = new StreamReader(arg1);
                    iniFile = new StreamWriter(arg2);
                    var strLine = defaultFile.ReadLine();
                    while (strLine != null)
                    {
                        //strLine = strLine.Trim(); //.ToUpper();
                        iniFile.WriteLine(strLine + "=" + defaultFile.ReadLine());
                        strLine = defaultFile.ReadLine();
                    }

                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (iniFile != null)
                        iniFile.Close();
                    if (defaultFile != null)
                        defaultFile.Close();
                }
            }
        }


    }
}
