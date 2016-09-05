using System.Collections.Generic;
using System.Linq;
using RestMasterService.WebApp;
using ServiceStack;

namespace RestMasterService.ComputationNodes
{
    //TODO create abstract DTO and its repository pattern - 3 instances (Workers,Simulate,IdentifyDTO)

    [Route("/processes", "POST")]
    [Route("/processes/{Id}", "PUT")]
    public class IdentifyDTO
    {
        public long Id { get; set; }
        public string model { get; set; }
        public string name { get; set; }
        public long countcycles { get; set; }
        public string elapsedtime { get; set; }
        public string[] Variablenames { get; set; }
        //[StringLength(int.MaxValue)]
        public double[][] Variablevalues { get; set; }
        //[StringLength(int.MaxValue)]
        public double[][] Experimentalvalues { get; set; }
        public string[] Parameternames { get; set; }
//        public  OrderedDictionary <string, IdentifyParameters>  ParameterAssignment { get; set; }
        public double[] Parametervalues{ get; set; }
//        public double Ssq { get; set; }
        public long timepercycle { get; set; }
        public long simulationtime { get; set; }
        public long countpercycle { get; set; }
        public int workerspercycle { get; set; }
    }

    [Route("/processes")]
    [Route("/processes/{Ids}")]
    public class Identifys : IReturn<List<IdentifyDTO>>
    {
        public long[] Ids { get; set; }
        public Identifys(params long[] ids)
        {
            this.Ids = ids;
        }
    }


    public class IdentifyService : Service
    {        
        public IdentifyRepository Repository { get; set; }  //Injected by IOC

        public object Get(Identifys request)
        {
            return request.Ids.IsEmpty()
                ? Repository.GetAll()
                : Repository.GetByIds(request.Ids);
        }

        [OutgoingHub]
        public object Post(IdentifyDTO w)
        {
            return Repository.Store(w);
        }

        [OutgoingHub]
        public object Put(IdentifyDTO w)
        {
            return Repository.Store(w);
        }

        [OutgoingHub]
        public void Delete(Identifys request)
        {
            Repository.DeleteByIds(request.Ids);
        }
    }

    public class IdentifyRepository
    {
        List<IdentifyDTO> identifyDtos = new List<IdentifyDTO>();

/* move database feature to ResultRepository
        public void UploadFromDB()
            {
              using (IDbConnection db = "Data Source=localhost;Initial Catalog=restmasterservice;Integrated Security=True".OpenDbConnection())
              {
                identifyDtos = db.Select<IdentifyDTO>();
              }
            }

        public void StoreToDB()
        {
            using (IDbConnection db = "Data Source=localhost;Initial Catalog=restmasterservice;Integrated Security=True".OpenDbConnection())
            {
                db.InsertAll(identifyDtos);
                //identifyDtos = db.Select<IdentifyDTO>();
            }
            
        }

        public void InsertToDB(IdentifyDTO Idto)
        {
            using (IDbConnection db = "Data Source=localhost;Initial Catalog=restmasterservice;Integrated Security=True".OpenDbConnection())
            {
                db.Insert(Idto);
                //db.Update()
            }

        }

        public void UpdateToDB(IdentifyDTO Idto)
        {
            using (IDbConnection db = "Data Source=localhost;Initial Catalog=restmasterservice;Integrated Security=True".OpenDbConnection())
            {
                db.Update(Idto);                
            }

        }
*/


        public List<IdentifyDTO> GetByIds(long[] ids)
        {
            return identifyDtos.Where(x => ids.Contains(x.Id)).ToList();
        }

        public IdentifyDTO GetById(long id)
        {
            return identifyDtos.First(x => x.Id == id);
        }

        public List<IdentifyDTO> GetAll()
        {
            return identifyDtos;
        }

        public List<string> GetAllNames()
        {
            return identifyDtos.Select(p => p.name).ToList();
        }


        public IdentifyDTO Store(IdentifyDTO process)
        {
            var existing = identifyDtos.FirstOrDefault(x => x.Id == process.Id);
            if (existing == null)
            {
                var newId = identifyDtos.Count > 0 ? identifyDtos.Max(x => x.Id) + 1 : 1;
                process.Id = newId;
                identifyDtos.Add(process);
                //InsertToDB(process);
            }
            else
            {
                //fix bug of not being updated all items within dto stored - 
                //updated - not a fix at all - get;set attributes not set to all fields caused this bug
                //identifyDtos.Remove(existing);
                //identifyDtos.Add(process);
                existing.PopulateNonNull(process);
                //UpdateToDB(existing);
            }
            return process;
        }

        public void DeleteByIds(params long[] ids)
        {
            identifyDtos.RemoveAll(x => ids.Contains(x.Id));
        }

        public List<IdentifyDTO> GetByModelName(string p)
        {
            return identifyDtos.Where(x => p.Equals(x.model)).ToList();
            //throw new NotImplementedException();
        }

        /*public List<ResultResponseDTO> GetAllResultsMeta()
        {
            return identifyDtos.Select(p => new ResultResponseDTO(){Id= p.Id, Name = p.name}).ToList();
        }*/
    }
}