using Microsoft.Owin;
using Template.Bootstrap;

[assembly: OwinStartup(typeof(Startup), "Configuration")]

namespace Template.Bootstrap
{
    using System;
    using System.Security.Claims;
    using System.Web.Http;
    using GNaP.Owin.Authentication.Jwt;
    using Microsoft.Owin.Extensions;
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.Jwt;
    using Newtonsoft.Json.Serialization;
    using Owin;
    using Properties;
    using Swashbuckle;
    using Swashbuckle.Application;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseWelcomePage();

            ConfigureAuth(app);
            ConfigureWebApi(app, basePath: "/api");
        }

        private void ConfigureAuth(IAppBuilder app)
        {
            var issuer = Settings.Default.Issuer;
            var audience = Settings.Default.Audience;
            var tokenSigningKey = Settings.Default.TokenSigningKey;

            app.UseJwtTokenIssuer(
                new JwtTokenIssuerOptions
                {
                    Issuer = issuer,
                    Audience = audience,
                    TokenSigningKey = tokenSigningKey,
                    Authenticate = (username, password) =>
                    {
                        // TODO: Implement your own authentication check here
                        if (username.Equals("gnap"))
                        {
                            return new[]
                            {
                                // TODO: Implement your own claims here
                                new Claim(ClaimTypes.AuthenticationInstant, DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")),
                                new Claim(ClaimTypes.AuthenticationMethod, AuthenticationTypes.Password),
                                new Claim(ClaimTypes.Name, username)
                            };
                        }

                        // Invalid user
                        return null;
                    }
                });

            app.UseJwtBearerAuthentication(
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

        private void ConfigureWebApi(IAppBuilder app, string basePath)
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
                c.IncludeXmlComments(String.Format(@"{0}\bin\GNaP.Template.WebApi.xml", AppDomain.CurrentDomain.BaseDirectory));
            });
            Bootstrapper.Init(config);

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

            app.Map(basePath, inner =>
            {
                // Configure Web API
                inner.UseWebApi(config);

                // Needed to fix some IIS issues
                inner.UseStageMarker(PipelineStage.MapHandler);
            });
        }
    }
}