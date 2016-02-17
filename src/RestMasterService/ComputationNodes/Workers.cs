
using System;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;
using NLog;
using PostSharp.Patterns.Diagnostics;
using RestMasterService.WebApp;
using ServiceStack.Configuration;
using ServiceStack.Common;
//using ServiceStack.ServiceHost;
//using ServiceStack.ServiceInterface;
//using ServiceStack.ServiceInterface.Auth;
//using ServiceStack.ServiceInterface.ServiceModel;
//using ServiceStack.WebHost.Endpoints;
using PostSharp.Extensibility;
using ServiceStack;

//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;

namespace RestMasterService.ComputationNodes
{

    [Route("/workersByName/{ModelName}", "GET")]
    [Route("/workers")]
    [Route("/workers/{Ids}")]
    
    public class Workers : IReturn<List<Worker>>
    {
        public string ModelName { get; set; }
        public long[] Ids { get; set; }        
        public Workers(params long[] ids)
        {
            this.Ids = ids;
        }
    }
    
    [Route("/workers", "POST")]
    [Route("/workers/{Id}", "PUT")]
    public class Worker : IReturn<Worker>
    {
        public long Id { get; set; }
        public string ModelName { get; set; }
        public string RestUrl { get; set; } //endpoint of web service - 
        public string WorkerType { get; set; } //the type of the worker - could be cloud/grid - REST web service, or boinc - SOAP web service
        public int Priority { get; set; } //for future usage
    }

    public class ModelDTO : IReturn<List<ModelDTO>> 
    {
        public int count { get; set; }
        public string name { get; set; }

    }

    public class WorkersService : Service
    {
        //private readonly static WorkersService _instance = new WorkersService();
        public WorkersRepository Repository { get; set; }  //Injected by IOC
        //Logger logger = LogManager.GetLogger("MyClassName");

        public object Get(Workers request)
        {
            if (request.ModelName.IsEmpty())
            return request.Ids.IsEmpty()
                ? Repository.GetAll()
                : Repository.GetByIds(request.Ids);
            else
            {
                return Repository.GetByModelName(request.ModelName);
            }
        }


        [Log, OutgoingHub]
        public object Post(Worker w)
        {
            //logger.Debug("post " + w.ToString());
            return Repository.Store(w);
        }


        [Log, OutgoingHub]
        public object Put(Worker w)
        {
            //logger.Debug("put " + w.ToString());
            return Repository.Store(w);
        }

        [Log, OutgoingHub]
        public void Delete(Workers request)
        {
            //logger.Debug("delete "+request.ToString());
            Repository.DeleteByIds(request.Ids);
        }
    }

    public class WorkersRepository
    {
        List<Worker> workers = new List<Worker>();
        private readonly object _workerLock = new object();

        public List<Worker> GetByIds(long[] ids)
        {
            return workers.Where(x => ids.Contains(x.Id)).ToList();
        }

        public List<Worker> GetByModelName(string model)
        {
            return workers.Where(x => model.Equals(x.ModelName)).ToList();
        }

        //returns model names which are available to compute among the workers
        public List<ModelDTO> GetModelNames()
        {
            var models = workers.Select(w => w.ModelName).Distinct().Select(m => new ModelDTO(){count =1, name = m}).ToList();//.ToArray();
            foreach (var model in models) model.count = GetWorkersPerModelCount(model.name);
            return models;

            //var workerspermodel = models.Select(m => new ModelDTO() {count = GetWorkersPerModelCount(m), name = m});//.ToList();
            //return workerspermodel;
        }
        private int GetWorkersPerModelCount(string name)
        {
            return workers.Count(w => w.ModelName == name);
        }

        
        public List<Worker> GetAll()
        {
            return workers;
        }


        
        public Worker Store(Worker worker)
        {
            lock (_workerLock)
            {
                var existing = workers.FirstOrDefault(x => x.Id == worker.Id);
                if (existing == null)
                {
                    var newId = workers.Count > 0 ? workers.Max(x => x.Id) + 1 : 1;
                    worker.Id = newId;
                    workers.Add(worker);
                }
                else
                {
                    existing.PopulateWith(worker);
                }
            }
            return worker;
        }

        
        public void DeleteByIds(params long[] ids)
        {
            lock (_workerLock)
            {
                workers.RemoveAll(x => ids.Contains(x.Id));
            }
        }

    }

}