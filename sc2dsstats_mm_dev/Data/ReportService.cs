using DSmm.Models;
using pax.s2decode;
using pax.s2decode.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Text.Json;
using dsmm_server.Models;
using System.Text.RegularExpressions;
using sc2dsstats_mm_dev;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Mvc;
using System;
using sc2dsstats_mm_dev.Models;
using System.Threading;

namespace dsmm_server.Data
{
    public class ReportService
    {
        private StartUp _startUp;
        private s2decode _s2dec;
        private readonly IFileProvider _fileProvider;
        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();

        private object dec_lock { get; set; } = new object();

        public ReportService(StartUp startup, IFileProvider fileProvider)
        {
            _startUp = startup;
            _fileProvider = fileProvider;
            _s2dec = new s2decode();
            _s2dec.DEBUG = 0;
            _s2dec.JsonFile = Program.myJson_file;
            Dictionary<string, string> repFolder = new Dictionary<string, string>();
            repFolder.Add(Program.replaydir, Program.replaydir);
            repFolder.Add(Program.myreplaydir, Program.myreplaydir);
            string mypath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            _s2dec.LoadEngine(mypath);
        }

        public FileStreamResult GetFileAsStream(string file)
        {
            if (File.Exists(_startUp.Exedir + "/" + file))
            {
                var stream = _fileProvider
                    .GetFileInfo(file)
                    .CreateReadStream();

                return new FileStreamResult(stream, "application/octet-stream");
            } else
                return null;
        }

        public async Task<dsreplay> Decode(string rep, int mmid, bool saveit = true)
        {
            return await Task.Run(() =>
            {
                dsreplay replay = null;
                lock (dec_lock)
                {
                    //_s2dec.REPID = mmid - 1;
                    replay = _s2dec.DecodePython(rep, false, true);
                    if (replay != null)
                    {
                        replay.ID = mmid;
                        replay.REPLAY = rep;
                        var json = JsonSerializer.Serialize(replay);
                        File.AppendAllText(Program.myJson_file, json + Environment.NewLine);

                        if (_startUp.replays.ContainsKey(mmid))
                            _startUp.replays[mmid].Add(replay);
                        else
                        {
                            _startUp.replays.TryAdd(mmid, new List<dsreplay>());
                            _startUp.replays[mmid].Add(replay);
                        }
                        string dest = _startUp.Exedir + "/replays/" + Path.GetFileName(rep);
                        if (!File.Exists(dest))
                            File.Copy(rep, dest);

                        SaveDetails(replay);
                    }
                }
                return replay;
            });
        }

        public async Task<dsreplay> myDecode(string rep, int mmid, bool saveit = true)
        {
            return await Task.Run(() =>
            {
                dsreplay replay = null;
                lock (dec_lock)
                {
                    //_s2dec.REPID = mmid - 1;
                    replay = _s2dec.DecodePython(rep, false, true);
                    if (replay != null)
                    {
                        replay.ID = mmid;
                        replay.REPLAY = rep;
                        var json = JsonSerializer.Serialize(replay);
                        File.AppendAllText(Program.myReplays_file, json + Environment.NewLine);
                        _startUp.MyReplays.Add(replay);
                        string dest = _startUp.Exedir + "/replays/" + Path.GetFileName(rep);
                        if (!File.Exists(dest))
                            File.Copy(rep, dest);

                        SaveDetails(replay);
                    }
                }
                return replay;
            });
        }

        public async Task SaveDetails(dsreplay replay, string detaildir = Program.detaildir)
        {
            string mydir = detaildir + "/" + replay.ID.ToString();
            if (!Directory.Exists(mydir))
            {
                Directory.CreateDirectory(mydir);
                WriteToBinaryFile(mydir + "/mid.bin", replay.MIDDLE);

                foreach (dsplayer pl in replay.PLAYERS)
                {
                    PlStats st = new PlStats();
                    st.Loops = new List<int>(pl.STATS.Keys.ToList());
                    st.Stats = new List<M_stats>(pl.STATS.Values.ToList());
                    try
                    {
                        WriteToBinaryFile(mydir + "/" + pl.REALPOS.ToString() + "_stats.bin", st);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
        }

        public async Task GetDetails(dsreplay replay, string detaildir = Program.detaildir)
        {
            await Task.Run(() => { 
                string mydir = detaildir + "/" + replay.ID.ToString();
                if (Directory.Exists(mydir))
                {
                    if (File.Exists(mydir + "/mid.bin"))
                        replay.MIDDLE = ReadFromBinaryFile<List<KeyValuePair<int, int>>>(mydir + "/mid.bin");

                    foreach (dsplayer pl in replay.PLAYERS)
                    {
                        PlStats st = new PlStats();
                        if (File.Exists(mydir + "/" + pl.REALPOS.ToString() + "_stats.bin"))
                            st = ReadFromBinaryFile<PlStats>(mydir + "/" + pl.REALPOS.ToString() + "_stats.bin");

                        int i = 0;
                        foreach (var ent in st.Loops)
                        {
                            pl.STATS[ent] = st.Stats.ElementAt(i);
                            i++;
                        }
                    }
                }
            });
        }

        public async Task SaveComment(int repid, GameComment _com)
        {
            string mycomfile = Program.commentdir + "/" + repid + ".bin";
            _readWriteLock.EnterWriteLock();
            List<GameComment> Gamecoms = new List<GameComment>(await GetComments(repid));
            Gamecoms.Add(_com);
            WriteToBinaryFile(mycomfile, Gamecoms);
            _readWriteLock.ExitWriteLock();
        }

        public async Task<List<GameComment>> GetComments(int repid)
        {
            List<GameComment> Gamecoms = new List<GameComment>();
            _readWriteLock.EnterReadLock();
            string mycomfile = Program.commentdir + "/" + repid + ".bin";
            if (File.Exists(mycomfile))
            {
                try
                {
                    Gamecoms = ReadFromBinaryFile<List<GameComment>>(mycomfile);
                } catch { }
            }
            _readWriteLock.ExitReadLock();
            return Gamecoms;
        }

        public async Task DeleteComment(int repid, GameComment _com)
        {
            string mycomfile = Program.commentdir + "/" + repid + ".bin";
            _readWriteLock.EnterWriteLock();
            List<GameComment> Gamecoms = new List<GameComment>(await GetComments(repid));
            Gamecoms.Remove(_com);
            WriteToBinaryFile(mycomfile, Gamecoms);
            _readWriteLock.ExitWriteLock();
        }

        public async Task ReScan()
        {
            File.Delete(Program.myJson_file);
            File.Create(Program.myJson_file).Dispose();


            foreach (string file in Directory.EnumerateFiles(Program.replaydir))
            {
                Regex rx = new Regex(@"^(\d+)");
                string name = Path.GetFileName(file);
                Match m = rx.Match(name);
                if (m.Success)
                {
                    int id = int.Parse(m.Groups[1].Value.ToString());
                    //_s2dec.REPID = id - 1;
                    //dsreplay rep = _s2dec.DecodePython(file, true, false);
                    await Decode(file, id, true);
                }
            }
            
            File.Delete(Program.myReplays_file);
            File.Create(Program.myReplays_file).Dispose();

            foreach (string file in Directory.EnumerateFiles(Program.myreplaydir))
            {
                Regex rx = new Regex(@"^(\d+)");
                string name = Path.GetFileName(file);
                Match m = rx.Match(name);
                if (m.Success)
                {
                    string stid = m.Groups[1].Value.ToString();
                    string nstid = new string(stid.Skip(4).ToArray());

                    int id = int.Parse(nstid);

                    await myDecode(file, id, true);
                }
            }
        }

        public async Task ReScan(string tdir)
        {
            string jsonfile = tdir + "/treplays.json";
            if (File.Exists(jsonfile))
                File.Delete(jsonfile);
            File.Create(jsonfile).Dispose();

            if (!_startUp.TournamentReplays.ContainsKey(Path.GetFileName(tdir)))
                _startUp.TournamentReplays.Add(Path.GetFileName(tdir), new List<dsreplay>());
            else
                _startUp.TournamentReplays[Path.GetFileName(tdir)].Clear();

            if (Directory.Exists(tdir + "/replays"))
            {
                int i = 0;
                foreach (string file in Directory.EnumerateFiles(tdir + "/replays"))
                {
                    i++;
                    dsreplay replay = _s2dec.DecodePython(file, false, true);
                    if (replay != null)
                    {
                        replay.ID = i;
                        replay.REPLAY = file;
                        var json = JsonSerializer.Serialize(replay);
                        File.AppendAllText(jsonfile, json + Environment.NewLine);
                        _startUp.TournamentReplays[Path.GetFileName(tdir)].Add(replay);
                        string dest = _startUp.Exedir + "/treplays/" + Path.GetFileName(tdir) + "/" + Path.GetFileName(file);
                        if (!Directory.Exists(Path.GetPathRoot(dest)))
                            Directory.CreateDirectory(Path.GetPathRoot(dest));
                        if (!File.Exists(dest))
                            File.Copy(file, dest);

                        SaveDetails(replay, tdir);
                    }
                }

            }
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

        public async Task RemoveUserReplay(int myrepid)
        {
            dsreplay rep = _startUp.MyReplays.Where(x => x.ID == myrepid).FirstOrDefault();
            if (rep != null)
            {
                _startUp.MyReplays.Remove(rep);
                try
                {
                    File.Delete(rep.REPLAY);
                } catch { }

                if (Directory.Exists(Program.detaildir + "/" + rep.ID))
                {
                    try
                    {
                        Directory.Delete(Program.detaildir + "/" + rep.ID, true);
                    } catch { }
                }

                string removeline = JsonSerializer.Serialize(rep).Trim();
                var lines = File.ReadAllLines(Program.myReplays_file).Where(line => line.Trim() != removeline).ToArray();
                File.WriteAllLines(Program.myReplays_file, lines);
                await _startUp.Reload();
            }
        }

        public async Task Save(MMgameNG game)
        {
            // /**
            foreach (var pl in game.GetPlayers())
            {
                if (pl.Name.StartsWith("Random") || pl.Name.StartsWith("Dummy")) continue;
                var dbpl = _startUp._db.MMdbPlayers.Where(x => x.Name == pl.Name).FirstOrDefault();
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
                        _startUp._db.MMdbRatings.Add(dbrat);
                    }
                }
            }
            await _startUp._db.SaveChangesAsync();
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
                var dbpl = _startUp._db.MMdbRaces.Where(x => x.Name == pl.Name).FirstOrDefault();
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
                        _startUp._db.MMdbRaceRatings.Add(dbrat);
                    }
                }
            }
            await _startUp._db.SaveChangesAsync();
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

        public static void WriteToBinaryFile<T>(string filePath, T objectToWrite, bool append = false)
        {
            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, objectToWrite);
            }
        }

        public static T ReadFromBinaryFile<T>(string filePath)
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (T)binaryFormatter.Deserialize(stream);
            }
        }
    }

    [Serializable]
    public class PlStats
    {
        public List<int> Loops = new List<int>();
        public List<M_stats> Stats = new List<M_stats>();
    }
}
