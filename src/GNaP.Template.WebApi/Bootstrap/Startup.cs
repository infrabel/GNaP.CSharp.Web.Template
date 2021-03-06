﻿using Microsoft.Owin;
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
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using Owin;
    using Properties;

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

            // Enable Attribute based routing
            config.MapHttpAttributeRoutes();

            // Old-style routing
            //config.Routes.MapHttpRoute(name: "DefaultApi",
            //                           routeTemplate: "{controller}/{id}",
            //                           defaults: new { id = RouteParameter.Optional });

            // Configure JSON.NET to properly camelCase replies and to not fail on reference loops
            var jsonSettings = config.Formatters.JsonFormatter.SerializerSettings;
            jsonSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

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