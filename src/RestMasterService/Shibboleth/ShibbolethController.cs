using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RestMasterService.Shibboleth
{
    public class ShibbolethController : Controller
    {
        protected new ShibbolethPrincipal User
        {
            get { return (base.User as ShibbolethPrincipal) ?? null; //ShibbolethPrincipal.GetUnauthorizedPrincipal();
            }
        }
    }
}