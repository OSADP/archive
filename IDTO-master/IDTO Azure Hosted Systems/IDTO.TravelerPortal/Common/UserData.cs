using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Newtonsoft.Json;

namespace IDTO.TravelerPortal.Common
{
    public class UserData
    {
        public string FirstName { get; set; }
        public UserData()
        {
            FirstName = "Unknown";
        }

    }

    /// <summary>
    /// Binder to pull the UserData out for any actions that may want it.
    /// </summary>
    public class UserDataModelBinder<T> : IModelBinder
    {

        public object BindModel(ControllerContext controllerContext,
        ModelBindingContext bindingContext)
        {
            if (bindingContext.Model != null)
                throw new InvalidOperationException("Cannot update instances");
            if (controllerContext.RequestContext.HttpContext.Request.IsAuthenticated)
            {
                var cookie = controllerContext
                .RequestContext
                .HttpContext
                .Request
                .Cookies[FormsAuthentication.FormsCookieName];
                if (null == cookie)
                    return null;
                var decrypted = FormsAuthentication.Decrypt(cookie.Value);
                if (!string.IsNullOrEmpty(decrypted.UserData))
                    return JsonConvert.DeserializeObject<T>(decrypted.UserData);
            }
            return null;
        }
    }
}