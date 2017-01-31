using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceClient.Web;

namespace FMUSingleNodeClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new JsonServiceClient("http://localhost:48048/");
            //TestFMUSimulateGet(client);
            //TestFMUSimulatePost(client);
            TestFMUSimulatePostTimePoints(client);
        }

        private static void TestFMUSimulateGet(JsonServiceClient client)
        {
            var response = client.Get<SimulateResponse>("/simulate/Time/gases.acidBase.ArtysPh.pH/gases.acidBase.ArtysPh.cHb");
            for (int i = 0; i < response.Result.Length; i++)
            {
                for (int j = 0; j < response.Result[i].Length; j++) Console.Write(response.Result[i][j] + " ");
                Console.WriteLine("");
            }
            Console.ReadKey();
        }

        private static void TestFMUSimulatePost(JsonServiceClient client)
        {
            var response = client.Post<SimulateResponse>("/simulate/Time/gases.acidBase.ArtysPh.pH/gases.acidBase.ArtysPh.cHb",
                                                         new Simulate()
                                                             {
                                                                 ParameterNames = new string[] {"gases.acidBase.ArtysPh.cHb"},
                                                                 ParameterValues = new double[] {41}
                                                             });
            for (int i = 0; i < response.Result.Length; i++)
            {
                for (int j = 0; j < response.Result[i].Length; j++) Console.Write(response.Result[i][j] + " ");
                Console.WriteLine("");
            }
            Console.ReadKey();
        }

        private static void TestFMUSimulatePostTimePoints(JsonServiceClient client)
        {
            var response = client.Post<SimulateResponse>("/simulate/Time/gases.acidBase.ArtysPh.pH/gases.acidBase.ArtysPh.cHb",
                                                         new Simulate()
                                                         {
                                                             ParameterNames = new string[] { "gases.acidBase.ArtysPh.cHb" },
                                                             ParameterValues = new double[] { 41 },
                                                             TimePoints = new double[] {0,333.51,992.64,1369.38,1808.73,2219.31,2551.56,2875.55,3186.94,3492.72,3797.46,4090.08,4375.36,4676.13,26372,58516.2,73114.2,73272,73434.1,73588,73734.7,73881.5,74028.6,74162.1,74302}
                                                         });
            for (int i = 0; i < response.Result.Length; i++)
            {
                for (int j = 0; j < response.Result[i].Length; j++) Console.Write(response.Result[i][j] + " ");
                Console.WriteLine("");
            }
            Console.ReadKey();
        }
    }
    //DTO response
    public class SimulateResponse
    {
        public double[][] Result { get; set; }
    }

    //DTO request

    public class Simulate
    {
        public Simulate()
        {
            Start = 0;
            Stop = 2592000;
            Steps = 200;
        }

        public string[] VariableNames { get; set; }
        public string[] ParameterNames { get; set; } //names of parameter in POST
        public double[] ParameterValues { get; set; } //values of the parameter in POST in the same order as ParameterNames
        public double[] TimePoints { get; set; }

        //        [System.ComponentModel.DefaultValue(0)]
        public int Start { get; set; }

        //[System.ComponentModel.DefaultValue(2592000)]
        public int Stop { get; set; }

        //[System.ComponentModel.DefaultValue(200)]
        public int Steps { get; set; }
    }
}
