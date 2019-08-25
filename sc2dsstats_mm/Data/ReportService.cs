using DSmm.Models;
using DSmm.Trueskill;
using sc2dsstats.Models;
using s2decode;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Text.Json;
using dsmm_server.Models;
using System.Text.RegularExpressions;
using sc2dsstats_mm;

namespace dsmm_server.Data
{
    public class ReportService
    {
        private S2decode _s2dec;
        private StartUp _startUp;
        private ScanStateChange _scan;
        private MMdb _db;

        private object dec_lock { get; set; } = new object();

        public ReportService(S2decode s2dec, StartUp startup, ScanStateChange scan, MMdb db)
        {
            _s2dec = s2dec;
            _startUp = startup;
            _scan = scan;
            _db = db;
            _s2dec.LoadEngine(0);
        }

        public async Task<dsreplay> Decode(string rep, int mmid, bool saveit = true)
        {
            return await Task.Run(() =>
            {
                dsreplay replay = null;
                lock (dec_lock)
                {
                    replay = _s2dec.DecodePython(rep, mmid, saveit);
                    if (_startUp.replays.ContainsKey(mmid))
                        _startUp.replays[mmid].Add(replay);
                    else
                    {
                        _startUp.replays.TryAdd(mmid, new List<dsreplay>());
                        _startUp.replays[mmid].Add(replay);
                    } 
                }
                return replay;
            });
        }

        public async Task<int> CheckValid(dsreplay replay, MMgameNG game)
        {
            int valid = 0;
            return await Task.Run(() => { 
                HashSet<string> game_Names = game.GetPlayers().Select(s => s.Name).ToHashSet();
                foreach (dsplayer dspl in replay.PLAYERS)
                {
                    if (game_Names.Contains(dspl.NAME))
                    {
                        valid++;
                        int rep_team = dspl.TEAM + 1;
                        List<MMplayerNG> game_team = new List<MMplayerNG>();
                        if (game.Team1.Select(s => s.Name == dspl.NAME).Count() > 0)
                            game_team = game.Team1;
                        else
                            game_team = game.Team2;

                        foreach (dsplayer tdspl in replay.PLAYERS.Where(x => x.TEAM == dspl.TEAM))
                        {
                            if (tdspl.NAME == dspl.NAME) continue;
                            if (game_Names.Contains(tdspl.NAME))
                            {
                                if (game_team.Where(x => x.Name == tdspl.NAME).Count() > 0)
                                    valid++;
                            }
                        }
                    }
                }
                return valid;
            });
        }

        public async Task Save(MMgameNG game)
        {
            // /**
            foreach (var pl in game.GetPlayers())
            {
                if (pl.Name.StartsWith("Random") || pl.Name.StartsWith("Dummy")) continue;
                var dbpl = _db.MMdbPlayers.Where(x => x.Name == pl.Name).FirstOrDefault();
                if (dbpl != null)
                {
                    foreach (var rat in pl.Rating[game.Lobby].Where(x => x.Db == false).OrderBy(o => o.Time).ToArray())
                    {
                        MMdbRating dbrat = new MMdbRating();
                        dbrat.EXP = rat.EXP;
                        dbrat.Games = rat.Games;
                        dbrat.Lobby = game.Lobby;
                        dbrat.MU = rat.MU;
                        dbrat.SIGMA = rat.SIGMA;
                        dbrat.MMdbPlayerId = dbpl.MMdbPlayerId;
                        dbrat.MMdbPlayer = dbpl;
                        rat.Db = true;
                        _db.MMdbRatings.Add(dbrat);
                    }
                }
            }
            await _db.SaveChangesAsync();
            // **/
            string output = Program.workdir + "/games/" + game.ID + "_report.json";
            if (!File.Exists(output))
            {
                using (FileStream fs = File.Create(output))
                {
                    await JsonSerializer.SerializeAsync(fs, game);
                }
            }
        }

        public async Task SaveRace(MMgameNG game)
        {
            // /**
            foreach (var pl in game.GetPlayers())
            {
                var dbpl = _db.MMdbRaces.Where(x => x.Name == pl.Name).FirstOrDefault();
                if (dbpl != null)
                {
                    foreach (var rat in pl.Rating[game.Lobby].Where(x => x.Db == false).OrderBy(o => o.Time).ToArray())
                    {
                        MMdbRaceRating dbrat = new MMdbRaceRating();
                        dbrat.EXP = rat.EXP;
                        dbrat.Games = rat.Games;
                        dbrat.Lobby = game.Lobby;
                        dbrat.MU = rat.MU;
                        dbrat.SIGMA = rat.SIGMA;
                        dbrat.MMdbRaceId = dbpl.MMdbRaceId;
                        dbrat.MMdbRace = dbpl;
                        rat.Db = true;
                        _db.MMdbRaceRatings.Add(dbrat);
                    }
                }
            }
            await _db.SaveChangesAsync();
            // **/
        }

        public bool CheckName(string name)
        {
            if (name.Length <= 2)
            {
                return false;
            }

            if (name.Length > 64)
            {
                return false;
            }

            if (Regex.Escape(name) != name)
            {
                return false;
            }

            return true;
        }
    }
}
