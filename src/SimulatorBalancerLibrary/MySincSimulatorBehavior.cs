using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace SimulatorBalancerLibrary
{
    public class MySincSimulatorBehavior : ISimulatorBehavior
    {
        private static int computationcycles=0;
        public static double[][][] Simulate(string[] parameternames, double[][] parametervalues, string[] variablenamesinresult, double[] timepoints)
        {
            computationcycles++; 
            if (parametervalues[0].Length != parameternames.Length)
                throw new Exception("Number of parameters mismatch. Parameter names is " + parameternames.Length +
                                    ",but values in one parameter row is " + parametervalues[0].Length);
            if (parameternames.Length != 4) throw new Exception("expected 4 parameters for this specific model");
            if (variablenamesinresult.Length != 2) throw new Exception("expected 2 variables in result for this specific model");
            var result = new double[parametervalues.Length][][]; //parametervalues.Length x timepoints.Length x variablenamesinresult.Length
            for (int j = 0; j < parametervalues.Length; j++)
            {
                result[j] = new double[timepoints.Length][];
                for (int i = 0; i < timepoints.Length; i++)
                {
                    result[j][i] = new double[variablenamesinresult.Length];
                    result[j][i][0] = timepoints[i];
                    var t = parametervalues[j]; //t.Length == parameternames.Length
                    result[j][i][1] = MySincFunction(timepoints, i, t);
                }
            }
            return result;
        }

        private static Stopwatch sw = new Stopwatch();
//        private static long simulationtime =0;

        private static double MySincFunction(double[] timepoints, int i, double[] t)
        {            
            sw.Start();
            var result = t[2]*Sinc(t[0]*(timepoints[i] - t[1])) + t[3]*timepoints[i];
            sw.Stop();
            return result;
        }

        public  double[][] Simulate(string wurl, string[] parameternames, double[] parametervalues, string[] variablenamesinresult, double[] timepoints)
        {
            return Simulate(parameternames, parametervalues, variablenamesinresult, timepoints);
        }

        public  double[][] Simulate(string[] parameternames, double[] parametervalues, string[] variablenamesinresult, double[] timepoints)
        {
            //TODO delete duplicated code with above method
            computationcycles++;
            if (parameternames.Length != 4) throw new Exception("expected 4 parameters for this specific model");
            if (variablenamesinresult.Length != 2) throw new Exception("expected 2 variables in result for this specific model");
                var result = new double[timepoints.Length][];
                for (int i = 0; i < timepoints.Length; i++)
                {
                    result[i] = new double[variablenamesinresult.Length];
                    result[i][0] = timepoints[i];
                    var t = parametervalues; //t.Length == parameternames.Length
                    result[i][1] = MySincFunction(timepoints, i, t); 
                }
            return result;            
        }      

        public string Description()
        {
            return "Simulation of sin(x)/x.";
        }

        public long GetComputationCycles()
        {
            var mycc = computationcycles;
            computationcycles = 0;
            return mycc;// throw new NotImplementedException();
        }

        public long GetSimulationTime()
        {
            var myst = sw.ElapsedMilliseconds;
            sw.Reset();
            return myst;
        }

        public void FinishSimulate()
        {
            // throw new NotImplementedException();
        }

        public static string test1(string[] parameternames)
        {
            return parameternames.Aggregate("", (current, name) => current + ("parameter: " + name + ", "));
        }

        public static string test2(double[] points)
        {
            return points.Aggregate("", (current, point) => current + ("value: " + point + ", "));
        }

        public static string test3(double[][] p)
        {
            string s = "";
            foreach (var row in p)
            {
                s = row.Aggregate(s, (current, cell) => current + ("parameter: " + cell + ", "));
                s += "\n";
            }
            return s;
        }

        public static double[][][] test4(string[] parameternames, double[][] parametervalues, string[] variablenamesinresult, double[] timepoints)
        {
            throw new Exception("parameternames " + parameternames.Length + " parametervalues[.][] " +
                                parametervalues.Length + " parametervalues[][.] " + parametervalues[0].Length +
                                " variablenamesinresult " + variablenamesinresult.Length + " timepoints " +
                                timepoints.Length);

        }

        private static double Sinc(double x)
        {
            if (Math.Abs(x) < 1e-6) return x;
            else return Math.Sin(x)/x;
        }

        double[][][] ISimulatorBehavior.Simulate(string[] parameternames, double[][] parametervalues, string[] variablenamesinresult, double[] timepoints)
        {
            return Simulate(parameternames, parametervalues, variablenamesinresult, timepoints);
        }
    }

    public class MySincSimulatorBehavior2 : MySincSimulatorBehavior
    {
        public string Description()
        {
            return "MySincSimulatorBehavior2";
        }
    }
}