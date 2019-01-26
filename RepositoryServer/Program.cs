using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RepositoryServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Creating host...");
            var unbuilt_host = WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseKestrel(options => {
                    options.Limits.MaxRequestBodySize = 10000000000L;
                });

            Console.WriteLine("Building host...");

            var built_host = unbuilt_host.Build();

            Console.WriteLine("Host created, starting host");

            built_host.Run();

            Console.WriteLine("Host started");
        }
    }
}
