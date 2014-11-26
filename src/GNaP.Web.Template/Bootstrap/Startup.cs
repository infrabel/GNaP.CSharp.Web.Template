using GNaP.Web.Template.Bootstrap;
using Microsoft.Owin;

[assembly: OwinStartup(typeof(Startup), "Configuration")]

namespace GNaP.Web.Template.Bootstrap
{
    using System.Linq;
    using System.Net.Http.Formatting;
    using System.Web.Http;
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.Jwt;
    using Newtonsoft.Json.Serialization;
    using Owin;
    using Properties;

    public class Startup
    {
        public void Configuration(IAppBuilder builder)
        {
            ConfigureAuth(builder);
            ConfigureWebApi(builder);
        }

        private void ConfigureAuth(IAppBuilder builder)
        {
            var issuer = Settings.Default.Issuer;
            var audience = Settings.Default.Audience;
            var tokenSigningKey = Settings.Default.TokenSigningKey;

            builder.UseJwtBearerAuthentication(
                new JwtBearerAuthenticationOptions
                {
                    AuthenticationMode = AuthenticationMode.Active,
                    AllowedAudiences = new[] { audience },
                    IssuerSecurityTokenProviders = new IIssuerSecurityTokenProvider[]
                                                   {
                                                       new SymmetricKeyIssuerSecurityTokenProvider(issuer, tokenSigningKey)
                                                   }
                });
        }

        private void ConfigureWebApi(IAppBuilder builder)
        {
            var config = new HttpConfiguration();

            // Enable Attribute based routing
            config.MapHttpAttributeRoutes();

            // Old-style routing
            //config.Routes.MapHttpRoute(name: "DefaultApi",
            //                           routeTemplate: "api/{controller}/{id}",
            //                           defaults: new { id = RouteParameter.Optional });

            // Configure Web API return types to be properly camelCased
            var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            builder.UseWebApi(config);   
        }
    }
}