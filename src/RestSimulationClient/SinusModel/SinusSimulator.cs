using System;
using System.Threading;
using RestSharp;
using RestSharp.Deserializers;

namespace DymolaModel
{
    public static class SinusSimulator
    {
        private static RestClient client;
        private const string SERVER_REST_URL = "http://localhost/simulace/rest";
        public static SinusModel MySinusModel = new SinusModel();
        public static int ModelId;
        static SinusSimulator ()
        {
            client = new RestClient(SERVER_REST_URL);
            //PUT - create model instance and variables on server
            CreateParams();
        }


        public static void StartSimulation(int seconds)
        {
            MySinusModel.Modeltime = 0;
            do
            {
                //TODO after each step - send params using SPRING AOP method
                Thread.Sleep(200);
                if (MySinusModel.Modeltime > 0.2 && MySinusModel.Modeltime < 2)
                {
                    SinusModel controlModel = GetParams();
                    if (controlModel!=null)
                   Console.WriteLine("equals?:"+(Math.Abs(controlModel.Variable - MySinusModel.Variable) < 0.00001) +"t:"+controlModel.Modeltime+" v:"+controlModel.Variable);
                }
                MySinusModel.Step(); 
                SendParams();
            } while (MySinusModel.Modeltime < seconds);
        }

        //any other reuse of created params
        public static void SendParams()
        {
            var request = new RestRequest(Method.POST);
            request.Resource = "model/"+ModelId;
            request.RequestFormat = DataFormat.Json;
            request.AddBody(MySinusModel);
//            request.JsonSerializer.Serialize(MySinusModel);
            client.Execute(request);
            Console.WriteLine("t:"+MySinusModel.Modeltime+" v:"+MySinusModel.Variable);
        }

        //first creation of params on server simkernel
        public static void CreateParams()
        {
            var request = new RestRequest(Method.PUT);
            request.Resource = "model";
            request.JsonSerializer.Serialize(MySinusModel);
            IRestResponse response = client.Execute(request); //expecting integer
            ModelId = Convert.ToInt32(response.Content);
        }

        public static SinusModel GetParams()
        {
            var request = new RestRequest(Method.GET);
            request.Resource = "model/" + ModelId;
            IRestResponse response = client.Execute (request);
            var json = new JsonDeserializer();
            return json.Deserialize<SinusModel>(response);
        }
    }
}