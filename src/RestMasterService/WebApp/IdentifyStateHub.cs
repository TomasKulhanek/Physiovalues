
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using RestMasterService.ComputationNodes;


using System;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;
using RestMasterService.WebApp;
using ServiceStack.Configuration;
using ServiceStack.Common;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.ServiceInterface.ServiceModel;
using ServiceStack.WebHost.Endpoints;

namespace RestMasterService.WebApp
{
    [HubName("fakeidentifystate")]
    public class IdentifyStateHub : Hub
    {

        private readonly IdentifyStateTicker _identStateTicker;

        public IdentifyStateHub() : this(IdentifyStateTicker.Instance) { }

        public IdentifyStateHub(IdentifyStateTicker identStateTicker)
        {
            _identStateTicker = identStateTicker;
        }

        public override Task OnConnected()
        {
                var myrepository = ((AppHostBase) EndpointHost.AppHost).Container.Resolve<WorkersRepository>();
                var resultsRepository = ((AppHostBase)EndpointHost.AppHost).Container.Resolve<ResultRepository>();


            Clients.Client(Context.ConnectionId).updateModelsResults(myrepository.GetModelNames(),resultsRepository.GetAllResultsMeta());
            return base.OnConnected();
            //return Clients.All.joined(Context.ConnectionId, DateTime.Now.ToString());
        }

        public void Send(string message)
        {
            Clients.All.addMessage(message);
        }

        public void SendParameters(string[][] parameters)
        {
            Clients.All.currentParams(parameters);
        }

        public void StartIdentify(string[][] initialparameters, string [] variablenames, double[][] experiment_data,string modelName,double[] computationParams)
        {
            _identStateTicker.SetUri(Context.Request.Url.Port);
            _identStateTicker.SetParameters(initialparameters);
            _identStateTicker.SetExperimentVariables(variablenames);
            _identStateTicker.SetExperimentValues(experiment_data);
            _identStateTicker.SetModelToIdentify(modelName);
            _identStateTicker.SetComputationParams(computationParams);
            _identStateTicker.StartIdentify();
        }

        public void StartIdentifyObjects(IdentifyParameters[] initialparameters)
        {
            _identStateTicker.SetParameters(initialparameters);
            _identStateTicker.StartIdentify();
        }

        public void StopIdentify()
        {
            _identStateTicker.StopIdentify();
        }
        
        public string[] UpdateIdentifyProcess(IdentifyDTO identify)
        {
            Clients.All.updateIdentifyProcess(identify);
            /*
            if ((identify.Variablenames != null) && (identify.Variablevalues != null))
            {
                Clients.All.updateVariables(identify.Variablenames, identify.Variablevalues);
            }
            if ((identify.Parameternames != null) && (identify.Parametervalues != null))
            {
                Clients.All.updateParameters(identify.Parameternames, identify.Parametervalues);
            }
            Clients.All.updateCountCycles(identify.countcycles);*/
            //_identStateTicker.BroadcastIdentifyStateMessage();
            var myrepository = ((AppHostBase)EndpointHost.AppHost).Container.Resolve<WorkersRepository>();
            //var workers = (List<Worker>) myrepository.GetByModelName(identify.model);//TODO move from singleton computation to multiple computation
            var workers = (List<Worker>)myrepository.GetByModelName(_identStateTicker.GetModelToIdentify());
            return workers.Select(x => x.RestUrl).ToArray();
        }
    }
}
