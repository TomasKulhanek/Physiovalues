using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;

//using ServiceStack.ServiceHost;
//using ServiceStack.ServiceInterface;
//using ServiceStack.ServiceInterface.Auth;


namespace RestMasterService.Shibboleth
{
    public class ShibbolethAuthProvider : CredentialsAuthProvider
    {
        public ShibbolethPrincipal sp;// = new ShibbolethPrincipal();

            public override bool TryAuthenticate(IServiceBase authService,
                string userName, string password)
            {
                //Add here your custom auth logic (database calls etc)
                //Return true if credentials are valid, otherwise false
                sp = new ShibbolethPrincipal();
                return sp.firstname!=null;
            }

/*        public override IHttpResult OnAuthenticated(IServiceBase authService, IAuthSession session, IAuthTokens tokens, Dictionary<string, string> authInfo)
        {
            session.FirstName = sp.firstname;
            return base.OnAuthenticated(authService, session, tokens, authInfo);
        }
        */

        /*public override void OnAuthenticated(IServiceBase authService, IAuthSession session, IAuthTokens tokens, Dictionary<string, string> authInfo)
        {
            session.FirstName = sp.firstname;
            base.OnAuthenticated(authService, session, tokens, authInfo);
        }*/
        

        public override void OnAuthenticated(IServiceBase authService,
                IAuthSession session, IOAuthTokens tokens,
                Dictionary<string, string> authInfo)
            {
                //Fill IAuthSession with data you want to retrieve in the app eg:
                session.FirstName = "some_firstname_from_db";
                //...

                //Call base method to Save Session and fire Auth/Session callbacks:
                //return base.OnAuthenticated(authService, session, tokens, authInfo);

                //Alternatively avoid built-in behavior and explicitly save session with
                //authService.SaveSession(session, SessionExpiry);
                //return null;
            }

    }
}