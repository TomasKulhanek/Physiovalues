using ServiceStack.ServiceInterface;

namespace DistributeSimulationService
{
    public class HelloService : Service
    {
        public object Any(Hello request)
        {
            return new HelloResponse { Result = "Hello, " + request.Name };
        }
    }
}