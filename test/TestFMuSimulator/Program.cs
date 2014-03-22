using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimulatorBalancerLibrary;

namespace TestFMuSimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Test of FMU simulator library");
            TestRestFMUSimulator();//string modelName,string updateURL)
            Console.Write("Press any key to exit");
            Console.ReadKey();
        }

        private static void TestRestFMUSimulator()
        {
            var rfmu = new RestFMUSimulator("testModel",null);
            var pnames = new string[] {"im[1].k_imai[1]", "im[1].k_imai[2]", "im[1].k_imai[3]", "im[1].k_imai[4]"};
            var pvalues = new double[][] { new double[] { 2, 3, 4, 5 }, new double[] { 1.1, 2.1, 3.1, 4.1 }, new double[] { 1.2, 2.2, 3.2, 4.2 } };
            var testresults = new double[] {0.7389, 0.7358, 0.7362};
            var vnames = new string[] {"time","im[1].sO2"};
            var timepoints = new double[]{0,5,10,15,20,25,30,35,40,45,50,55,60};
            //http://ipv4.fiddler http://localhost.fiddler
            string workerurl = "http://localhost.fiddler:48051/worker/simulation/MatejakAB2013_Kulhanek2013.fmu";
            rfmu.RestURLs = new string[]{workerurl};
            var results = rfmu.Simulate( pnames,pvalues,vnames,timepoints);
            if (results.Length!=3) throw new Exception("Test must return 3 arrays of results.");
            if (results[0].Length!=timepoints.Length) throw new Exception("Each result should have timepoints variables.");
            if (results[0][0].Length!=vnames.Length) throw new Exception("Each row should have same numbers as variable names.");
            
                for (int i = 0; i < results.Length; i++)
                {
                    for (int j = 0; j < results[i].Length; j++)
                    {
                        for (int k = 0; k < results[i][j].Length; k++)
                        {
                            Console.Write(results[i][j][k] + " ");
                        }
                        Console.WriteLine();
                    }
                    Console.WriteLine("----");
                }
                //TODO compare results
                for (int i = 0; i < testresults.Length; i++)
                {
                    if (Math.Abs(results[i][timepoints.Length - 1][1] - testresults[i]) > 1e-4) Console.WriteLine("difference between expected and retrieved result:" + testresults[i]+ " - "+ results[i][timepoints.Length - 1][1] );
                }
        }
    }
}
