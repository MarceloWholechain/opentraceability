using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace DiagnosticsTool
{
    public static class WebServiceFactory
    {
        public static Uri? TestBaseAddress = null;

        public static IWebHost Create(string url, IConfiguration config)
        {
            TestBaseAddress = new Uri(url);

            var webhost = WebHost.CreateDefaultBuilder(args: new string[] { })
                                 .UseConfiguration(config)
                                 .UseStartup<Startup>()
                                 .UseKestrel()
                                 .UseDefaultServiceProvider((context, options) =>
                                 {
                                     options.ValidateOnBuild = true;
                                 })
                                 .UseUrls(TestBaseAddress.ToString())
                                 .Build();
            webhost.Start();
            return webhost;
        }
    }
}
