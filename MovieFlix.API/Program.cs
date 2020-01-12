using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace MovieFlix
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
            proc.WorkingDirectory = Environment.CurrentDirectory + "\\Lib\\Redis64";
            proc.FileName = "redis-server.exe";
            proc.Arguments = "redis.windows.conf";
            //proc.CreateNoWindow = true;
            //proc.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

            Process.Start(proc);
        }
    }
}
