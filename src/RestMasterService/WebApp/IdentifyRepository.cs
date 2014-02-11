using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestMasterService.WebApp
{
    public class IdentifyRepository<T>
        where T:struct 
    {
    }
    /*
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

        public class WorkersService : Service
        {
            //private readonly static WorkersService _instance = new WorkersService();
            public WorkersRepository Repository { get; set; }  //Injected by IOC

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

            [OutgoingHub]
            public object Post(Worker w)
            {
                return Repository.Store(w);
            }

            [OutgoingHub]
            public object Put(Worker w)
            {
                return Repository.Store(w);
            }

            [OutgoingHub]
            public void Delete(Workers request)
            {
                Repository.DeleteByIds(request.Ids);
            }
        }

        public class WorkersRepository
        {
            List<Worker> workers = new List<Worker>();

            public List<Worker> GetByIds(long[] ids)
            {
                return workers.Where(x => ids.Contains(x.Id)).ToList();
            }

            public List<Worker> GetByModelName(string model)
            {
                return workers.Where(x => model.Equals(x.ModelName)).ToList();
            }

            //returns model names which are available to compute among the workers
            public string[] GetModelNames()
            {
                return workers.Select(w => w.ModelName).Distinct().ToArray();
            }

            public List<Worker> GetAll()
            {
                return workers;
            }

            public Worker Store(Worker worker)
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
                return worker;
            }

            public void DeleteByIds(params long[] ids)
            {
                workers.RemoveAll(x => ids.Contains(x.Id));
            }

        }

    }
     */
}