using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using ServiceStack;
using ServiceStack.Common;
using ServiceStack.OrmLite;
using ServiceStack.ServiceInterface;

namespace RestMasterService.WebApp
{
    [Route("/SimAppScreens", "POST")]
    [Route("/SimAppScreens/{Id}", "PUT")]
    public class SimAppScreenDTO 
    {
        public long Id { get; set; }        
        public string ReferencedModelName { get; set; }
        public string Name { get; set; }
        public string ScreenDefinition { get; set; } //holds definition in Screen Definition Language - SDL
    }
    
    [Route("/SimAppScreens")]
    [Route("/SimAppScreens/{Ids}")]
    public class SimAppScreens : IReturn<List<SimAppScreenDTO>>
{
    public long[] Ids { get; set; }
    public SimAppScreens(params long[] ids)
{
    this.Ids = ids;
        
}
        
}

    public class SimAppScreenService : Service
    {        
        public SimAppScreenRepository Repository { get; set; }  //Injected by IOC
   

        public object Get(SimAppScreens request)
    {
        return request.Ids.IsEmpty()
                   ? Repository.GetAll()
                   : Repository.GetByIds(request.Ids);
    }

        //[OutgoingHub] signalr hub - callback for new object in repository
        public object Post(SimAppScreenDTO w)
        {
            return Repository.Store(w);
        }

        //[OutgoingHub]
        public object Put(SimAppScreenDTO w)
        {
            return Repository.Store(w);
        }

        //[OutgoingHub]
        public void Delete(SimAppScreens request)
    {
        Repository.DeleteByIds(request.Ids);
    }
    }

    public class SimAppScreenRepository
    {
        List<SimAppScreenDTO> SimAppScreenDtos = new List<SimAppScreenDTO>();

        public void UploadFromDB()
        {
            using (IDbConnection db = AppHost.OpenDbConnection())
            {
                SimAppScreenDtos = db.Select<SimAppScreenDTO>();
                foreach (var scr in SimAppScreenDtos)
                    scr.ScreenDefinition = HttpUtility.UrlDecode(scr.ScreenDefinition);
            }
        }

        public void StoreToDB()
        {
            using (IDbConnection db = AppHost.OpenDbConnection())
            {
                //TODO some encode
                db.InsertAll(SimAppScreenDtos);
            }
            
        }

        public void InsertToDB(SimAppScreenDTO Idto)
        {
            using (IDbConnection db = AppHost.OpenDbConnection())
            {
                var Idtoenc = new SimAppScreenDTO()
                {
                    Id = Idto.Id,
                    Name = Idto.Name,
                    ReferencedModelName = Idto.ReferencedModelName,
                    ScreenDefinition = HttpUtility.UrlEncode(Idto.ScreenDefinition)
                };
                db.Insert(Idtoenc);
            }

        }

        public void UpdateToDB(SimAppScreenDTO Idto)
        {
            using (IDbConnection db = AppHost.OpenDbConnection())
            {
                var Idtoenc = new SimAppScreenDTO()
                                  {
                                      Id = Idto.Id,
                                      Name = Idto.Name,
                                      ReferencedModelName = Idto.ReferencedModelName,
                                      ScreenDefinition = HttpUtility.UrlEncode(Idto.ScreenDefinition)
                                  };
                //Idto.ScreenDefinition = HttpUtility.UrlEncode(Idto.ScreenDefinition);
                db.Update(Idtoenc);                
            }

        }
        public void DeleteFromDB(params long[] Ids)
        {
            using (IDbConnection db = AppHost.OpenDbConnection())
            {
                db.DeleteByIds<SimAppScreenDTO>(Ids);
                //db.Delete(Ids);
            }

        }



        public List<SimAppScreenDTO> GetByIds(long[] ids)
        {
            return SimAppScreenDtos.Where(x => ids.Contains(x.Id)).ToList();
        }

        public SimAppScreenDTO GetById(long id)
        {
            return SimAppScreenDtos.First(x => x.Id == id);
        }

        public List<SimAppScreenMetaDTO> GetMetaById(long id)
        {
            return SimAppScreenDtos.Where(x => x.Id ==id).Select(y  =>  new SimAppScreenMetaDTO() {Id = y.Id, Name = y.Name, ReferencedModelName = y.ReferencedModelName}).ToList();
            
            ;//Select(p => new SimAppScreenMetaDTO() { Id = p.Id, Name = p.Name });
        }

        public List<SimAppScreenDTO> GetAll()
        {
            return SimAppScreenDtos;
        }

        public List<string> GetAllNames()
        {
            return SimAppScreenDtos.Select(p => p.Name).ToList();
        }


        public SimAppScreenDTO Store(SimAppScreenDTO SimAppScreenDto)
        {
            var existing = SimAppScreenDtos.FirstOrDefault(x => x.Id == SimAppScreenDto.Id);
            if (existing == null)
            {
                var newId = SimAppScreenDtos.Count > 0 ? SimAppScreenDtos.Max(x => x.Id) + 1 : 1;
                SimAppScreenDto.Id = newId;                    
                SimAppScreenDtos.Add(SimAppScreenDto);
                InsertToDB(SimAppScreenDto);
            }
            else
            {
                //fix bug of not being updated all items within dto stored - 
                //SimAppScreenDto);
                //existing = SimAppScreenDto;
                //SimAppScreenDto.Id = existing.Id;
                SimAppScreenDtos.Remove(existing);
                SimAppScreenDtos.Add(SimAppScreenDto);
                UpdateToDB(SimAppScreenDto);
            }
            return SimAppScreenDto;
        }

        public void DeleteByIds(params long[] ids)
        {
            SimAppScreenDtos.RemoveAll(x => ids.Contains(x.Id));
            DeleteFromDB(ids);
        }

        public List<SimAppScreenMetaDTO> GetByModelName(string p)
        {
            return SimAppScreenDtos.Where(x => p.Equals(x.ReferencedModelName)).Select(y  =>  new SimAppScreenMetaDTO() {Id = y.Id, Name = y.Name, ReferencedModelName = y.ReferencedModelName}).ToList();
            //throw new NotImplementedException();
        }

        public List<SimAppScreenMetaDTO> GetAllSimAppScreensMeta()
        {
            return SimAppScreenDtos.Select(p => new SimAppScreenMetaDTO(){Id= p.Id, Name = p.Name, ReferencedModelName = p.ReferencedModelName}).ToList();
        }

        public List<string> GetAllSimulators()
        {
            return SimAppScreenDtos.Select(p => p.ReferencedModelName).Distinct().ToList();
        }
    }
}
