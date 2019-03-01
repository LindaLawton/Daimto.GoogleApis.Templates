using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DaimtoTools.GAV4AspNetMvc.AuthenticateOAuth2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            var clientInfo = ClientInfo.Load();

            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureServices(services =>
                {
                    services.AddSingleton(clientInfo);
                })
                .Build();
        }
    }
}
