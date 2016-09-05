
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(RestMasterService.App_Start.Startup1))]

namespace RestMasterService.App_Start
{
    public class Startup1
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}
