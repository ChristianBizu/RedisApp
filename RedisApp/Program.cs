using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RedisApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            LaunchRedisServer();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static void LaunchRedisServer()
        {
            ProcessStartInfo proc = new ProcessStartInfo();

            proc.UseShellExecute = true;
            proc.WorkingDirectory = Environment.CurrentDirectory + "\\Redis64";
            proc.FileName = "redis-server.exe";

            try
            {
                Process.Start(proc);
            }
            catch (Exception ex)
            {
                //log error
                var a = 2;
            }
        }
    }
}
