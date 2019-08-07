using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace dsmm_server
{
    public class Program
    {
        public static int DEBUG = 0;
        public static string workdir = "/data";
        public static string myScan_log = workdir + "/log.txt";
        public static string replaydir = workdir + "/replays";
        public static string myJson_file = workdir + "/data.json";
        public static string ladder_file = workdir + "/ladder.json";

        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddSerilog(new LoggerConfiguration().WriteTo.File("/data/serilog.txt").CreateLogger());
                    logging.AddConsole();
                    logging.AddDebug();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        public static object Log(string msg, int Debug = 1)
        {
            if (DEBUG > 0 && DEBUG >= Debug)
                Console.WriteLine(msg);
            if (DEBUG > 1 && DEBUG >= Debug)
            {
                _readWriteLock.EnterWriteLock();
                File.AppendAllText(myScan_log, msg + Environment.NewLine);
                _readWriteLock.ExitWriteLock();
            }
            return null;
        }
    }
}
