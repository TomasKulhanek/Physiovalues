using System.Collections.Generic;
using ServiceStack;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace RestMasterService.WebApp
{
    [Route("/SimAppScreenMetas")]
    [Route("/SimAppScreenMetas/{ReferencedModelName}")]
    public class SimAppScreenMetaDTO : IReturn<List<SimAppScreenMetaDTO>>
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string ReferencedModelName { get; set; }
    }

    //only GET 
    public class SimAppScreenMetasService : Service
    {
        public SimAppScreenRepository Repository { get; set; }  //Injected by IOC

        public object Get(SimAppScreenMetaDTO request)
        {
            if (request.Id != 0) return Repository.GetMetaById(request.Id);
            if (!string.IsNullOrEmpty(request.ReferencedModelName)) return Repository.GetByModelName(request.ReferencedModelName);
            return Repository.GetAllSimAppScreensMeta();
        }
    }
}