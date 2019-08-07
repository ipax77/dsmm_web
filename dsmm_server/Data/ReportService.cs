using DSmm.Models;
using DSmm.Trueskill;
using dsweb_electron6.Models;
using s2decode;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Text.Json;
using dsmm_server.Models;

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

        public async Task<dsreplay> Decode(string rep, int mmid)
        {
            return await Task.Run(() =>
            {
                dsreplay replay = null;
                lock (dec_lock)
                {
                    replay = _s2dec.DecodePython(rep, mmid);
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

        public async Task Save(MMgameNG game)
        {
            // /**
            foreach (var pl in game.GetPlayers())
            {
                if (pl.Name.StartsWith("Random") || pl.Name.StartsWith("Dummy")) continue;
                var dbpl = _db.MMdbPlayers.Where(x => x.Name == pl.Name).FirstOrDefault();
                if (dbpl != null)
                {
                    var dbrat = _db.MMdbRatings.Where(x => x.MMdbPlayerId == dbpl.MMdbPlayerId && x.Lobby == game.Lobby).FirstOrDefault();
                    if (dbrat == null)
                        dbrat = new MMdbRating();
                    var rat = pl.Rating[game.Lobby];
                    dbrat.EXP = rat.EXP;
                    dbrat.Games = rat.Games;
                    dbrat.Lobby = game.Lobby;
                    dbrat.MU = rat.MU;
                    dbrat.SIGMA = rat.SIGMA;
                    dbrat.MMdbPlayerId = dbpl.MMdbPlayerId;
                    dbrat.MMdbPlayer = dbpl;

                    if (dbrat.MMdbRatingId == null)
                        dbpl.MMdbRatings.Add(dbrat);
                        //_db.MMdbRatings.Add(dbrat);
                    else
                        _db.MMdbRatings.Update(dbrat);
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
                    var dbrat = _db.MMdbRaceRatings.Where(x => x.MMdbRaceId == dbpl.MMdbRaceId && x.Lobby == game.Lobby).FirstOrDefault();
                    if (dbrat == null)
                        dbrat = new MMdbRaceRating();
                    var rat = pl.Rating[game.Lobby];
                    dbrat.EXP = rat.EXP;
                    dbrat.Games = rat.Games;
                    dbrat.Lobby = game.Lobby;
                    dbrat.MU = rat.MU;
                    dbrat.SIGMA = rat.SIGMA;
                    dbrat.MMdbRaceId = dbpl.MMdbRaceId;
                    dbrat.MMdbRace = dbpl;

                    if (dbrat.MMdbRaceRatingId == null)
                        dbpl.MMdbRaceRatings.Add(dbrat);
                    else
                        _db.MMdbRaceRatings.Update(dbrat);
                }
            }
            await _db.SaveChangesAsync();
            // **/
        }
    }
}
