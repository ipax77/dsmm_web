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

namespace dsmm_server
{
    public class Program
    {
        public static int DEBUG = 3;
        public static string workdir = "/data";
        public static string myConfig = workdir + "/config.json";
        public static string myScan_log = workdir + "/log.txt";
        public static string replaydir = workdir + "/replays";
        public static string myJson_file = workdir + "/data.json";

        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(workdir);
                    config.AddJsonFile(
                        "config.json", optional: true, reloadOnChange: false);
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
