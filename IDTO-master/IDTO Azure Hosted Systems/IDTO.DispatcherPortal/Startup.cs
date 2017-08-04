using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(IDTO.DispatcherPortal.Startup))]
namespace IDTO.DispatcherPortal
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
