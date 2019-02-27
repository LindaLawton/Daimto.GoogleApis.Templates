using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DaimtoTools.GAV4AspNetMvc.AuthenticateOAuth2.Auth;
using Microsoft.AspNetCore.Mvc;
using DaimtoTools.GAV4AspNetMvc.AuthenticateOAuth2.Models;
using Google.Apis.AnalyticsReporting.v4;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authorization;

namespace DaimtoTools.GAV4AspNetMvc.AuthenticateOAuth2.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public async  Task<IActionResult> Index()
        {

            //var token = new TokenResponse()
            //{
            //    AccessToken = HttpContext.User.Identities.FirstOrDefault()?.Claims.FirstOrDefault(c => c.Type.Equals("googleaccesstoken"))?.Value,
               
                
            //};

            //var x = new AuthorizationCodeFlow.Initializer("", "");
            //x.ClientSecrets = new ClientSecrets();
            //var m = new AuthorizationCodeFlow(x);
            
            //var cred = new UserCredential(new AuthorizationCodeFlow(x), "test", new TokenResponse()) { };


            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = "xxx.apps.googleusercontent.com",
                    ClientSecret = "xxxx"
                },
                Scopes = new List<string>() {AnalyticsReportingService.Scope.AnalyticsReadonly },
                DataStore = new HttpContextDataStore(HttpContext)
            });

            var token = new TokenResponse
            {
                AccessToken = HttpContext.User.Identities.FirstOrDefault()?.Claims.FirstOrDefault(c => c.Type.Equals("googleaccesstoken"))?.Value,
                RefreshToken = "[your_refresh_token_here]"
            };

            var credential = new UserCredential(flow, Environment.UserName, token);



            var service = new AnalyticsReportingService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Analyticsreporting Service account Authentication Sample",
            });



            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
