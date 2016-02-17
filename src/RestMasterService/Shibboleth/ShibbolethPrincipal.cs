using System;
using System.Security.Principal;
using System.Web;

namespace RestMasterService.Shibboleth
{
    public class ShibbolethPrincipal : GenericPrincipal
    {
        public string username
        {
            get { return this.Identity.Name.Replace("@cuni.cz", ""); }
        }

        public string firstname
        {
            get { return HttpContext.Current.Request.Headers["givenName"]; }
        }

        public string lastname
        {
            get { return HttpContext.Current.Request.Headers["surname"]; }
        }

        public string phone
        {
            get { return HttpContext.Current.Request.Headers["telephoneNumber"]; }
        }

        public string mobile
        {
            get { return HttpContext.Current.Request.Headers["mobile"]; }
        }

        public string entitlement
        {
            get { return HttpContext.Current.Request.Headers["eduzgEntitlement"]; }
        }

        public string homeOrganization
        {
            get { return HttpContext.Current.Request.Headers["homeOrganization"]; }
        }

        public DateTime birthday
        {
            get
            {
                DateTime dtHappy = DateTime.MinValue;
                try
                {
                    dtHappy = DateTime.Parse(HttpContext.Current.Request.Headers["dateOfBirth"]);
                }
                finally
                {

                }

                return dtHappy;
            }
            set { }
        }

        public ShibbolethPrincipal()
            : base(new GenericIdentity(GetUserIdentityFromHeaders()), GetRolesFromHeader())
        {
        }

        public static string GetUserIdentityFromHeaders()
        {
            //return HttpContext.Current.Request.Headers["eppn"];            
            return HttpContext.Current.Request.Headers["principalName"];
        }

        public static string[] GetRolesFromHeader()
        {
            string[] roles = null;
            //string rolesheader = HttpContext.Current.Request.Headers["affiliation"];
            string rolesheader = HttpContext.Current.Request.Headers["eduzgEntitlement"];
            if (rolesheader != null)
            {
                roles = rolesheader.Split(';');
            }
            return roles;
        }
    }
}