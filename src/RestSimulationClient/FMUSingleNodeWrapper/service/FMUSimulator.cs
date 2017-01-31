using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace FMUSingleNodeWrapper
{
    internal class FMUSimulator
    {
        public static string inputFile = "HumMod_HumMod_0GolemEdition.fmu";
        public static string defaulttempDir = "fmutmp";
        public static string MatlabResFile = "dsres.mat";
        public static Boolean initialized;
        private static int mysimulationsteps = 1;
        

        [DllImport("CW2FMIDriver.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void DisposeSimulator();

        [DllImport("CW2FMIDriver.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void DisposeAllSimulator();

        [DllImport("CW2FMIDriver.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void StepSimulation();
        [DllImport("CW2FMIDriver.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void StepZeroSimulation();

        [DllImport("CW2FMIDriver.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void InitializeSlave();

        [DllImport("CW2FMIDriver.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void StopSimulation();

        [DllImport("CW2FMIDriver.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void InitSimulation();

        [DllImport("CW2FMIDriver.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void RestartSimulationIfNeeded();

        [DllImport("CW2FMIDriver.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void PauseSimulation();

        [DllImport("CW2FMIDriver.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void ResumeSimulation();

        [DllImport("CW2FMIDriver.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void ContinueSimulation();

        [DllImport("CW2FMIDriver.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void InitSimulator(string FMUPath, string tmpPath);

        [DllImport("CW2FMIDriver.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void ReInitSimulatorByName(string FMUPath);

        [DllImport("CW2FMIDriver.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void ReInitSimulator(Int32 index);

        [DllImport("CW2FMIDriver.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe void GetVariableValue(string variableName, double* value);

        [DllImport("CW2FMIDriver.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void GetVariableValues(string[] variableNames, int variableNamesLength,double [] values);//, out double[] values);

        [DllImport("CW2FMIDriver.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetVariableValue(string variableName, double value);

        [DllImport("CW2FMIDriver.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetStepTime(double value);

        [DllImport("CW2FMIDriver.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void ResetSimulationTimes(double start, double step, double end);

        public static int RunSimulator(ref List<double[]> myresults, string[] varnames, int startTime,
                                       int simulationsteps, int stopTime)
        {
            return RunFMUSimulator(ref myresults, varnames, startTime, simulationsteps, stopTime);
        }

        public static void SetProperty(string key, string value)
        {
            inputFile = value; //TODO change it to implement key/value assignment
        }

        /*        private const int simulationsteps = 200;
        private const int Stop = 2592000;
        private const int Start = 0;
        */

        public static int RunFMUSimulator(ref List<double[]> myresults, string[] varnames, int startTime,
                                          int simulationsteps, int stopTime)
        {
            InitSimulator(); //conditionally initialize
            mysimulationsteps = simulationsteps;
            ResetSimulationTimes(startTime, (stopTime - startTime)/simulationsteps, stopTime);
            //SetVariableValue();
            SameStepSimulate(ref myresults, varnames);
            /*return dymosimMainModel(3, new[]
                {   "dymosim.exe",
                    inputFile,
                    MatlabResFile
                }, DymosimModelFunctions());
             */

            return 0;
        }

        public static void StopSimulator()
        {
            if (initialized)
            {
                DisposeAllSimulator();
                initialized = false;
            }
        }

        public static void MyInitSimulator(string fmufile, string tempdir)
        {
            inputFile = fmufile;
            defaulttempDir = tempdir;
            InitSimulator();
        }

        private static void InitSimulator(string fmufile)
        {
            inputFile = fmufile;
            InitSimulator();
        }

        private static void InitSimulator()
        {
            if (!Directory.Exists(defaulttempDir)) 
            {
                InitSimulator(inputFile, defaulttempDir);
                InitSimulation();
                initialized = true;
            } else
            {  // TODO check whether to improve performance when the same call is repeated on the same model
                ReInitSimulatorByName(inputFile);
                //ReInitSimulator();
            }
        }

        private static void SameStepSimulate(ref List<double[]> myresults, string[] varnames)
        {
            //fix bug when "Time" is requested, but FMU returns time values for "time".
            //if (varnames[0].Equals("Time")) varnames[0] = "time";            fixed in driver _stricmp
            for (int i = 0; i < mysimulationsteps; i++) //simulationsteps
            {
                GetFMUValue(ref myresults, ref varnames);
                //if (varnames.Length>0) Console.WriteLine(varnames[0]+" : " + values[0]);
                StepSimulation();
            }
            GetFMUValue(ref myresults, ref varnames); //fixed bug - last step performed, but quantities not retrieved
            StepZeroSimulation();
        }

        public static class ArrayOf<T>
        {
            public static T[] Create(int size, T initialValue)
            {
                T[] array = (T[])Array.CreateInstance(typeof(T), size);
                for (int i = 0; i < array.Length; i++)
                    array[i] = initialValue;
                return array;
            }
        }

        private static void GetFMUValue(ref List<double[]> myresults, ref string[] varnames)
        {
            try
            {
                //var values = new double[varnames.Length];
                if (varnames.Length <= 0) return;
                double[] values = ArrayOf<double>.Create(varnames.Length,0);
                //double[] values = new double[varnames.Length];

                //fixed (double * resPtr = values)
                
                GetVariableValues(varnames, varnames.Length, values); //,out values);

                myresults.Add(values);
                /*for (int j = 0; j < varnames.Length; j++)
                        //TODO FMU can retrieve array of variable in one call, it may be implemented in FMU level
                    {
                        fixed (double* p = &values[j])
                            //gets address of the item of array (http://stackoverflow.com/questions/5079736/cannot-take-the-address-of-the-given-expressionc-pointer)
                        {
                            GetVariableValue(varnames[j], p); //accessviolationexception
                        }
                    }*/
            } catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }

        }

        public static void RunSimulator(ref List<double[]> myresults, string[] varnames, int startTime,
                                        int simulationsteps, int stopTime, string[] parameterNames,
                                        double[] parameterValues)
        {
            mysimulationsteps = simulationsteps;
            if (simulationsteps == 0 && startTime == -1)
            {
//special case - e.g. for reading scalar values, and for cache purposes, simulation stops
                //TODO add check with simulation state time and starttime
                SetSimulationParameters(parameterNames, parameterValues);
                GetFMUValue(ref myresults, ref varnames);
                /*double[] values = new double[varnames.Length];
                if (varnames.Length > 0)

                    for (int j = 0; j < varnames.Length; j++)
                    //TODO FMU can retrieve array of variable in one call, it may be implemented in FMU level
                    {
                        unsafe
                        {
                            fixed (double* p = &values[j])
                            //gets address of the item of array (http://stackoverflow.com/questions/5079736/cannot-take-the-address-of-the-given-expressionc-pointer)
                            {
                                GetVariableValue(varnames[j], p);
                            }
                        }
                    }
                myresults.Add(values);   */
            }
            else
            {
                try
                {
                    if (startTime == -1) startTime = 0;
                    double step = simulationsteps > 0 ? (stopTime - startTime)/(double) simulationsteps : 0;
                    //first set parameters ??
                    //now set the parameter
                    SetSimulationParameters(parameterNames, parameterValues);
                    //reset time resets the simulator??
                    double st = startTime;
                    double sp = stopTime;
                    ResetSimulationTimes(st, step, sp); //TODO bug when calling the simulation second time

                    //and initialize, after initialization it is not possible to change some other parameters
                    //InitializeSlave(); //fixed bug not set parameters after init
                    SameStepSimulate(ref myresults, varnames);
                } catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static void RunSimulator(ref List<double[]> myresults, string[] varnames, double[] timePoints,
                                        string[] parameterNames, double[] parameterValues)
        {
            InitSimulator(); //conditionally initialize
            if (timePoints.Length < 2)
                throw new Exception(
                    "not enough timepoints to provide simulation, at least 2 timepoints required, provided only " +
                    timePoints.Length);

            double startTime = timePoints[0]; //first point
            double stopTime = timePoints[timePoints.Length - 1]; //last point
            //set simulation times, but the simulation step will be changed during the simulation
            //bug #163  first set parameters ??
            SetSimulationParameters(parameterNames, parameterValues);

            ResetSimulationTimes(startTime, (stopTime - startTime) / timePoints.Length, stopTime);
            

            //SetSimulationParameters(parameterNames, parameterValues);
            //InitializeSlave(); //fixed bug not set parameters after init
            //foreach (var vname in varnames) Console.WriteLine("variable name:'"+vname+"'");
            //foreach (var pname in parameterNames) Console.WriteLine("parameter name:'"+pname+"'");
            SimulateVariableStep(ref myresults, varnames, timePoints);
        }

        private static void SetSimulationParameters(string[] parameterNames, double[] parameterValues)
        {
            if ((parameterNames == null) || (parameterValues == null)) return;
            if ((parameterNames.Length != parameterValues.Length))
                throw new Exception("Not the same number of parameter names and values. Parameter names: " +
                                    parameterNames.Length + " values:" + parameterValues.Length);
            for (int i = 0; i < parameterNames.Length; i++)
            {
                SetVariableValue(parameterNames[i], parameterValues[i]);
                //TODO debug log output by parameter
                //Console.Write(parameterNames[i] + ":" + parameterValues[i] + " ");
            }
            //Console.WriteLine();
        }

        private static void SimulateVariableStep(ref List<double[]> myresults, string[] varnames, double[] timePoints)
        {
            //fix bug when "Time" is requested, but FMU returns time values for "time".
            if (varnames[0].Equals("Time")) varnames[0] = "time";
            for (int i = 0; i < timePoints.Length; i++)
            {
                GetFMUValue(ref myresults, ref varnames);
                /*double[] values = new double[varnames.Length];
                if (varnames.Length > 0)

                    for (int j = 0; j < varnames.Length; j++)
                        //TODO FMU can retrieve array of variable in one call, it may be implemented in FMU level
                    {
                        unsafe
                        {
                            fixed (double* p = &values[j])
                                //gets address of the item of array (http://stackoverflow.com/questions/5079736/cannot-take-the-address-of-the-given-expressionc-pointer)
                            {
                                GetVariableValue(varnames[j], p);
                            }
                        }
                    }
                myresults.Add(values);*/
                //if (varnames.Length > 0) Console.WriteLine(varnames[0] + " : " + values[0]);
                if (i < timePoints.Length - 1) //fix bug indexoutofbounds exception
                {
                    SetStepTime(timePoints[i + 1] - timePoints[i]);
                    StepSimulation();
                }
            }
            //fix bug #160 - last value was retrieved double times - error in identification - returned array with different size
            //GetFMUValue(ref myresults, ref varnames); 

        }
    }
}