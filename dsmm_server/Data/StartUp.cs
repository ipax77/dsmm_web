using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using dsmm_server.Models;
using dsmm_server.Data;
using System.Threading;
using System.Collections.Concurrent;
using dsweb_electron6.Models;
using Microsoft.EntityFrameworkCore;
using DSmm.Repositories;
using DSmm.Models;
using dsweb_electron6.Data;

namespace dsmm_server.Data
{
    public class StartUp
    {
        public static string VERSION = "v0.1";
        public HashSet<string> Players = new HashSet<string>();
        //public Dictionary<string, MMplayerNG> Conf = new Dictionary<string, MMplayerNG>();
        public ConcurrentDictionary<string, MMplayerNG> MMplayers { get; set; } = new ConcurrentDictionary<string, MMplayerNG>();
        public ConcurrentDictionary<string, MMplayerNG> MMraces { get; set; } = new ConcurrentDictionary<string, MMplayerNG>();
        public ConcurrentDictionary<int, List<dsreplay>> replays = new ConcurrentDictionary<int, List<dsreplay>>();
        public HashSet<string> repHash = new HashSet<string>();
        public Dictionary<string, string> Auth = new Dictionary<string, string>();

        private DbContextOptions<MMdb> _mmdb;

        public StartUp(DbContextOptions<MMdb> mmdb)
        {
            _mmdb = mmdb;

            foreach (string cmdr in DSdata.s_races)
                MMraces.TryAdd(cmdr, new MMplayerNG(cmdr));

            /**
            using (var db = new MMdb(_mmdb))
            {
                foreach (var ent in db.MMdbPlayers)
                    db.MMdbPlayers.Remove(ent);
                foreach (var ent in db.MMdbRatings)
                    db.MMdbRatings.Remove(ent);
                db.SaveChanges();
            }
            **/
            // /**
            using (var db = new MMdb(_mmdb))
            {
                foreach (var ent in db.MMdbPlayers)
                    MMplayers[ent.Name] = new MMplayerNG(ent);

                foreach (var ent in db.MMdbRaces)
                    MMraces[ent.Name] = new MMplayerNG(ent);

                foreach (var ent in db.MMdbRatings) 
                    if (MMplayers.ContainsKey(ent.MMdbPlayer.Name))
                        if (MMplayers[ent.MMdbPlayer.Name].AuthName == ent.MMdbPlayer.AuthName)
                            MMplayers[ent.MMdbPlayer.Name].Rating[ent.Lobby] = new MMPlRating(ent);

                foreach (var ent in db.MMdbRaceRatings)
                    if (MMraces.ContainsKey(ent.MMdbRace.Name))
                        MMraces[ent.MMdbRace.Name].Rating[ent.Lobby] = new MMPlRating(ent);
            }
            // **/
            foreach (string name in MMplayers.Keys)
            {
                if (MMplayers[name].AuthName != null && MMplayers[name].AuthName != "")
                {
                    Players.Add(MMplayers[name].Name);
                    Auth.Add(MMplayers[name].AuthName, MMplayers[name].Name);
                }
            }

            if (!File.Exists(Program.myJson_file))
                File.Create(Program.myJson_file).Dispose();

            foreach (var line in File.ReadAllLines(Program.myJson_file))
            {
                dsreplay rep = null;
                try
                {
                    rep = JsonSerializer.Deserialize<dsreplay>(line);
                } catch { }
                if (rep != null)
                {
                    //rep.Init();
                    repHash.Add(rep.HASH);
                    if (!replays.ContainsKey(rep.ID))
                        replays[rep.ID] = new List<dsreplay>();
                    replays[rep.ID].Add(rep);
                }
            }

        }

        public async Task Save()
        {
            // /**
            List<MMdbPlayer> temp = new List<MMdbPlayer>();
            using (var db = new MMdb(_mmdb))
            {
                foreach (var conf in MMplayers.Values.Where(x => x.DBId == 0))
                {
                    MMdbPlayer dbpl = new MMdbPlayer(conf);
                    db.MMdbPlayers.Add(dbpl);
                    temp.Add(dbpl);

                }

                foreach (var conf in MMplayers.Values.Where(x => x.DBupdate == true))
                {
                    MMdbPlayer pl = new MMdbPlayer(conf);
                    db.MMdbPlayers.Update(pl);
                }
                await db.SaveChangesAsync();

                foreach (var ent in temp)
                {
                    MMplayerNG pl = MMplayers.Values.Where(x => x.Name == ent.Name).FirstOrDefault();
                    pl.DBId = ent.MMdbPlayerId;
                }
            }
            // **/
        }
    }
}
