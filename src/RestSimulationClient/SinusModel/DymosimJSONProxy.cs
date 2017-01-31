using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.ServiceModel.Web;
using NLog;
using RestSharp;
using RestSharp.Deserializers;

//using csmatio.io;
//using csmatio.types;

namespace DymolaModel
{
    //instead of calling some other DLL, it calls the dymosim.dll directly
    public static class DymosimJSONProxy
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static RestClient client = null;
//        private const string SERVER_REST_URL_2 = "http://ipv4.fiddler:48045/websim/rest/";
//        private const string SERVER_REST_URL = "http://ipv4.fiddler/websim/rest/";
//        private const string SERVER_REST_URL = "http://localhost/websim/rest/";
        private const string SERVER_REST_URL = "http://localhost/persim/rest/";
        private const string SimModelResource = "simulation/model/";
        private const string SimVariablesResource = "simulation/variables/";
        private const string SimMultiVectorResource = "simulation/multivector/";
        private const string SimMultiBinResource = "simulation/binaryvector/";
        private const string MatlabResFile = "dsres.mat";
        private const string MetaResource = "meta/";
        private const int ROWCOLLUMNS = 50;
        private const int ROWCOLLUMNSBINARY = 10;
        private const int namestopost_length = 1000;
        //private const string DymosimResource = "dymosimmodel/";
        public static DymolaModel MyDymolaModel = new DymolaModel();
        public static string ModelId;
        public static bool persistent = true; //false for default small models, true for default big models ;


        public static void SetRESTURL(string url)
        {
            //WebRequest.DefaultWebProxy = new WebProxy("127.0.0.1", 8888);
            client = new RestClient(url);
            //PUT - create model instance and variables on server
            //CreateParams();

        }

        private static void InitRestClient()
        {
            if (client==null)
            {
                client = new RestClient(SERVER_REST_URL);
            }
        }


       private static void ReadSimulatorValues()
       {
           ReadSimulatorValuesMatFile();
       }

        private static void ReadSimulatorValuesMatFile()
        {

            MatFile.read(MatlabResFile);
            var curveNames = MatFile.getCurveNames();
            //double[] timePoints = MatFile.getTimePoints();
            var dataWithTimePoints = MatFile.getDataWithTimePoints(false);
            var datarow = new float[dataWithTimePoints.GetLength(1)];
            Buffer.BlockCopy(dataWithTimePoints, 0, datarow, 0, dataWithTimePoints.GetLength(1) * sizeof(float));
            MyDymolaModel.VariableValues = new Dictionary<string, float[]>();
            MyDymolaModel.VariableValues.Add("time", datarow);
            for (var i = 0; i < curveNames.Length; i++)
            {
                datarow = new float[dataWithTimePoints.GetLength(1)];
                //array datawithtimepoints is shifted by 1 from array curveNames - curvenames doesn't have timepoints
                Buffer.BlockCopy(dataWithTimePoints, (i + 1) * dataWithTimePoints.GetLength(1) * sizeof(float), datarow, 0, dataWithTimePoints.GetLength(1) * sizeof(float));
                MyDymolaModel.VariableValues.Add(curveNames[i], datarow);
            }
        }

/*
        private static void ReadSimulatorValuesCSMatIO()
        {
            MatFileReader mfr = new MatFileReader(MatlabResFile);
            MyDymolaModel.VariableValues = new Dictionary<string, float[]>();
            Dictionary<string, MLArray> mfvariables = mfr.Content;
            foreach (var mfvariable in mfvariables)
            {
                MyDymolaModel.VariableValues.Add(mfvariable.Key,((MLDouble) mfvariable.Value).GetArray()[0]);
            }
        }
        */

//        [DllImport("DymolaSimulator.dll",CharSet = CharSet.Ansi, EntryPoint = "RunDymolaSimulator")  ]
//        public static extern unsafe Int32 RunDymolaSimulator(String dymosimdll, String dsin, String dsres);
        [DllImport("dymosim.dll")]
        public static extern IntPtr DymosimModelFunctions();

        [DllImport("libdsdll.dll")]
        public static extern Int32 dymosimMainModel (Int32 argc, String[] argv,IntPtr fPtr);

        //public static extern Int32 DymosimInitialize(struct DymosimSimulator*);
/* Ensures that the initial section has been run. Return true if ok, false otherwise */



        public static int RunDymolaSimulator(string inputfile)
        {
            return dymosimMainModel(3, new[]
                                           {   "dymosim.exe",
                                               inputfile,
                                               MatlabResFile
                                           }, DymosimModelFunctions());

        }

        //update variable values
        public static void UpdateVariable(string VariableName)
        {
            var request = new RestRequest(Method.POST);
            request.Resource = SimModelResource+ModelId+"/"+VariableName;
            request.RequestFormat = DataFormat.Json;            
            float[] value;
            if (MyDymolaModel.VariableValues.TryGetValue(VariableName, out value))
            request.JsonSerializer.Serialize(value);
            InitRestClient();
            client.Execute(request);
        }

        public static void RegisterModel(bool append)
        {
            if (persistent) RegisterBigModel(append);
            else RegisterSmallModel(append);
        }

        //first creation of params on server
        //bug outofmemory when registering big model
        public static void RegisterSmallModel(bool append)
        {
            ReadSimulatorValues();
            var request = append ? new RestRequest(Method.POST) : new RestRequest(Method.PUT);
            if (ModelId.Length > 0) request.Resource = SimModelResource + ModelId;
            else request.Resource = SimModelResource;
            request.RequestFormat = DataFormat.Json;
            request.AddBody(MyDymolaModel);
            InitRestClient();
            var response = client.Execute(request); //expecting integer
            ModelId = response.Content;//.Substring(1,response.Content.Length-2);
            logger.Debug("succesfully registered as "+ModelId);
        }

        public static void RegisterBigModel(bool append)
        {
            var p = MatFile.readHeader(MatlabResFile);

            InitRestClient();
            RestRequest request;
            if (!append)
            {
                //first call will be PUT to force to create new entity Model
                request = new RestRequest(Method.PUT);
                append = true;
            }
            else
            {
                //other variables will be added to the existing entity
                request = new RestRequest(Method.POST);
            }
            logger.Debug("url:"+SimModelResource+ModelId);
            if ((ModelId!=null) &&(ModelId.Length > 0)) request.Resource = SimVariablesResource + ModelId;
            else throw new Exception("cannot register model without modelid");//request.Resource = "dymosimmodel/";
            bool dataAvailable = true;
            var variables = MatFile.getCurveNamesWithTime();
            var tmpvariables = new string[namestopost_length];
            int vi = 0;
            while (dataAvailable)
            {
                request.Resource = SimVariablesResource + ModelId;
                request.RequestFormat = DataFormat.Json;
                //fix bug - at the end of the big array there may be indexoutofarray
                if (namestopost_length > variables.Length - vi)
                {
                    tmpvariables = new string[variables.Length - vi];
                    Array.Copy(variables, vi, tmpvariables, 0, variables.Length-vi);
                }else Array.Copy(variables, vi, tmpvariables, 0, namestopost_length);
                vi += namestopost_length;
                dataAvailable = vi < variables.Length;
                request.AddBody(tmpvariables);
                var response = client.Execute(request); //register variable names
                if (response.StatusCode != HttpStatusCode.OK) throw new Exception("Error when sending variable names to server:"+response.StatusCode+ " "+ response.Content);
                logger.Debug("variables " + vi +" of "+variables.Length+ " saved.");
                if (dataAvailable) request = new RestRequest(Method.POST);
            }
            //SubmitValuesVector(p);
            SubmitValuesVectorBinary(p);
            p.Close();
        }

        private static void SubmitValuesVector(MatFile p)
        {
            RestRequest request;
            bool dataAvailable;
//float[] row;
            var rows = new float[ROWCOLLUMNS][];
            //int rowi = 0;
            dataAvailable = true;
            while (dataAvailable)
            {
                for (int i = 0; i < ROWCOLLUMNS; i++) dataAvailable = (rows[i] = p.getRowDataSection()) != null;
                request = new RestRequest(Method.POST);
                request.Resource = SimMultiVectorResource + ModelId;
                request.RequestFormat = DataFormat.Json;

                request.AddBody(rows);
                InitRestClient();
                // debug measure without sending  
                var response = client.Execute(request); //register variable values
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception("Error when sending variables to server:" + response.Content);
            }
        }

        private static void SubmitValuesVectorBinary(MatFile p)
        {
            RestRequest request;
            bool dataAvailable;
            //float[] row;
            //var rows = new float[ROWCOLLUMNS][];
            var rows = new byte[ROWCOLLUMNSBINARY*p.getRowSizeinBytes()];
            var row = new byte[p.getRowSizeinBytes()];
            //new List<byte>();//[ROWCOLLUMNS*1023];
            //int rowi = 0;
            dataAvailable = true;
            int writtenbytes = 0;
            int lastrowindex = 0;
            while (dataAvailable)
            {
                for (int i = 0; i < ROWCOLLUMNSBINARY; i++) { 
                    dataAvailable = (row = p.getRowDataSectionBytes()) != null;
                    if (dataAvailable) { 
                        Buffer.BlockCopy(row,0,rows,i*p.getRowSizeinBytes(),row.Length);
                        writtenbytes += p.getRowSizeinBytes();
                    } else
                    {
                        lastrowindex= i;
                        break;
                    }
                }
                if (!dataAvailable)
                {
                    Array.Resize(ref rows, lastrowindex*p.getRowSizeinBytes());
                }
                request = new RestRequest(Method.POST);
                request.Resource = SimMultiBinResource + ModelId+"/"+p.getRowSizeinBytes();
                //request.RequestFormat = DataFormat.Json;
                request.AddFile("vectors", rows,"vectors.bin");
                //request.AddBody(rows);
                InitRestClient();
                // debug measure without sending  
                var response = client.Execute(request); //register variable values
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception("Error when sending variable names to server:" + response.Content);
            }
        }

        //get parameters, expects that full model with variables and values will be returned 
        public static DymolaModel GetModel()
        {
            var request = new RestRequest(Method.GET);
            request.Resource = SimModelResource + ModelId;
            InitRestClient();
            IRestResponse response = client.Execute (request);
            var json = new JsonDeserializer();
            return json.Deserialize<DymolaModel>(response);
        }

        internal static void InitializeSimulator(string p)
        {
            throw new NotImplementedException();
        }

        internal static void SetParameter(string key, string value)
        {
            throw new NotImplementedException();
        }

        internal static void RunSimulator()
        {
            throw new NotImplementedException();
        }

        public static void Delete()
        {
            var request = new RestRequest(Method.DELETE) { Resource = SimModelResource + ModelId };//, RequestFormat = DataFormat.Json };
            InitRestClient();
            client.Execute(request);
            
        }
        public static void RegisterModelMeta(string path,string id)
        {
            ModelId = id;
            var request = new RestRequest(Method.POST) {Resource = MetaResource + ModelId, RequestFormat = DataFormat.Json};
            var mm = new ModelMeta {ModelInstancePath = path};
            DymosimFile.ParseMetaSimulationFile("dsin.txt",mm);
            request.AddBody(mm);
            //request.JsonSerializer.Serialize()
            //request.JsonSerializer.Serialize(mm);
            InitRestClient();
            
            client.Execute(request);
        }

        public static void GetNextSimulationParameters()
        {
            var request = new RestRequest(Method.GET) { Resource = MetaResource + ModelId+"/parameters", RequestFormat = DataFormat.Json };
            //var mm = new ModelMeta { ModelInstancePath = path };
            //DymosimFile.ParseMetaSimulationFile("dsin.txt", mm);
            //request.AddBody(mm);
            //request.JsonSerializer.Serialize()
            //request.JsonSerializer.Serialize(mm);
            InitRestClient();
            var response = client.Execute(request);
            //TODO do something with the response :-)            
        }
    }
}
