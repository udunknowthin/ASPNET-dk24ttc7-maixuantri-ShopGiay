using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ShopGiay.Startup))]
namespace ShopGiay
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
