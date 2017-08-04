using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(IDTO.TravelerPortal.Startup))]
namespace IDTO.TravelerPortal
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
