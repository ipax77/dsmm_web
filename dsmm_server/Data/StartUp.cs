using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using dsmm_server.Models;
using System.Threading;

namespace dsmm_server.Data
{
    public class StartUp
    {
        private IConfiguration _config;
        public static string VERSION = "v0.1";
        public Dictionary<string, UserConfig> Conf = new Dictionary<string, UserConfig>();
        public HashSet<string> Players = new HashSet<string>();

        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();

        public StartUp(IConfiguration config)
        {
            _config = config;
            if (!File.Exists(Program.myConfig))
                File.Create(Program.myConfig).Dispose();
            else
                _config.Bind("Config", Conf);

            foreach (string name in Conf.Keys)
                Players.Add(Conf[name].Player);
        }

        public void Save()
        {
            _readWriteLock.EnterWriteLock();

            Dictionary<string, Dictionary<string, UserConfig>> temp = new Dictionary<string, Dictionary<string, UserConfig>>();
            temp.Add("Config", Conf);
            var json = JsonSerializer.Serialize(temp);
            File.WriteAllText(Program.myConfig, json);

            foreach (string name in Conf.Keys)
                Players.Add(Conf[name].Player);

            _readWriteLock.ExitWriteLock();
        }
    }
}
