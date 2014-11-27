using GNaP.Web.Template.Bootstrap;
using Microsoft.Owin;

[assembly: OwinStartup(typeof(Startup), "Configuration")]

namespace GNaP.Web.Template.Bootstrap
{
    using System;
    using System.Linq;
    using System.Net.Http.Formatting;
    using System.Web.Http;
    using Microsoft.Owin.Extensions;
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.Jwt;
    using Newtonsoft.Json.Serialization;
    using Owin;
    using Properties;
    using Swashbuckle.Application;

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

            // Enable Swagger documentation
            // TODO: Find a better way to get to the XML documentation path
            // TODO: Figure out how to get Swashbuckle respect the resolver below (https://github.com/domaindrivendev/Swashbuckle/issues/113)
            // TODO: Define a proper api path (https://github.com/domaindrivendev/Swashbuckle/issues/137)
            // TODO: Think about styling the docs page
            Swashbuckle.Bootstrapper.Init(config);
            SwaggerSpecConfig.Customize(c => c.IncludeXmlComments(String.Format(@"{0}\bin\GNaP.Web.Template.XML", AppDomain.CurrentDomain.BaseDirectory)));
            SwaggerUiConfig.Customize(c => c.DocExpansion = DocExpansion.List);

            // Enable Attribute based routing
            config.MapHttpAttributeRoutes();

            // Old-style routing
            //config.Routes.MapHttpRoute(name: "DefaultApi",
            //                           routeTemplate: "api/{controller}/{id}",
            //                           defaults: new { id = RouteParameter.Optional });

            // Configure Web API return types to be properly camelCased
            config.Formatters
                  .JsonFormatter
                  .SerializerSettings
                  .ContractResolver = new CamelCasePropertyNamesContractResolver();

            builder.UseWebApi(config);

            builder.UseStageMarker(PipelineStage.MapHandler);
        }
    }
}