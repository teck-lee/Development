using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace MiniProgram
{
    /// <summary>
    /// The Main function can be used to run the ASP.NET Core application locally using the Kestrel webserver.
    /// </summary>
    public class LocalEntryPoint
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
              //.UseKestrel()
              //.UseContentRoot(Directory.GetCurrentDirectory())
              //.UseIISIntegration()
              .UseStartup<Startup>()
              .Build();
    }
}