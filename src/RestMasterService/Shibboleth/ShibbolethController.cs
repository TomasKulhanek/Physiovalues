using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestMasterService.Shibboleth
{
    public class ShibbolethController 
    {
        protected new ShibbolethPrincipal User { get { return null; }
            //get { return (base as ShibbolethPrincipal) ?? null; //ShibbolethPrincipal.GetUnauthorizedPrincipal();
            //}
        }
    }
}