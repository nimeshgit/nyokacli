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
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseKestrel(options => {
                    if (options.Limits.MaxRequestBodySize.HasValue)
                    {
                        System.Console.WriteLine(options.Limits.MaxRequestBodySize.Value);
                    }
                    else
                    {
                        System.Console.WriteLine("Kestrel does not have a max request vody size by default, it seems");
                    }
                    
                    options.Limits.MaxRequestBodySize = null;
                })
                .Build()
                .Run();
        }
    }
}
