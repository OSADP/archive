using IDTO.BasicHttpAuth.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http;

namespace IDTO.BasicAuthentication
{
    public class BasicAuthentication
    {
        public static void Init()
        {
            var config = System.Configuration.ConfigurationManager.GetSection("basicAuth");
            var basicAuth = (Configuration.BasicAuthenticationConfigurationSection)config;
            IDictionary<string, string> activeUsers = new Dictionary<string, string>();

            for (int i = 0; i < basicAuth.Credentials.Count; i++)
            {
                var credential = basicAuth.Credentials[i];
                activeUsers.Add(credential.UserName, credential.Password);
            }

            GlobalConfiguration.Configuration
                .MessageHandlers.Add(new BasicAuthMessageHandler()
                {
                    PrincipalProvider = new DictionaryPrincipalProvider(activeUsers)
                });
        }
    }
}