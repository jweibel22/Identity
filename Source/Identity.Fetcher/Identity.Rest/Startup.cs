using System;
using System.Web;
using System.Web.Http;
using log4net.Config;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Ninject;
using Ninject.Web.Common;
using Ninject.Web.Common.OwinHost;
using Ninject.Web.WebApi.OwinHost;
using Owin;

[assembly: OwinStartup(typeof(Identity.Rest.Startup))]

namespace Identity.Rest
{
    public class Startup
    {
        public static OAuthBearerAuthenticationOptions OAuthBearerOptions { get; private set; }

        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            app.UseNinjectMiddleware(CreateKernel);
            app.UseNinjectWebApi(config);

            ConfigureOAuth(app);
            WebApiConfig.Register(config);

            app.UseWebApi(config);

            AutoMapperConfiguration.Configure();

            XmlConfigurator.Configure();
        }

        private void ConfigureOAuth(IAppBuilder app)
        {
            OAuthBearerOptions = new OAuthBearerAuthenticationOptions();
            //Token Consumption
            app.UseOAuthBearerAuthentication(OAuthBearerOptions);
        }

        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                kernel.Load<WebModule>();

                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }
    }
}