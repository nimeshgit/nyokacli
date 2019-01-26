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
            var host = WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseKestrel(options => {
                    options.Limits.MaxRequestBodySize = 10000000000L;
                })
                .Build();

            Console.WriteLine("Host created, starting host");

            host.Run();

            Console.WriteLine("Host started");
        }
    }
}
