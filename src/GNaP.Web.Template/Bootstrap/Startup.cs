using GNaP.Web.Template.Bootstrap;
using Microsoft.Owin;

[assembly: OwinStartup(typeof(Startup), "Configuration")]

namespace GNaP.Web.Template.Bootstrap
{
    using System;
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
            ConfigureWebApi(builder, basePath: "/api");
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

        private void ConfigureWebApi(IAppBuilder builder, string basePath)
        {
            var config = new HttpConfiguration();

            // Enable Swagger documentation
            // TODO: Think about styling the docs page
            // TODO: Setup authorization on docs, in case they should not be publically visible
            SwaggerUiConfig.Customize(c => c.DocExpansion = DocExpansion.List);
            SwaggerSpecConfig.Customize(c =>
            {
                c.ResolveBasePathUsing(request => request.RequestUri.GetLeftPart(UriPartial.Authority).TrimEnd('/') + basePath);

                // TODO: Find a better way to get to the XML documentation path
                c.IncludeXmlComments(String.Format(@"{0}\bin\GNaP.Web.Template.xml", AppDomain.CurrentDomain.BaseDirectory));
            });
            Swashbuckle.Bootstrapper.Init(config);

            // TODO: Define a proper api path (https://github.com/domaindrivendev/Swashbuckle/issues/137)
            config.Routes
                  .MapHttpRoute(name: "api_documentation",
                                routeTemplate: "docs", 
                                handler: new RedirectHandler("swagger/ui/index.html"), 
                                defaults: null, 
                                constraints: null);

            // Enable Attribute based routing
            config.MapHttpAttributeRoutes();

            // Old-style routing
            //config.Routes.MapHttpRoute(name: "DefaultApi",
            //                           routeTemplate: "{controller}/{id}",
            //                           defaults: new { id = RouteParameter.Optional });

            // TODO: Figure out how to get Swashbuckle respect the resolver below (https://github.com/domaindrivendev/Swashbuckle/issues/113)
            // Configure Web API return types to be properly camelCased
            config.Formatters
                  .JsonFormatter
                  .SerializerSettings
                  .ContractResolver = new CamelCasePropertyNamesContractResolver();

            builder.Map(basePath, inner =>
            {
                // Configure Web API
                inner.UseWebApi(config);

                // Needed to fix some IIS issues
                inner.UseStageMarker(PipelineStage.MapHandler);
            });
        }
    }
}