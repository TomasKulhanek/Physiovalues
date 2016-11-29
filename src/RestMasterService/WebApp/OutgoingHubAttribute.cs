using System;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hosting;
using RestMasterService.ComputationNodes;
using ServiceStack;
using ServiceStack.ServiceHost;
using ServiceStack.WebHost.Endpoints;

//using IRequest = ServiceStack.Web.IRequest;

//using IRequest = Microsoft.AspNet.SignalR.IRequest;

//using ServiceStack.ServiceHost;
//using ServiceStack.Web;
//using ServiceStack.WebHost.Endpoints;

namespace RestMasterService.WebApp
{
    //this responsefilter is ussed to integrate ServiceStack and SignalR - when added,deleted worker, the updated list is sent to all connected clients
    public class OutgoingHubAttribute : Attribute, IHasResponseFilter
    {

        
  /*      public void ResponseFilter(IRequest req, IResponse res, object responseDto)
        {
            //TODO avoid memory leaks
            var hub = GlobalHost.ConnectionManager.GetHubContext<IdentifyStateHub>();
            if (hub != null)
            {
                
                if ((responseDto==null) || (responseDto.GetType()==typeof(Worker)))
                {
                    var myrepository = HostContext.Container.Resolve<WorkersRepository>();
                    hub.Clients.All.updateModels(myrepository.GetModelNames());
                }                
                else if (responseDto.GetType()==typeof(IdentifyDTO)) hub.Clients.All.updateIdentifyProcess(responseDto);
            }
        }*/


        public void ResponseFilter(IHttpRequest req, IHttpResponse res, object response)
        {

            //TODO avoid memory leaks
            var hub = GlobalHost.ConnectionManager.GetHubContext<IdentifyStateHub>();
            if (hub != null)
            {

                if ((response == null) || (response.GetType() == typeof(Worker)))
                {
                    //                    var myrepository = HostContext.Container.Resolve<WorkersRepository>();
                    var myrepository = ((AppHostBase)EndpointHost.AppHost).Container.Resolve<WorkersRepository>();
                    hub.Clients.All.updateModels(myrepository.GetModelNames());
                }
                else if (response.GetType() == typeof(IdentifyDTO)) hub.Clients.All.updateIdentifyProcess(response);
            }
        }

        public IHasResponseFilter Copy()
        {
            return this;
        }

        public int Priority
        {
            get
            {
                return -1;   
            }
        }
    }
}