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

namespace sc2dsstats_mm_dev
{
    public class Program
    {
        public static int DEBUG = 0;
        public const string workdir = "/data";
        public const string myScan_log = workdir + "/log.txt";
        public const string replaydir = workdir + "/replays";
        public const string myreplaydir = workdir + "/myreplays";
        public const string detaildir = workdir + "/details";
        public const string commentdir = workdir + "/comments";
        public const string myJson_file = workdir + "/replays.json";
        public const string myReplays_file = workdir + "/myreplays.json";
        public const string ladder_file = workdir + "/ladder.json";
        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
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
