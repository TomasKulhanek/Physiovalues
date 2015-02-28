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
                    if (args.Length == 4) IniFile2ModelFile(args[1], args[2],args[3],"n");
                    else if (args.Length > 4) IniFile2ModelFile(args[1], args[2], args[3], args[4]);
                }
                else if (args[0].Equals("p"))
                {
                    if (args.Length > 3) IniFile2PhysioModelFile(args[1], args[2], args[3]);
                    //else if (args.Length > 4) IniFile2ModelFile(args[1], args[2], args[3], args[4]);
                }
                else if (args[0].Equals("d"))
                {
                    if (args.Length > 2) DsFile2IniFile(args[1], args[2]);
                }
                else if (args[0].Equals("2"))
                {
                    if (args.Length > 2) DsFile2DsFile(args[1], args[2]);
                }

                else Help();
            }
            else Help();
        
        }

        private static void DsFile2DsFile(string s, string s1)
        {
            TextReader dsFile = null;
            TextWriter iniFile = null;
            String[] lineElements;
            if (File.Exists(s))
            {
                try
                {
                    dsFile = new StreamReader(s);
                    iniFile = new StreamWriter(s1);
                    string strLine,strLine2;
                    do
                    {
                        strLine = dsFile.ReadLine();
                        strLine2 = dsFile.ReadLine();
                        iniFile.WriteLine(strLine + strLine2);
                    } while (strLine != null);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (iniFile != null)
                        iniFile.Close();
                    if (dsFile != null)
                        dsFile.Close();
                }
            }
        }

        private static void IniFile2PhysioModelFile(string inifile, string modelfile1, string modelfile2)
        {
            //throw new NotImplementedException();
            var pairs = ReadIniFile(inifile);
            WriteModel(modelfile1, modelfile2, pairs, "", new PhysiomodelConvert());
        }

        private static void Help()
        {
            Console.WriteLine(
            "Usage: \n" +
            "c [defaultfile] [inifile]\n" +
            "    converts default file to inifile\n"+
            "m [inifile] [inputmodelfile] [outputmodelfile2] [n|f]\n" +
            "     adds values from inifile to hummod (till 2014) modelica model\n" +
            "     adds 'varValue= k' to coresponding Physiolibrary.Utilities.ConstantFromFile\n" +
            "     adds 'varValue=k' to coresponding Variable\n" +
            "     last argument is either default 'n' for initType = Init.NoInit \n" +
            "     or 'c' = Init.FromFile which cause reading the default.txt from file again\n" +
            "p [inifile] [inputmodelfile] [outputmodelfile]\n" +
            "     adds values from defaultfile to physiomemodel (from 2014 and later) modelica model\n" +
            "     similar to 'm' switch.\n"+
            "d [dsin.txt] [dsin.ini]\n" +
            "     converts Dymola dsin.txt file to ini file with varname=varvalue\n" +
            "2 [dsfile.txt] [dsfile2014.txt]\n" +
            "     joins 2 lines into 1 line, preparation from Dymola2015 version to Dymola2014 version\n" +
            ""
            );
        }


        private static void DsFile2IniFile(string arg1, string arg2)
        {
            //throw new Exception("The method or operation is not implemented.");
            TextReader dsFile = null;
            TextWriter iniFile = null;
            String[] lineElements;
            if (File.Exists(arg1))
            {
                try
                {
                    dsFile = new StreamReader(arg1);
                    iniFile = new StreamWriter(arg2);
                    var strLine = dsFile.ReadLine();
                    var fillvalues = false;
                    while (strLine != null)
                    {
                        //split by space
                        if (fillvalues)
                        {
                            lineElements = strLine.Split(" ".ToCharArray(), 8, StringSplitOptions.RemoveEmptyEntries);
                            fillvalues = lineElements.Length >= 8;
                            //ignore iconpoints (with [])
                            //
                            if (fillvalues)
                                //if (!lineElements[7].EndsWith("]")) 
                                    iniFile.WriteLine(lineElements[7] + "=" + lineElements[1]);                            
                        }
                        else
                        {
                            fillvalues = strLine.StartsWith("double initialValue");
                        }
                        strLine = dsFile.ReadLine();
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
                    if (dsFile != null)
                        dsFile.Close();
                }
            }
        }


        private static void IniFile2ModelFile(string inifile, string modelfile1, string modelfile2, string initType)
        {
            //throw new NotImplementedException();
            var pairs = ReadIniFile(inifile);
            if (initType.Equals('f')) WriteModel(modelfile1,modelfile2, pairs,initType, new HumModFileConvert());
                WriteModel(modelfile1,modelfile2, pairs,initType, new HumModConvert());
        }

        private static void WriteModel(string mf1,string mf2, Dictionary<string, string> pairs, string initType,IConvertBehavior C)
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
                    var nextstrLine = defaultFile.ReadLine();
                    while (strLine != null)
                    {
                        //strLine = strLine.Trim(); //.ToUpper();
                        //iniFile.WriteLine(strLine + "=" + defaultFile.ReadLine());                        
                        bool usedNextStrline = false;
                        if (C.CheckVariables(strLine)) strLine = C.ConvertVariables(strLine, pairs, nextstrLine,ref usedNextStrline);
                        else if (C.CheckConstant(strLine)) strLine = C.ConvertConstant(strLine, pairs, nextstrLine, ref usedNextStrline);
                        iniFile.WriteLine(strLine);
                        if (!usedNextStrline) strLine = nextstrLine;
                        else strLine = defaultFile.ReadLine();
                        nextstrLine = defaultFile.ReadLine();

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
                            try
                            {
                                keyPairs.Add(keyPair[0], value);
                            }
                            catch (ArgumentException ex)
                            {
                                Console.WriteLine("Duplicate variable:{0}",keyPair[0]);
                            }
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
