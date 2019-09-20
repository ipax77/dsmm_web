using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Collections.Concurrent;
using pax.s2decode.Models;
using Microsoft.EntityFrameworkCore;
using DSmm.Models;
using sc2dsstats.Data;
using sc2dsstats_mm_dev;
using DSmm.Trueskill;
using System.Reflection;

namespace dsmm_server.Data
{
    public class StartUp
    {
        public static string VERSION = "v0.3";
        public HashSet<string> Players = new HashSet<string>();
        //public Dictionary<string, MMplayerNG> Conf = new Dictionary<string, MMplayerNG>();
        public ConcurrentDictionary<string, MMplayerNG> MMplayers { get; set; } = new ConcurrentDictionary<string, MMplayerNG>();
        public ConcurrentDictionary<string, MMplayerNG> MMraces { get; set; } = new ConcurrentDictionary<string, MMplayerNG>();
        public ConcurrentDictionary<int, List<dsreplay>> replays = new ConcurrentDictionary<int, List<dsreplay>>();
        public List<dsreplay> MyReplays { get; set; } = new List<dsreplay>();
        public HashSet<string> repHash = new HashSet<string>();
        public Dictionary<string, string> Auth = new Dictionary<string, string>();
        private DbContextOptions<MMdb> _mmdb;
        public MMdb _db { get; set; }

        public StartUp(DbContextOptions<MMdb> mmdb)
        {
            _mmdb = mmdb;
            _db = new MMdb(_mmdb);
            foreach (string cmdr in DSdata.s_races)
                MMraces.TryAdd(cmdr, new MMplayerNG(cmdr));

             /**
            using (var db = new MMdb(_mmdb))
            {
                foreach (var ent in db.MMdbPlayers)
                    db.MMdbPlayers.Remove(ent);
                foreach (var ent in db.MMdbRatings)
                    db.MMdbRatings.Remove(ent);
                foreach (var ent in db.MMdbRaceRatings)
                    db.MMdbRaceRatings.Remove(ent);
                
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

                foreach (var ent in db.MMdbRatings.OrderBy(o => o.Games)) 
                    if (MMplayers.ContainsKey(ent.MMdbPlayer.Name))
                        if (MMplayers[ent.MMdbPlayer.Name].AuthName == ent.MMdbPlayer.AuthName)
                            MMplayers[ent.MMdbPlayer.Name].Rating[ent.Lobby].Add(new MMPlRating(ent));

                foreach (var ent in db.MMdbRaceRatings.OrderBy(o => o.Games))
                    if (MMraces.ContainsKey(ent.MMdbRace.Name))
                        MMraces[ent.MMdbRace.Name].Rating[ent.Lobby].Add(new MMPlRating(ent));
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

            if (!File.Exists(Program.myReplays_file))
                File.Create(Program.myReplays_file).Dispose();

            foreach (var line in File.ReadAllLines(Program.myReplays_file))
            {
                dsreplay rep = null;
                try
                {
                    rep = JsonSerializer.Deserialize<dsreplay>(line);
                }
                catch { }
                if (rep != null)
                {
                    MyReplays.Add(rep);
                }
            }

            string exedir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            foreach (var file in Directory.EnumerateFiles(Program.replaydir))
            {
                string dest = exedir + "/replays/" + Path.GetFileName(file);
                if (!File.Exists(dest))
                    File.Copy(file, dest);
            }

            // ladder init

            //LadderInit();
        }

        async Task LadderInit()
        {
            await Save();
            List<string> LadderGames = new List<string>();
            if (File.Exists(Program.ladder_file))
            {
                LadderGames = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(Program.ladder_file));
            }
            foreach (var ent in LadderGames)
            {
                MMgameNG game;
                MMgameNG racegame;
                (game, racegame) = MMrating.RateGame(ent, "Commander3v3True", this);
                await Save();
                await Save(game);
                await SaveRace(racegame);
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

                
                List<MMdbRace> race_temp = new List<MMdbRace>();
                /**
                foreach (var ent in MMraces.Values)
                {
                    MMdbRace cmdr = new MMdbRace(ent);
                    db.MMdbRaces.Add(cmdr);
                    race_temp.Add(cmdr);
                }
                **/
                await db.SaveChangesAsync();

                foreach (var ent in temp)
                {
                    MMplayerNG pl = MMplayers.Values.Where(x => x.Name == ent.Name).FirstOrDefault();
                    pl.DBId = ent.MMdbPlayerId;
                }

                foreach (var ent in race_temp)
                {
                    MMplayerNG pl = MMraces.Values.Where(x => x.Name == ent.Name).FirstOrDefault();
                    pl.DBId = ent.MMdbRaceId;
                }



            }
            // **/
        }

        public async Task Reload()
        {
            replays.Clear();
            await Task.Run(() => { 
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
            });
        }

        public async Task Save(MMgameNG game)
        {
            // /**
            using (var _db = new MMdb(_mmdb))
            {
                foreach (var pl in game.GetPlayers())
                {
                    if (pl.Name.StartsWith("Random") || pl.Name.StartsWith("Dummy")) continue;
                    var dbpl = _db.MMdbPlayers.Where(x => x.Name == pl.Name).FirstOrDefault();
                    if (dbpl != null)
                    {
                        foreach (var rat in pl.Rating[game.Lobby].Where(x => x.Db == false).OrderBy(o => o.Games).ToArray())
                        {
                            MMdbRating dbrat;
                            dbrat = new MMdbRating();
                            dbrat.EXP = rat.EXP;
                            dbrat.Games = rat.Games;
                            dbrat.Lobby = game.Lobby;
                            dbrat.MU = rat.MU;
                            dbrat.SIGMA = rat.SIGMA;
                            dbrat.MMdbPlayerId = dbpl.MMdbPlayerId;
                            dbrat.MMdbPlayer = dbpl;
                            dbrat.Time = rat.Time;
                            rat.Db = true;
                            _db.MMdbRatings.Add(dbrat);
                            //dbpl.MMdbRatings.Add(dbrat);
                        }
                    }
                }
                await _db.SaveChangesAsync();
            }
            // **/

        }
        public async Task SaveRace(MMgameNG game)
        {
            // /**
            using (var _db = new MMdb(_mmdb))
            {
                foreach (var pl in game.GetPlayers())
                {
                    var dbpl = _db.MMdbRaces.Where(x => x.Name == pl.Name).FirstOrDefault();
                    if (dbpl != null)
                    {
                        foreach (var rat in pl.Rating[game.Lobby].Where(x => x.Db == false).OrderBy(o => o.Games).ToArray())
                        {
                            MMdbRaceRating dbrat;
                            dbrat = new MMdbRaceRating();
                            dbrat.EXP = rat.EXP;
                            dbrat.Games = rat.Games;
                            dbrat.Lobby = game.Lobby;
                            dbrat.MU = rat.MU;
                            dbrat.SIGMA = rat.SIGMA;
                            dbrat.MMdbRaceId = dbpl.MMdbRaceId;
                            dbrat.MMdbRace = dbpl;
                            dbrat.Time = rat.Time;
                            rat.Db = true;
                            _db.MMdbRaceRatings.Add(dbrat);
                            //dbpl.MMdbRaceRatings.Add(dbrat);
                        }
                    }
                }
                await _db.SaveChangesAsync();
            }
            // **/
        }
    }
}
