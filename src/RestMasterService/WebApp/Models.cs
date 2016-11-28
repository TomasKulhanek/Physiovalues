using System.Collections.Generic;
using ServiceStack;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace RestMasterService.WebApp
{
    [Route("/Models")]
    public class ModelsDTO : IReturn<List<string>>
    {        
        public string Name { get; set; }        
    }

    //only GET 
    public class ModelsService : Service
    {
        public SimAppScreenRepository Repository { get; set; }  //Injected by IOC

        public object Get(ModelsDTO request)
        {
            return Repository.GetAllSimulators();
        }
    }
    public class Models
    {
    }
}