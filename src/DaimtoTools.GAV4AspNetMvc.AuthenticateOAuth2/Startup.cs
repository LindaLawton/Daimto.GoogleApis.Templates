using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace DaimtoTools.GAV4AspNetMvc.AuthenticateOAuth2
{
    //https://accounts.google.com/.well-known/openid-configuration

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            // https://www.jerriepelser.com/blog/authenticate-oauth-aspnet-core-2/

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "Cookies";
                    options.DefaultChallengeScheme = "Google";
                })
                .AddCookie("Cookies")
                .AddOAuth("Google", options =>
                {
                    options.ClientId = "1046123799103-i6cjd1hkjntu5bkdkjj5cdnpcu4iju8p.apps.googleusercontent.com";
                    options.ClientSecret = "XXXXX";
                    options.CallbackPath = new PathString("/signin-google");
                    options.Scope.Add("https://www.googleapis.com/auth/analytics.readonly");
                    options.Scope.Add("profile");
                    options.AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/auth";
                    options.TokenEndpoint = "https://oauth2.googleapis.com/token";
                    options.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";

                    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Locality, "locale");
                    options.ClaimActions.MapJsonKey("urn:google:link", "link");
                    options.ClaimActions.MapJsonKey("urn:google:picture", "picture");

                    options.Events = new OAuthEvents
                    {
                        OnCreatingTicket = async context =>
                        {
                            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

                            var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                            response.EnsureSuccessStatusCode();

                            var user = JObject.Parse(await response.Content.ReadAsStringAsync());
                            var claims = new List<Claim>
                            {
                                new Claim("googleaccesstoken",  context.AccessToken)
                            };
                            
                            context.RunClaimActions(user);

                            var claimsIdentity = (ClaimsIdentity)context.Principal.Identity;
                            //add your custom claims here
                            claimsIdentity.AddClaim(new Claim("googleaccesstoken", context.AccessToken));

                        }
                    };
                });
                //.AddOpenIdConnect("oidc", options =>
                //{
                //    options.Authority = "https://accounts.google.com/o/oauth2/auth0";
                //    options.ClientId = "1046123799103-i6cjd1hkjntu5bkdkjj5cdnpcu4iju8p.apps.googleusercontent.com";
                //    options.SaveTokens = true;
                //    options.Scope.Add("testapi");
                //});

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
