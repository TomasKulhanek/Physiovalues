using System.Collections.Generic;
using System.Data;
using System.Linq;
using RestMasterService.WebApp;
using ServiceStack.Common.Extensions;
using ServiceStack.OrmLite;
using ServiceStack.Redis.Support;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace RestMasterService.ComputationNodes
{
    [Route("/results", "POST")]
    [Route("/results/{Id}", "PUT")]
    public class ResultDTO
    {
        public long Id { get; set; }
        public string model { get; set; }
        public string name { get; set; }
        public long countcycles { get; set; }
        public string elapsedtime { get; set; }
        public string[] Variablenames { get; set; }
        public double[][] Variablevalues { get; set; }
        public double[][] Experimentalvalues { get; set; }
        public string[] Parameternames { get; set; }
        public IdentifyParameters [] ParameterAssignment { get; set; }
        public double[] Parametervalues { get; set; }
        public double Ssq { get; set; }
    }

    [Route("/results")]
    [Route("/results/{Ids}")]
    public class Results : IReturn<List<ResultDTO>>
    {
        public long[] Ids { get; set; }
        public Results(params long[] ids)
        {
            this.Ids = ids;
        }
    }

    public class ResultMetaDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }


    public class ResultService : Service
    {        
        public ResultRepository Repository { get; set; }  //Injected by IOC

        public object Get(Results request)
        {
            return request.Ids.IsEmpty()
                ? Repository.GetAllResultsMeta()
                : Repository.GetByIds(request.Ids);
        }

        [OutgoingHub]
        public object Post(ResultDTO w)
        {
            return Repository.Store(w);
        }

        [OutgoingHub]
        public object Put(ResultDTO w)
        {
            return Repository.Store(w);
        }

        [OutgoingHub]
        public void Delete(Results request)
        {
            Repository.DeleteByIds(request.Ids);
        }
    }

    public class ResultRepository
    {
        List<ResultDTO> ResultDtos = new List<ResultDTO>();

        public void UploadFromDB()
            {
              using (IDbConnection db = "Data Source=localhost;Initial Catalog=restmasterservice;Integrated Security=True".OpenDbConnection())
              {
                ResultDtos = db.Select<ResultDTO>();
              }
            }

        public void StoreToDB()
        {
            using (IDbConnection db = "Data Source=localhost;Initial Catalog=restmasterservice;Integrated Security=True".OpenDbConnection())
            {
                db.InsertAll(ResultDtos);
            }
            
        }

        public void InsertToDB(ResultDTO Idto)
        {
            using (IDbConnection db = "Data Source=localhost;Initial Catalog=restmasterservice;Integrated Security=True".OpenDbConnection())
            {
                db.Insert(Idto);
            }

        }

        public void UpdateToDB(ResultDTO Idto)
        {
            using (IDbConnection db = "Data Source=localhost;Initial Catalog=restmasterservice;Integrated Security=True".OpenDbConnection())
            {
                db.Update(Idto);                
            }

        }
        public void DeleteFromDB(params long[] Ids)
        {
            using (IDbConnection db = "Data Source=localhost;Initial Catalog=restmasterservice;Integrated Security=True".OpenDbConnection())
            {
                db.DeleteByIdParam<ResultDTO>(Ids);
            }

        }



        public List<ResultDTO> GetByIds(long[] ids)
        {
            return ResultDtos.Where(x => ids.Contains(x.Id)).ToList();
        }

        public ResultDTO GetById(long id)
        {
            return ResultDtos.First(x => x.Id == id);
        }

        public List<ResultDTO> GetAll()
        {
            return ResultDtos;
        }

        public List<string> GetAllNames()
        {
            return ResultDtos.Select(p => p.name).ToList();
        }


        public ResultDTO Store(ResultDTO resultDto)
        {
            var existing = ResultDtos.FirstOrDefault(x => x.Id == resultDto.Id);
            if (existing == null)
            {
                var newId = ResultDtos.Count > 0 ? ResultDtos.Max(x => x.Id) + 1 : 1;
                resultDto.Id = newId;
                ResultDtos.Add(resultDto);
                InsertToDB(resultDto);
            }
            else
            {
                //fix bug of not being updated all items within dto stored - 
                existing.PopulateNonNull(resultDto);
                //UpdateToDB(existing);
            }
            return resultDto;
        }

        public void DeleteByIds(params long[] ids)
        {
            ResultDtos.RemoveAll(x => ids.Contains(x.Id));
            DeleteFromDB(ids);
        }

        public List<ResultDTO> GetByModelName(string p)
        {
            return ResultDtos.Where(x => p.Equals(x.model)).ToList();
            //throw new NotImplementedException();
        }

        public List<ResultDTO> GetAllResultsMeta()
        {
            return ResultDtos.Select(p => new ResultDTO(){Id= p.Id, name = p.name, countcycles=p.countcycles,Ssq=p.Ssq}).ToList();
        }
    }
}
