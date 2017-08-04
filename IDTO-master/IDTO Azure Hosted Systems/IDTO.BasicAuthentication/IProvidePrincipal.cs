using System.Security.Principal;

namespace IDTO.BasicHttpAuth.Web
{
    public interface IProvidePrincipal
    {
        IPrincipal CreatePrincipal(string username, string password);
    }
}