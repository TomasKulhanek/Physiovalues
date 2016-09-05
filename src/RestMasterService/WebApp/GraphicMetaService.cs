using System.Collections.Generic;

namespace RestMasterService.WebApp
{
    [Route("/graphicmetas")]
    public class GraphicMetaDTO : IReturn<List<GraphicMetaDTO>>
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }



    //only GET 
    public class GraphicMetaService : Service
    {
        public GraphicRepository Repository { get; set; }  //Injected by IOC

        public object Get(GraphicMetaDTO request)
        {
            if (request.Id != 0) return Repository.GetMetaById(request.Id);
            //if (!string.IsNullOrEmpty(request.ReferencedModelName)) return Repository.GetByModelName(request.ReferencedModelName);
            return Repository.GetAllMeta();
        }
    }

}