using System.Collections.Generic;
using System.Security.Principal;

namespace IDTO.BasicHttpAuth.Web
{
    public class DictionaryPrincipalProvider : IProvidePrincipal
    {
        IDictionary<string, string> activeUsers;

        public DictionaryPrincipalProvider(IDictionary<string, string> activeUsers)
        {
            this.activeUsers = activeUsers;
        }

        public IPrincipal CreatePrincipal(string username, string password)
        {
            if (activeUsers.ContainsKey(username) && activeUsers[username] == password)
            {
                var identity = new GenericIdentity(username);
                IPrincipal principal = new GenericPrincipal(identity, new[] { "User" });
                return principal;
            }

            return null;
        }
    }
}