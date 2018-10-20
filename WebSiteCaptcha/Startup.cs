using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebSiteCaptcha.Startup))]
namespace WebSiteCaptcha
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
