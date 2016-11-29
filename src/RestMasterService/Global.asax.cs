using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using RestMasterService.ComputationNodes;
//using ServiceStack.WebHost.Endpoints;
using System.Security.Principal;
using RestMasterService.WebApp;
using ServiceStack;
using ServiceStack.CacheAccess;
using ServiceStack.CacheAccess.Providers;
using ServiceStack.Common;
using ServiceStack.Configuration;
using ServiceStack.OrmLite;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.WebHost.Endpoints;

namespace RestMasterService
{
    public class Global : System.Web.HttpApplication
    {
        public class AppHost
       : AppHostBase
        {
            public AppHost() //Tell ServiceStack the name and where to find your web services
                : base("Workers Host", typeof(WorkersService).Assembly) { }

            public override void Configure(Funq.Container container)
            {
                //Set JSON web services to return idiomatic JSON camelCase properties
                ServiceStack.Text.JsConfig.EmitCamelCaseNames = true;

                //Configure User Defined REST Paths
                //Routes
                //  .Add<Hello>("/hello")
                //  .Add<Hello>("/hello/{Name*}");

                //Uncomment to change the default ServiceStack configuration
                //SetConfig(new EndpointHostConfig {
                //});

                //Enable Authentication
                //ConfigureAuth(container);

                //Register all your dependencies
                //container.Register(new TodoRepository());
                container.Register(new WorkersRepository());
                container.Register(new IdentifyRepository());
                container.Register(new ResultRepository());
                container.Register(new SimAppScreenRepository());

                //TODO test - is it ok to upload from DB here - or from instance creation
                var myrepository = container.Resolve<SimAppScreenRepository>();
                myrepository.UploadFromDB();

                container.Register(new GraphicRepository());
                var mygrrepository = container.Resolve<GraphicRepository>();
                mygrrepository.UploadFromDB();

//                SetConfig(new HostConfig { DefaultRedirectPath = "/WebApp/GenericUI.html" });
                //Set once before use (i.e. in a static constructor).
                OrmLiteConfig.DialectProvider = SqlServerDialect.Provider;
                //SqlServerDialect.Provider.
                //OrmLiteConfig.DialectProvider.DefaultStringLength = 

                using (IDbConnection db = "Data Source=localhost;Initial Catalog=restmasterservice;Integrated Security=True".OpenDbConnection())
                {
                    //db.CreateTable<ResultDTO>(false);
                    //db.Insert(new IdentifyDTO()ple { Id = 1, Name = "Hello, World!" });
                    //var rows = db.Select<SimpleExample>();

                    //Assert.That(rows, Has.Count(1));
                    //Assert.That(rows[0].Id, Is.EqualTo(1));
                }
            }

            /* Uncomment to enable ServiceStack Authentication and CustomUserSession */
            private void ConfigureAuth(Funq.Container container)
            {
                var appSettings = new AppSettings();
                //Register all Authentication methods you want to enable for this web app.
                Plugins.Add(new AuthFeature(() => new AuthUserSession(),
                    new IAuthProvider[] {
       }
                ));
                Plugins.Add(new AuthFeature(() => new AuthUserSession(),
        new IAuthProvider[] {
        new TwitterAuthProvider(appSettings),
        new FacebookAuthProvider(appSettings),
        new OAuthProvider(), 
  //      new GoogleOAuth2Provider(appSettings), 
    })
                {
                    HtmlRedirect = "~/",
//                    IncludeRegistrationService = true,
//                    MaxLoginAttempts = appSettings.Get("MaxLoginAttempts", 5),
                });
                Plugins.Add(new RegistrationFeature());

                container.Register<ICacheClient>(new MemoryCacheClient());
                var userRep = new InMemoryAuthRepository();
                container.Register<IUserAuthRepository>(userRep);
                //Default route: /auth/{provider}

            }


            /*public static void Start()
            {
                new AppHost().Init();
            }*/
        }
        void Application_Start(object sender, EventArgs e)
        {
            //new AppHost().Init();
            var myrepository = ((AppHostBase)EndpointHost.AppHost).Container.Resolve<ResultRepository>();
            myrepository.UploadFromDB();
            //var myrepository = HostContext.Container.Resolve<ResultRepository>();
            //myrepository.UploadFromDB();
            // Code that runs on application startup
            RouteTable.Routes.MapHubs();
            //RouteTable.Routes.IgnoreRoute("WebApp/{*pathInfo}");
            //RouteTable.Routes.MapHubs();

        }

        protected void Application_PostAuthenticateRequest()
        {
            var rolesheader = Context.Request.Headers["RolesHeader"];
            if (rolesheader == null) return;
            var userId = Context.Request.Headers["UserId"];
            if (userId == null) return;
            var roles = rolesheader.Split(',');
            var principal = new GenericPrincipal(new GenericIdentity(userId), roles);
            Context.User = principal;
        }

        void Application_End(object sender, EventArgs e)
        {
            //var myrepository = ((AppHostBase)EndpointHost.AppHost).Container.Resolve<ResultRepository>();
            //myrepository.StoreToDB();
            //  Code that runs on application shutdown

        }

        void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs

        }

        void Session_Start(object sender, EventArgs e)
        {
            // Code that runs when a new session is started

        }

        void Session_End(object sender, EventArgs e)
        {
            // Code that runs when a session ends. 
            // Note: The Session_End event is raised only when the sessionstate mode
            // is set to InProc in the Web.config file. If session mode is set to StateServer 
            // or SQLServer, the event is not raised.

        }

    }
}
