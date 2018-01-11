using System;
using System.Web;
using System.Web.Http;

namespace PlanetaKinoScheduleChecker.Api.Host
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(ConfigurationCallback);
        }

        private void ConfigurationCallback(HttpConfiguration config)
        {
            AutofacConfig.ConfigureDi(config);
            WebApiConfig.Register(config);

        }
    }
}
