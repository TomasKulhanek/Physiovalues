using System.Collections.Generic;
using System.Data;
using System.Linq;
using ServiceStack;
using ServiceStack.Common.Extensions;
using ServiceStack.OrmLite;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;


//using RestMasterService.WebApp;
//using ServiceStack.Redis.Support;

namespace RestMasterService.WebApp
{
    [Route("/graphics", "POST")]
    [Route("/graphics/{Id}", "PUT")]
    public class GraphicDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public byte[] Definition { get; set; } //blob
    }

    [Route("/graphics")]
    [Route("/graphics/{Ids}")]
    [Route("/graphicname/{Name}")]
    public class Graphics : IReturn<List<GraphicDTO>>
{
    public long[] Ids { get; set; }
    public string Name { get; set; }
    public Graphics(params long[] ids)
{
    this.Ids = ids;
}
}

    public class GraphicService : Service
    {        
        public GraphicRepository Repository { get; set; }  //Injected by IOC

        public object Get(Graphics request)
    {
        return (request.Ids.IsEmpty())
                   ? Repository.GetAllByName(request.Name)
                   : Repository.GetByIds(request.Ids);
    }

        //[OutgoingHub]
        public object Post(GraphicDTO w)
        {
            return Repository.Store(w);
        }

        //[OutgoingHub]
        public object Put(GraphicDTO w)
        {
            return Repository.Store(w);
        }

        //[OutgoingHub]
        public void Delete(Graphics request)
    {
        Repository.DeleteByIds(request.Ids);
    }
    }

    public class GraphicRepository
    {
        List<GraphicDTO> _graphicDtos = new List<GraphicDTO>();

        public void UploadFromDB()
        {
            using (IDbConnection db = "Data Source=localhost;Initial Catalog=restmasterservice;Integrated Security=True".OpenDbConnection())
            {
                _graphicDtos = db.Select<GraphicDTO>();
            }
        }

        public void StoreToDB()
        {
            using (IDbConnection db = "Data Source=localhost;Initial Catalog=restmasterservice;Integrated Security=True".OpenDbConnection())
            {
                db.InsertAll(_graphicDtos);
            }
            
        }

        public void InsertToDB(GraphicDTO Idto)
        {
            using (IDbConnection db = "Data Source=localhost;Initial Catalog=restmasterservice;Integrated Security=True".OpenDbConnection())
            {
                db.Insert(Idto);
            }

        }

        public void UpdateToDB(GraphicDTO Idto)
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
                //db.DeleteByIdParam<GraphicDTO>(Ids);
                db.DeleteByIds<GraphicDTO>(Ids);
            }

        }



        public List<GraphicMetaDTO> GetMetaByIds(long[] ids)
        {
            return _graphicDtos.Where(x => ids.Contains(x.Id)).Select(p => new GraphicMetaDTO() { Id = p.Id, Name = p.Name }).ToList();
            //return graphicDtos.Where(x => ids.Contains(x.Id)).ToList();
        }

        public List<GraphicDTO> GetByIds(long[] ids)
        {
            return _graphicDtos.Where(x => ids.Contains(x.Id)).ToList();
            //return graphicDtos.Where(x => ids.Contains(x.Id)).ToList();
        }

        public GraphicDTO GetById(long id)
        {
            return _graphicDtos.First(x => x.Id == id);
        }

        public GraphicMetaDTO GetMetaById(long id)
        {
            return _graphicDtos.Where(p => p.Id == id).Select(p => new GraphicMetaDTO() { Id = p.Id, Name = p.Name }).First();
        }

        public List<GraphicDTO> GetAllByName(string p)
        {

            return (p.IsNullOrEmpty())?GetAll():_graphicDtos.Where(x => p.Equals(x.Name)).ToList();
            //throw new NotImplementedException();
        }

        public List<GraphicDTO> GetAll()
        {
            return _graphicDtos;
        }

        public List<string> GetAllNames()
        {
            return _graphicDtos.Select(p => p.Name).ToList();
        }


        public GraphicDTO Store(GraphicDTO graphicDto)
        {
            var existing = _graphicDtos.FirstOrDefault(x => x.Id == graphicDto.Id);
            if (existing == null)
            {
                var newId = _graphicDtos.Count > 0 ? _graphicDtos.Max(x => x.Id) + 1 : 1;
                graphicDto.Id = newId;
                _graphicDtos.Add(graphicDto);
                InsertToDB(graphicDto);
            }
            else
            {
                //fix bug of not being updated all items within dto stored - 
                //existing.PopulateNonNull(graphicDto);
                _graphicDtos.Remove(existing);
                _graphicDtos.Add(graphicDto);
                UpdateToDB(graphicDto);
            }
            return graphicDto;
        }

        public void DeleteByIds(params long[] ids)
        {
            _graphicDtos.RemoveAll(x => ids.Contains(x.Id));
            DeleteFromDB(ids);
        }

        public List<GraphicDTO> GetByModelName(string p)
        {
            return _graphicDtos.Where(x => p.Equals(x.Definition)).ToList();
            //throw new NotImplementedException();
        }

        public List<GraphicMetaDTO> GetAllMeta()
        {
            return _graphicDtos.Select(p => new GraphicMetaDTO(){Id= p.Id, Name = p.Name}).ToList();
        }
    }
}
