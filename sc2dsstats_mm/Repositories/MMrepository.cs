using DSmm.Models;
using DSmm.Trueskill;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using sc2dsstats.Models;

namespace DSmm.Repositories
{
    public class MMrepository : IMMrepository
    {
        private readonly ILogger _logger;

        private ConcurrentDictionary<string, MMplayer> MMplayers { get; set; } = new ConcurrentDictionary<string, MMplayer>();
        private ConcurrentDictionary<string, MMplayer> Ladderplayers { get; set; } = new ConcurrentDictionary<string, MMplayer>();

        //private ConcurrentDictionary<string, Qplayer> Qplayers { get; set; } = new ConcurrentDictionary<string, Qplayer>();
        private ObservableCollection<MMplayer> QMMplayers = new ObservableCollection<MMplayer>();

        private ConcurrentDictionary<string, ConcurrentDictionary<MMplayer, byte>> Lobby { get; set; } = new ConcurrentDictionary<string, ConcurrentDictionary<MMplayer, byte>>();
        
        private ConcurrentDictionary<int, MMgame> Games { get; set; } = new ConcurrentDictionary<int, MMgame>();
        private ConcurrentDictionary<int, ConcurrentBag<dsreplay>> Replays { get; set; } = new ConcurrentDictionary<int, ConcurrentBag<dsreplay>>();
        private ConcurrentBag<string> ReplayHash { get; set; } = new ConcurrentBag<string>();


        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();
        private static string WorkDir { get; } = "/data";
        public static string data_json = WorkDir + "/mmplayer.json";

        static Regex rx_pl = new Regex(@"^\(([^\(]+)(.*)");
        static Regex rx_rng = new Regex(@"^Random\d+");
        static Regex rx_mmid = new Regex(@"^(\d+)");

        static int Minticks = 20 / 2;
        static int MMID = 1000;
        bool LobbyCheck = false;

        public DSladder Ranking { get; private set; } = new DSladder();
        public DSladder PubRanking { get; private set; } = new DSladder();

        private static ReaderWriterLockSlim _readWriteLockLadder = new ReaderWriterLockSlim();

        public static List<string> laddergames = new List<string>() {
            "(PAX, Kerrigan, 14360, (Panzerfaust, Zagara, 9160), (Feralan, Abathur, 8425) vs (Gorgoroth, Zagara, 4470), (Ragggy, Vorazun, 9850), (macissammich, Zagara, 9295)",
            "(PAX, Abathur, 39975), (Panzerfaust, Kerrigan, 38950, (Gorgoroth, Zagara, 37790) vs (Feralan, Artanis, 34655), (macissammich, Dehaka, 30345), (Ragggy, Fenix, 36165)",
            "(Feralan, Dehaka, 22145, (Gorgoroth, Zagara, 32075), (Ragggy, Alarak, 29090) vs (PAX, Abathur, 17385), (Panzerfaust, Kerrigan, 30610), (macissammich, Zagara, 19065)",
            "(PAX, Nova, 36440), (Shin, Dehaka, 28670), (Feralan, Karax, 40560) vs (Lolz, Dehaka, 32505), (Panzerfaust, Tychus, 21985), (Gorgoroth, Karax, 24205)",
            "(Feralan, Abathur, 43280), (Lolz, Fenix, 52135), (Ragggy, Raynor, 20630) vs (PAX, Kerrigan, 31220), (Gorgoroth, Nova, 23140), (Panzerfaust, Zagara, 43715)",
            "(PAX, Karax, 38500), (Shin, Nova, 41840), (macissammich, Kerrigan, 50520) vs (Ragggy, Alarak, 50720), (Panzerfaust, Artanis, 46140), (Lolz, Vorazun, 33095)",
            "(Feralan, Stukov, 12350) (macissammich, Zagara, 6710) (Lolz, Fenix, 8420) vs (Ragggy, Swann, 13130) (Shin, Horner, 8810) (Gorgoroth, Artanis, 9850)",
            "(PAX, Fenix, 149775), (Ragggy, Swann, 109795), (Gorgoroth, Artanis, 150635) vs (macissammich, Zagara, 113650), (Shin, Dehaka, 116435), (Feralan, Stukov, 114885)",
            "(Shin, Vorazun, 31765), (Lolz, Nova, 39965), (Feralan, Kerrigan, 37740) vs (PAX, Raynor, 41875), (Ragggy, Kerrigan, 29585), (macissammich, Zagara, 23230)",
            "(macissammich, Kerrigan, 45640), (Shin, Vorazun, 68345), (PAX, Tychus, 46710) vs (Ragggy, Abathur, 28980), (Gorgoroth, Artanis, 51795), (Feralan, Kerrigan, 50950)",
            "(PAX, Tychus, 68275), (Ragggy, Alarak, 70740), (Gorgoroth, Vorazun, 77585) vs (macissammich, Karax, 67915), (Lolz, Stukov, 44120), (Shin, Horner, 57690)",
            "(Shin, Abathur, 27990), (Panzerfaust, Tychus, 52970), (Ragggy, Alarak, 38555) vs (macissammich, Kerrigan, 48280), (PAX, Tychus, 18750), (Lolz, Abathur, 33800)",
            "(PAX, Zagara, 27450), (Ragggy, Artanis, 33550), (Lolz, Alarak, 30370) vs (macissammich, Karax, 39890), (Shin, Raynor, 28865), (Panzerfaust, Horner, 23000)",
            "(Ragggy, Fenix, 47175) (Feralan, Nova, 65540) (Leviathan, Abathur, 37860) vs (macissammich, Kerrigan, 58740) (Lolz, Stukov, 48280) (Panzerfaust, Vorazun, 23825)",
            "(Leviathan, Dehaka, 38400) (Lolz, Raynor, 40860) (Gorgoroth, Karax, 71925) vs (Ragggy, Alarak, 28635) (macissammich, Karax, 65720) (Feralan, Nova, 48075)",
            "(Feralan, Fenix, 8970) (Ragggy, Raynor, 4065) (Leviathan, Abathur, 8800) vs (Lolz, Fenix, 10650) (Gorgoroth, Nova, 6120) (macissammich, Karax, 7790)",
            "(Feralan, Stukov, 96640) (Lolz, Zagara, 77035) (Leviathan, Dehaka, 51320) vs (Ragggy, Horner, 77535) (Gorgoroth, Artanis, 121975) (macissammich, Kerrigan, 21400)",
            "(Feralan, Kerrigan, 22415) (Slayitquick, Artanis, 23255) (Ragggy, Karax, 20175) vs (Panzerfaust, Kerrigan, 15215) (PAX, Tychus, 23035) (Lolz, Vorazun, 16725)",
            "(Ragggy, Stukov, 140020) (PAX, Kerrigan, 119020) (Slayitquick, Vorazun, 94795) vs (Lolz, Karax, 133395) (Excalibur, Kerrigan, 115130) (Feralan, Artanis, 93765)",
            "(Lolz, Fenix, 52195), (Slayitquick, Karax, 46010), (Feralan, Raynor, 39055) vs (PAX, Kerrigan, 33680), (Excalibur, Vorazun, 31060), (Ragggy, Stukov, 37150)",
            "(PAX, Tychus, 17680), (Slayitquick, Karax, 19700), (Excalibur, Kerrigan, 26585) vs (Lolz, Fenix, 17800), (Panzerfaust, Tychus, 25010), (Feralan, Swann, 19225)",
            "(PAX, Stukov, 33560), (Panzerfaust, Zagara, 36470), (Feralan, Dehaka, 42110) vs (Excalibur, Artanis, 30830), (Slayitquick, Karax, 64485), (Ragggy, Horner, 37610)",
            "(PAX, Stukov, 12070), (Lolz, Fenix, 13060), (Excalibur, Swann, 12975) vs (Ragggy, Raynor, 10820), (Slayitquick, Vorazun, 9740), (Feralan, Fenix, 13460)",
            "(PAX, Nova, 37790), (Panzerfaust, Dehaka, 32390), (Arkos, Zagara, 20000) vs (Ragggy, Fenix, 26550), (Bonejury, Abathur, 26790), (Lolz, Stukov, 31345)",
            "(Lolz, Stukov, 10155), (PAX, Kerrigan, 23635), (Bonejury, Karax, 17270) vs (Ragggy, Kerrigan, 16385), (Arkos, Karax, 15825), (Panzerfaust, Raynor, 9395)",
            "(Arkos, Swann, 26010), (Lolz, Dehaka, 24140), (NneInchGirth, Artanis, 25935) vs (Ragggy, Vorazun, 13245), (PAX, Kerrigan, 20965), (Panzerfaust, Tychus, 13180)",
            "(Leviathan, Zagara, 70785), (Lolz, Dehaka, 92775), (Random1, Swann, 67715) vs (Ragggy, Kerrigan, 48405), (PAX, Fenix, 85240), (Random2, Dehaka, 62200)",
            "(Ragggy, Raynor, 44585), (Lolz, Stukov, 27855), (Random3, Kerrigan, 19615) vs (PAX, Tychus, 25945), (Leviathan, Vorazun, 28265), (Random4, Karax, 30620) ",
            "(Ragggy, Swann, 96670), (Leviathan, Zagara, 52490), (Random5, Zagara, 74690) vs (Lolz, Stukov, 73285), (PAX, Abathur, 83605), (Random6, Tychus, 56050)",
            "(PAX, Zagara, 116250), (Random7, Swann, 83240), (Random8, Alarak, 78280) vs (Ragggy, Karax, 103720), (Leviathan, Zagara, 96635), (Random9, Karax, 63900)",
            "(Lolz, Stukov, 40350), (NneInchGirth, Fenix, 38615), (Ragggy, Swann, 60645) vs (PAX, Kerrigan, 34715), (EnjuAihara, Stukov, 38550), (Duracell, Zagara, 31630)",
            "(EnjuAihara, Karax, 34580), (Ragggy, Artanis, 34025), (PAX, Kerrigan, 63120) vs (Duracell, Fenix, 31065), (Lolz, Stukov, 35090), (Panzerfaust, Vorazun, 20630)",
            "(EnjuAihara, Fenix, 13190), (Duracell, Stukov, 10745), (PAX, Swann, 19200) vs (Lolz, Dehaka, 13425), (Ragggy, Horner, 14300), (NneInchGirth, Swann, 7290)",
            "(PAX, Vorazun, 67675), (Duracell, Dehaka, 66870), (Panzerfaust, Tychus, 69680) vs (Lolz, Nova, 75190), (EnjuAihara, Abathur, 63840), (NneInchGirth, Artanis, 47955)",
            "(PAX, Horner, 20670), (DrManiac, Fenix, 18100), (Feralan, Swann, 10695) vs (Ragggy, Fenix, 20625), (laxnar, Raynor, 7845), (Shin, Abathur, 3155)",
            "(Feralan, Dehaka, 14450), (Shin, Horner, 4100), (laxnar, Swann, 7570) vs (PAX, Zagara, 3720), (Duracell, Stukov, 5190), (Ragggy, Kerrigan, 6450)",
            "(PAX, Vorazun, 61635), (Duracell, Dehaka, 77265), (laxnar, Raynor, 46980) vs (Feralan, Kerrigan, 59455), (Shin, Vorazun, 48965), (Ragggy, Artanis, 58735)",
            "(PAX, Kerrigan, 168665), (laxnar, Abathur, 81560), (Feralan, Stukov, 83900) vs (Duracell, Zagara, 87525), (Shin, Horner, 110380), (EnjuAihara, Dehaka, 107365)",
            "(Shin, Nova, 141895), (DrManiac, Tychus, 142785), (laxnar, Swann, 87660) vs (Duracell, Fenix, 129115), (PAX, Horner, 110055), (EnjuAihara, Swann, 108985)",
            "(DrManiac, Tychus, 42545), (Ragggy, Fenix, 71300), (VeNaRiS, Karax, 46505) vs (PAX, Kerrigan, 56010), (laxnar, Swann, 52460), (Panzerfaust, Stukov, 43590)",
            "(PAX, Alarak, 34190), (Panzerfaust, Zagara, 28915), (laxnar, Dehaka, 20950) vs (Ragggy, Horner, 22765), (DrManiac, Tychus, 32110), (VeNaRiS, Zagara, 36925)",
            "(PAX, Alarak, 37700), (VeNaRiS, Stukov, 30685), (laxnar, Swann, 27225) vs (Ragggy, Kerrigan, 32455), (EnjuAihara, Abathur, 39160), (Panzerfaust, Tychus, 38450)"

        };


        public MMrepository(ILogger<MMrepository> logger)
        {
            _logger = logger;
            
            if (File.Exists(data_json))
            {
                TextReader reader = new StreamReader(data_json, Encoding.UTF8);
                string fileContents;
                while ((fileContents = reader.ReadLine()) != null)
                {
                    var player = JsonConvert.DeserializeObject<BasePlayer>(fileContents);
                    if (player != null && player.Name != null)
                    {
                        MMplayer pl = new MMplayer();
                        pl = new MMplayer(player);
                        MMplayers.TryAdd(player.Name, pl);
                    }
                }
                reader.Close();
            }
            _logger.LogInformation("Initialized MMplayers with {0} ents.", MMplayers.Count());

            foreach (var file in Directory.EnumerateFiles(WorkDir + "/games", "*_found.json"))
            {
                var ent = Path.GetFileName(file);
                Match m = rx_mmid.Match(ent);
                if (m.Success)
                {
                    int mmid = int.Parse(m.Groups[1].ToString());
                    if (mmid > MMID) MMID = mmid;
                }

                try
                {
                    var json = JsonConvert.DeserializeObject<MMgame>(File.ReadAllText(file));
                    Games.TryAdd(json.ID, json);
                } catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

            }
            _logger.LogInformation("Setting MMID to " + MMID);

            foreach (var file in Directory.EnumerateFiles(WorkDir + "/games", "*_report.json"))
            {
                try
                {
                    var json = JsonConvert.DeserializeObject<MMgame>(file);
                    Games[json.ID] = json;
                }
                catch { }
            }

            foreach (var file in Directory.EnumerateFiles(WorkDir + "/games", "*_replay_*.json"))
            {
                try
                {
                    var json = JsonConvert.DeserializeObject<dsreplay>(file);
                    ReplayHash.Add(json.HASH);
                }
                catch { }
            }

            /**
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var ladder = System.Text.Json.JsonSerializer.Serialize(laddergames, options);
            File.WriteAllText(WorkDir + "/ladder.json", ladder);
            **/
            GetLadder();
            QMMplayers.CollectionChanged += QplayersChanged;

        }

        public async Task GetLadder()
        {
            using (FileStream SourceStream = File.Open(WorkDir + "/ladder.json", FileMode.Open))
            {
                laddergames = new List<string>(await System.Text.Json.JsonSerializer.DeserializeAsync<List<string>>(SourceStream));
            }
            
            Ranking = new DSladder();

            await Task.Run(() => { 
                foreach (var game in laddergames)
                {
                    RateStringResult(game, Ranking);
                }
                //Save();
                //Ladder();
            });

            foreach (var pl in Ranking.MMplayers.Values.ToArray())
            {
                if (pl.Name.StartsWith("Random") || pl.Name.StartsWith("Dummy"))
                    Ranking.MMplayers.Remove(pl.Name);
            }

            PubRanking = new DSladder();
            PubRanking.MMplayers = new Dictionary<string, MMplayer>(MMplayers);
            foreach (var pl in PubRanking.MMplayers.Values.ToArray())
            {
                if (pl.Name.StartsWith("Random") || pl.Name.StartsWith("Dummy"))
                    PubRanking.MMplayers.Remove(pl.Name);
            }

        }

        void QplayersChanged(object aSender, NotifyCollectionChangedEventArgs aArgs)
        {
            if (aArgs.Action == NotifyCollectionChangedAction.Add)
            {
                MMplayer qpl = new MMplayer();
                qpl = aArgs.NewItems[0] as MMplayer;

                int lobbysize = int.Parse(qpl.Mode2.Substring(qpl.Mode2.Length - 1, 1)) * 2;
                qpl.Lobbysize = lobbysize;
                MMplayers[qpl.Name].Lobbysize = lobbysize;
                if (!Lobby.ContainsKey(qpl.Mode + qpl.Mode2)) Lobby.TryAdd(qpl.Mode + qpl.Mode2, new ConcurrentDictionary<MMplayer, byte>());
                Lobby[qpl.Mode + qpl.Mode2].TryAdd(MMplayers[qpl.Name], 0);

                //qpl.Lobby = Lobby[qpl.Mode + qpl.Mode2];
                MMplayers[qpl.Name].Lobby = Lobby[qpl.Mode + qpl.Mode2];
                if (LobbyCheck == false)
                {
                    LobbyCheck = true;
                    CheckLobbyJob();
                }
            }
        }

        void CheckLobbyJob()
        {
            _logger.LogInformation("LobbyJob running.");
            Task.Factory.StartNew(() => { 
                while (LobbyCheck)
                {
                    Thread.Sleep(500);
                    foreach (var lobby in Lobby.Values)
                    {
                        if (lobby != null && lobby.Count > 0)
                        {
                            //_logger.LogInformation("Check lobby " + lobby.Count);
                            CheckLobby(lobby);
                        }
                            
                    }
                }
                _logger.LogInformation("LobbyJob halted.");
            });
        }

        MMgame CheckLobby(ConcurrentDictionary<MMplayer, byte> lobby)
        {
            int lobbysize = 0;
            if (lobby.Count > 0) lobbysize = lobby.Keys.First().Lobbysize;

            var rng = lobby.Keys.Where(x => x.Random == true);
            int minticks = lobby.OrderBy(o => o.Key.Ticks).First().Key.Ticks;

            if (rng.Count() >= 2 && lobby.Count < lobbysize && minticks > Minticks)
            {
                int i = 0;
                while (lobby.Count < lobbysize)
                {
                    i++;
                    MMplayer mm = new MMplayer();
                    mm.Name = "Random" + i;
                    mm.Ticks = Minticks * 3 + 1;
                    mm.Accepted = true;
                    mm.Lobbysize = lobbysize;
                    lobby.TryAdd(mm, 0);
                }
            }

            MMgame game = new MMgame();
            lock (lobby)
            {
                if (lobbysize > 0 && lobby.Count >= lobbysize)
                {
                    if (minticks > Minticks || lobby.Count > lobbysize)
                    {
                        _logger.LogInformation("Generating Matchup .. " + minticks);
                        game = GenMatchup(lobby, lobbysize).Result;
                        if (game != null)
                        {
                            Interlocked.Increment(ref MMID);
                            game.ID = MMID;
                            _logger.LogInformation("Game found: {0} ({1}) ", MMID, lobby.Count);

                            Save(game);
                            byte zero = 0;
                            var dic = GameList(game).ToDictionary(x => new MMplayer(x), x => zero);

                            Dictionary<string, int> Server = new Dictionary<string, int>();
                            foreach (var pl in GameList(game))
                            {
                                Match m = rx_rng.Match(pl.Name);
                                if (m.Success) continue;
                                lock (MMplayers[pl.Name])
                                {
                                    MMplayers[pl.Name].Game = game;
                                    MMplayers[pl.Name].Lobby = new ConcurrentDictionary<MMplayer, byte>(dic);
                                }
                                lobby.TryRemove(MMplayers[pl.Name], out zero);

                                if (lobby.Count == 0)
                                {
                                    int p = 0;
                                    foreach (var ent in Lobby.Values)
                                    {
                                        if (ent != null && ent.Count > 0)
                                            p++;
                                    }
                                    if (p == 0) LobbyCheck = false;
                                }

                                if (!Server.ContainsKey(MMplayers[pl.Name].Server)) Server.Add(MMplayers[pl.Name].Server, 1);
                                else Server[MMplayers[pl.Name].Server]++;
                            }
                            game.Server = Server.OrderByDescending(o => o.Value).First().Key;
                            Games.TryAdd(MMID, game);
                        }
                    }
                }
                // fail safe
                foreach (var ent in lobby.Keys)
                {
                    Match m = rx_rng.Match(ent.Name);
                    byte zero = 0;
                    if (m.Success) lobby.TryRemove(ent, out zero);
                }
            }
            return game;
        }

        async Task<MMgame> GenMatchup(ConcurrentDictionary<MMplayer, byte> lobby, int lobbysize)
        {
            return await MMrating.GenMatch(lobby.Keys.ToList(), lobbysize);
        }

        public bool Save ()
        {
            Task.Factory.StartNew(() => { 
                _readWriteLock.EnterWriteLock();
                if (File.Exists(data_json + "_bak"))
                    File.Delete(data_json + "_bak");
                if (File.Exists(data_json))
                    File.Move(data_json, data_json + "_bak");
                File.Create(data_json).Dispose();
                HashSet<string> _MMplayers = new HashSet<string>();
                foreach (var pl in MMplayers)
                {
                    Match m = rx_rng.Match(pl.Value.Name);
                    if (m.Success) continue;
                    try
                    {
                        var json = JsonConvert.SerializeObject(new BasePlayer(pl.Value));
                        _MMplayers.Add(json);
                    } 
                    catch (Exception e)
                    {
                        _logger.LogError("Failed Serializing player: " + e.Message);
                    }
                }
                File.WriteAllLines(data_json, _MMplayers, Encoding.UTF8);
                _readWriteLock.ExitWriteLock();
            });
            return true;
        }

        public void Save(MMgame game)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var json = JsonConvert.SerializeObject(game, Formatting.Indented);
                    File.WriteAllText(WorkDir + "/games/" + game.ID + "_found.json", json);
                }
                catch
                {
                    _logger.LogError("Failed writing found game " + game.ID);
                }
            });
        }

        public void Save(MMgame game, dsreplay replay)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var json = JsonConvert.SerializeObject(game, Formatting.Indented);
                    if (!File.Exists(WorkDir + "/games/" + game.ID + "_report.json"))
                        File.WriteAllText(WorkDir + "/games/" + game.ID + "_report.json", json);
                }
                catch
                {
                    _logger.LogError("Failed writing report game " + game.ID);
                }

                replay.GenHash();
                ReplayHash.Add(replay.HASH);
                try
                {
                    var json = JsonConvert.SerializeObject(replay, Formatting.Indented);
                    string replay_file = WorkDir + "/games/" + game.ID + "_replay.json";
                    int i = 1;
                    while (File.Exists(replay_file))
                    {
                        replay_file = WorkDir + "/games/" + game.ID + "_replay_" + i.ToString() + ".json";
                        i++;
                    }
                    File.WriteAllText(replay_file, json);
                }
                catch
                {
                    _logger.LogError("Failed writing report replay " + game.ID);
                }

            });
        }

        public void RateStringResult(string result, DSladder rank)
        {
            RESgame gameres = new RESgame();
            gameres.Winner = 0;
            var teams = result.Split(new string[] { " vs " }, StringSplitOptions.None);
            if (teams.Length == 2)
            {
                int t = 0;
                int p = 0;
                foreach (var team in teams)
                {
                    Match m = rx_pl.Match(team);
                    while (m.Success)
                    {
                        RESplayer plres = new RESplayer();
                        plres.Team = t;
                        plres.Pos = p;
                        var plent = m.Groups[1].ToString().Split(',', StringSplitOptions.None);
                        int i = 0;
                        foreach (var ent in plent)
                        {
                            string myent = ent.Replace(" ", string.Empty);
                            myent = ent.Replace("(", string.Empty);
                            myent = ent.Replace(")", string.Empty);
                            if (i == 0) plres.Name = myent;
                            if (i == 1) plres.Race = myent;
                            if (i == 2) plres.Kills = int.Parse(myent);
                            i++;
                        }
                        gameres.Players.Add(plres);
                        m = rx_pl.Match(m.Groups[2].ToString());
                        p++;
                    }
                    t++;
                }
            }

            List<MMplayer> t1 = new List<MMplayer>();
            List<MMplayer> t2 = new List<MMplayer>();
            List<MMplayer> t1_race = new List<MMplayer>();
            List<MMplayer> t2_race = new List<MMplayer>();
            foreach (var pl in gameres.Players)
            {
                if (!rank.MMplayers.ContainsKey(pl.Name)) rank.MMplayers.TryAdd(pl.Name, new MMplayer(pl.Name));
                if (!rank.MMraces.ContainsKey(pl.Race)) rank.MMraces.TryAdd(pl.Race, new MMplayer(pl.Race));

                if (pl.Team == 0)
                {
                    t1.Add(rank.MMplayers[pl.Name]);
                    t1_race.Add(rank.MMraces[pl.Race]);
                }
                if (pl.Team == 1)
                {
                    t2.Add(rank.MMplayers[pl.Name]);

                    if (t1_race.Contains(rank.MMraces[pl.Race]))
                        t1_race.Remove(rank.MMraces[pl.Race]);
                    else
                        t2_race.Add(rank.MMraces[pl.Race]);

                }
            }
            MMrating.RateGame(t1, t2);
            MMrating.RateGame(t1_race, t2_race);
        }

        public void Ladder()
        {
            string bab = "";
            foreach (var ent in MMplayers.OrderByDescending(o => o.Value.EXP))
            {
                Match m = rx_rng.Match(ent.Value.Name);
                if (m.Success) continue;
                bab += String.Format("{0} => {1} => {2} => {3} => {4}", ent.Value.Name, ent.Value.EXP, ent.Value.MU, ent.Value.SIGMA, ent.Value.Games) + Environment.NewLine;
            }
            _readWriteLock.EnterWriteLock();
            File.WriteAllText(data_json + "_ladder.txt" , bab);
            _readWriteLock.ExitWriteLock();
        }

        public async Task<BasePlayer> LetmePlay(SEplayer sepl)
        {

            if (sepl != null)
            {
                if (!MMplayers.ContainsKey(sepl.Name))
                {
                    MMplayer mm = new MMplayer(sepl);
                    MMplayers.TryAdd(sepl.Name, mm);
                }
                else
                {
                    ExitQ(sepl.Name);
                    MMplayers[sepl.Name].Mode = sepl.Mode;
                    MMplayers[sepl.Name].Mode2 = sepl.Mode2;
                    MMplayers[sepl.Name].Server = sepl.Server;
                    MMplayers[sepl.Name].Random = sepl.Random;
                }
                lock (QMMplayers)
                {
                    QMMplayers.Add(MMplayers[sepl.Name]);
                }
                Save();
                return new BasePlayer(MMplayers[sepl.Name]);
            }
            else
                return null;
        }

        public async Task<RetFindGame> FindGame(string name)
        {
            RetFindGame ret = new RetFindGame();
            foreach (var ent in MMplayers[name].Lobby.Keys.Take(MMplayers[name].Lobbysize))
                ret.Players.Add(ent as BasePlayer);

            if (MMplayers[name].Game != null && MMplayers[name].Game.ID != 0)
            {
                ret.Game = MMplayers[name].Game;
                ret.Players = null;
            }
            else
            {
                MMplayers[name].Ticks++;
                ret.Game = null;
            }
            return ret;
        }

        public async Task<string> ExitQ(string name)
        {
            if (MMplayers.ContainsKey(name))
            {
                if (QMMplayers.Contains(MMplayers[name]))
                {
                    if (MMplayers[name].Lobby.ContainsKey(MMplayers[name]))
                    {
                        foreach (var pl in MMplayers[name].Lobby.Keys)
                        {
                            lock (MMplayers[pl.Name])
                            {
                                byte zero = 0;
                                MMplayers[pl.Name].Lobby.TryRemove(MMplayers[name], out zero);
                            }
                        }
                    }

                    lock (QMMplayers)
                    {
                        QMMplayers.Remove(MMplayers[name]);
                    }
                }
                ResetMMplayer(name);
            }

            // fail safe
            foreach (var lobby in Lobby.Values)
                lock (lobby)
                {
                    foreach (var pl in lobby)
                    {
                        if (pl.Key.Name == name)
                        {
                            byte zero = 0;
                            lobby.TryRemove(MMplayers[name], out zero);
                        }
                    }
                }

            return "Ok";
        }

        public async Task<MMgame> Status(string id)
        {
            int mmid = int.Parse(id);

            /**
            if (Games.ContainsKey(mmid))
            {
                var json = JsonConvert.SerializeObject(Games[mmid], Formatting.Indented);
                _logger.LogInformation(json);
            }
            **/
            if (!Games.ContainsKey(mmid)) return null;
            else return Games[mmid];
        }

        public async Task<string> Accept(string name, string id)
        {
            lock (MMplayers[name])
            {
                MMplayers[name].Accepted = true;
            }

            int mmid = int.Parse(id);
            if (MMplayers[name].Game == null || MMplayers[name].Game.ID != mmid || MMplayers[name].Game.Declined == true || Games[mmid] == null || Games[mmid].Declined == true) {
                EnqueueDeclined(name);
                return "Game not found :(";
            }


            bool gameready = true;
            foreach (var pl in GameList(MMplayers[name].Game))
            {
                Match m = rx_rng.Match(pl.Name);
                if (m.Success) continue;

                if (pl.Name == name)
                    pl.Accepted = true;

                if (MMplayers[pl.Name].Accepted == false)
                    gameready = false;
                    
            }
            if (gameready == true)
            {
                lock (MMplayers[name].Game)
                {
                    MMplayers[name].Game.Accepted = true;
                }
            }

            return "TY!";
        }

        public async Task<string> Decline(string name, string id)
        {
            int mmid = int.Parse(id);
            if (MMplayers[name].Game != null)
            {
                if (MMplayers[name].Game.ID == mmid)
                {
                    lock (MMplayers[name].Game)
                    {
                        MMplayers[name].Game.Declined = true;

                        foreach (var pl in GameList(MMplayers[name].Game))
                        {
                            if (pl.Accepted == true)
                            {
                                pl.Accepted = false;
                                EnqueueDeclined(pl.Name);
                            }
                        }
                    }
                }
            }
            else if (Games.ContainsKey(mmid))
            {
                lock (Games[mmid])
                {
                    Games[mmid].Declined = true;
                    foreach (var pl in GameList(Games[mmid]))
                    {
                        if (pl.Accepted == true)
                        {
                            pl.Accepted = false;
                            EnqueueDeclined(pl.Name);
                        }
                    }
                }
            }
            await ExitQ(name);            
            return "Sad :(";
        }

        public async Task<string> Deleteme(string name)
        {
            ExitQ(name);
            if (MMplayers.ContainsKey(name))
            {
                MMplayer mm = new MMplayer();
                MMplayers.TryRemove(name, out mm);
                Save();
            }
            return "Ok.";
        }

        public async Task<string> Random(string name)
        {
            if (MMplayers.ContainsKey(name))
            {
                MMplayers[name].Random = !MMplayers[name].Random;
                if (MMplayers[name].Random == true) MMplayers[name].Ticks = 0;
            }
            return "Ok.";
        }

        public async Task<DSladder> Ladder(string name)
        {
            return Ranking;
        }



        public async Task<MMgame> Report(dsreplay replay, string id)
        {
            MMgame mmgame = new MMgame();
            int gid = int.Parse(id);
            if (Replays.ContainsKey(gid))
                Replays[gid].Add(replay);
            else
            {
                Replays.TryAdd(gid, new ConcurrentBag<dsreplay>());
                Replays[gid].Add(replay);
            }
            Save(Games[gid], replay);
            lock (Games)
            {
                if (Games.ContainsKey(gid))
                    if (Games[gid].Reported == true) return Games[gid];

                replay.GenHash();
                if (ReplayHash.Contains(replay.HASH)) {
                    //return null;
                }
                else ReplayHash.Add(replay.HASH);

                List<MMplayer> team1 = new List<MMplayer>();
                List<MMplayer> team2 = new List<MMplayer>();
                foreach (var pl in replay.PLAYERS)
                {
                    MMplayer mmpl = new MMplayer();
                    if (MMplayers.ContainsKey(pl.NAME))
                        mmpl = MMplayers[pl.NAME];
                    else
                        mmpl.Name = "Dummy";

                    if (pl.TEAM == replay.WINNER)
                        team1.Add(mmpl);
                    else
                        team2.Add(mmpl);
                }
                MMgame repgame = new MMgame();
                repgame = MMrating.RateGame(team1, team2);
                repgame.ID = Games[gid].ID;
                repgame.Quality = Games[gid].Quality;
                repgame.Server = Games[gid].Server;
                repgame.Reported = true;
                Games[gid] = repgame;
                
            }
            Save();
            return Games[gid];
            
        }

        public async Task<string> Manual(string file, string id)
        {
            try
            {
                using (StreamReader r = new StreamReader(file))
                {
                    string ln;
                    while ((ln = r.ReadLine()) != null) {
                        //RateStringResult(ln);
                    }
                }

            } catch { }



            return "Ok";
        }

        void EnqueueDeclined(string name)
        {
            List<BasePlayer> ilist = new List<BasePlayer>();
            ilist.Add(new BasePlayer(name));
            EnqueueDeclined(ilist, "Random1");
        }

        void EnqueueDeclined(List<BasePlayer> ilist, String name)
        {
            Task.Factory.StartNew(() => { 
                Thread.Sleep(2000);
                lock (QMMplayers)
                {
                    foreach (var pl in ilist)
                    {
                        if (pl.Name == name) continue;

                        Match m = rx_rng.Match(pl.Name);
                        if (m.Success) continue;

                        if (MMplayers[pl.Name].Accepted == true)
                        {
                            lock (MMplayers[pl.Name])
                            {
                                MMplayers[pl.Name].Game = null;
                                MMplayers[pl.Name].Accepted = false;
                            }
                            QMMplayers.Add(MMplayers[pl.Name]);
                        }
                    }
                }
            });
        }

        List<BasePlayer> GameList(MMgame game) {
            List<BasePlayer> ilist = new List<BasePlayer>();
            ilist.AddRange(game.Team1);
            ilist.AddRange(game.Team2);
            return ilist;
        }

        void ResetMMplayer(string name)
        {
            lock (MMplayers[name])
            {
                MMplayers[name].Game = null;
                MMplayers[name].Accepted = false;
                MMplayers[name].Lobbysize = 0;
                MMplayers[name].Ticks = 0;
                MMplayers[name].Random = false;
                MMplayers[name].Lobby = new ConcurrentDictionary<MMplayer, byte>();
            }
        }
    }
}
