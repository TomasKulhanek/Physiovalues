using System;
using System.Linq;
using ServiceStack;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace RestMasterService.ComputationNodes
{


    [Route("/resultdetails/{id}")]
    public class ResultDetailRequestByIdDTO : IReturn<IdentifyDTO>
    {
        public long Id { get; set; }
    }

    public class ResultDetailService : Service
    {
        public ResultRepository Repository { get; set; }
        public object Get(ResultDetailRequestByIdDTO request)
        {
            //var modelRepository =
            return (request.Id == 0)
                       ? Repository.GetAll().First()
                       : Repository.GetById(request.Id);
        }
        public object Post(ResultDetailRequestByIdDTO w)
        {
            throw new NotImplementedException();
            //return Repository.Store(w);
        }

        public object Put(ResultDetailRequestByIdDTO w)
        {
            throw new NotImplementedException();
            //            return Repository.Store(w);
        }

        public void Delete(ResultDetailRequestByIdDTO request)
        {
            throw new NotImplementedException();
            //            Repository.DeleteByIds(request.Ids);
        }
    }

}