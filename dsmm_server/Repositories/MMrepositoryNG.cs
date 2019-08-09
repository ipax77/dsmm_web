using DSmm.Models;
using DSmm.Trueskill;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DSmm.Repositories;
using dsweb_electron6.Models;
using dsmm_server.Data;

namespace dsmm_server.Repositories
{
    public class MMrepositoryNG : IMMrepositoryNG
    {
        //public ConcurrentDictionary<string, MMplayerNG> _startUp.MMplayers { get; set; } = new ConcurrentDictionary<string, MMplayerNG>();
        private ConcurrentDictionary<string, ObservableCollection<MMplayerNG>> Lobbies = new ConcurrentDictionary<string, ObservableCollection<MMplayerNG>>();

        private static string[] s_lobbies = new string[]
        {
            "Commander3v3True",
            "Commander2v2True",
            "Commander1v1True",
            "Standard3v3True",
            "Standard2v2True",
            "Standard1v1True",
            "Commander3v3False",
            "Commander2v2False",
            "Commander1v1False",
            "Standard3v3False",
            "Standard2v2False",
            "Standard1v1Fasle",
        };

        public static Dictionary<string, int> s_size = new Dictionary<string, int>()
        {
            { "Commander3v3True", 6 },
            { "Commander2v2True", 4 },
            {"Commander1v1True", 2 },
            {"Standard3v3True", 6 },
            {"Standard2v2True", 4 },
            {"Standard1v1True", 2 },
            {"Commander3v3False", 6 },
            {"Commander2v2False", 4 },
            {"Commander1v1False", 2 },
            {"Standard3v3False", 6 },
            {"Standard2v2False", 4 },
            {"Standard1v1Fasle", 2 }
        };

        public ObservableCollection<MMgameNG> Games { get; set; } = new ObservableCollection<MMgameNG>();
        public ConcurrentDictionary<int, MMgameNG> Reports { get; set; } = new ConcurrentDictionary<int, MMgameNG>();
        private ConcurrentBag<string> ReplayHash { get; set; } = new ConcurrentBag<string>();

        private readonly ILogger _logger;
        private static string WorkDir { get; } = Program.workdir;
        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();
        SemaphoreSlim semaphore = new SemaphoreSlim(1);

        static Regex rx_pl = new Regex(@"^\(([^\(]+)(.*)");
        static Regex rx_rng = new Regex(@"^Random\d+");
        static Regex rx_mmid = new Regex(@"^(\d+)");

        static int MMID = 1000;
        static int Minticks = 20 / 2;
        bool LobbyCheck = false;

        private StartUp _startUp;

        public MMrepositoryNG(ILogger<MMrepositoryNG> logger, StartUp startUp)
        {
            _logger = logger;
            _startUp = startUp;

            foreach (string l in s_lobbies)
            {
                Lobbies.TryAdd(l, new ObservableCollection<MMplayerNG>());
                Lobbies[l].CollectionChanged += LobbyChanged;
            }

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
                    var json = JsonSerializer.Deserialize<MMgameNG>(File.ReadAllText(file));
                    Games.Add(json);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

            }
            _logger.LogInformation("Setting MMID to " + MMID);

            foreach (var file in Directory.EnumerateFiles(WorkDir + "/games", "*_report.json"))
            {
                try
                {
                    var json = JsonSerializer.Deserialize<MMgameNG>(File.ReadAllText(file));
                    Reports.TryAdd(json.ID, json);
                }
                catch (Exception e) {
                    Console.WriteLine(e.Message);
                }
            }

            Games.CollectionChanged += GamesChanged;
        }

        async void LobbyChanged(object aSender, NotifyCollectionChangedEventArgs aArgs)
        {
            ObservableCollection<MMplayerNG> lobby = aSender as ObservableCollection<MMplayerNG>;

            foreach (MMplayerNG pl in lobby)
            {
                lock (pl)
                {
                    pl.Lobby = lobby.Take(6).ToHashSet();
                }
                await NotifyPlayer(pl, "lobby");
            }

            if (lobby != null && lobby.Count > 0 && aArgs.Action == NotifyCollectionChangedAction.Add)
            {
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
            bool checking = false;
            Task.Run(() => {
                while (LobbyCheck)
                {
                    Thread.Sleep(500);
                    int i = 0;
                    foreach (var lobby in Lobbies.Keys)
                    {
                        if (Lobbies[lobby] != null && Lobbies[lobby].Count > 0)
                        {
                            //_logger.LogInformation("Check lobby " + lobby.Count);
                            i++;
                            lock (Lobbies[lobby])
                            {
                                if (checking == false)
                                {
                                    checking = true;
                                    _ = CheckLobby(Lobbies[lobby], s_size[lobby]);
                                    checking = false;
                                }
                            }
                        }
                    }
                    if (i == 0)
                        LobbyCheck = false;
                }
                _logger.LogInformation("LobbyJob halted.");
            });
        }

        async Task<MMgameNG> CheckLobby(ObservableCollection<MMplayerNG> lobby, int lobbysize)
        {
            MMgameNG game = new MMgameNG();

            var rng = lobby.Where(x => x.Random == true);
            int minticks = lobby.OrderBy(o => o.Ticks).First().Ticks;

            if (rng.Count() >= 2) { 
                if (lobby.Count < lobbysize && minticks > Minticks)
                {
                    int i = 0;
                    while (lobby.Count < lobbysize)
                    {
                        i++;
                        MMplayerNG mm = new MMplayerNG();
                        mm.Name = "Random" + i;
                        mm.Ticks = Minticks * 3 + 1;
                        mm.Accepted = true;
                        lobby.Add(mm);
                    }
                }
            }
            if (lobby.Count >= lobbysize)
            {
                if (minticks > Minticks || lobby.Count >= lobbysize)
                {
                    _logger.LogInformation("Generating Matchup .. " + minticks);
                    game = await MMrating.GenMatch(lobby.ToList(), lobbysize);
                    if (game != null)
                    {
                        Interlocked.Increment(ref MMID);
                        game.ID = MMID;
                        var temp_pl = game.GetPlayers().FirstOrDefault();
                        game.Lobby = temp_pl.Mode + temp_pl.Mode2 + temp_pl.Ladder;
                        _logger.LogInformation("Game found: {0} ({1}) ", MMID, lobby.Count);

                        Dictionary<string, int> Server = new Dictionary<string, int>();
                        foreach (var pl in game.GetPlayers())
                        {
                            Match m = rx_rng.Match(pl.Name);
                            if (m.Success) continue;
                            lock (_startUp.MMplayers[pl.Name])
                            {
                                _startUp.MMplayers[pl.Name].Game = game;
                            }

                            lock (lobby)
                            {
                                lobby.Remove(_startUp.MMplayers[pl.Name]);

                                if (lobby.Count == 0)
                                {
                                    int p = 0;
                                    foreach (var ent in Lobbies.Values)
                                    {
                                        if (ent != null && ent.Count > 0)
                                            p++;
                                    }
                                    if (p == 0) LobbyCheck = false;
                                }
                            }

                            if (!Server.ContainsKey(_startUp.MMplayers[pl.Name].Server)) Server.Add(_startUp.MMplayers[pl.Name].Server, 1);
                            else Server[_startUp.MMplayers[pl.Name].Server]++;
                        }
                        game.Server = Server.OrderByDescending(o => o.Value).First().Key;
                        game.State.GameID = game.ID;
                        game.State.PropertyChanged += GameChanged;
                        lock (Games)
                        {
                            Games[game.ID] = game;
                        }
                    }
                }
            }

            return game;

        }

        async void GamesChanged(object aSender, NotifyCollectionChangedEventArgs aArgs)
        {
            MMgameNG game;
            if (aArgs.Action == NotifyCollectionChangedAction.Add)
            {
                game = aArgs.NewItems[0] as MMgameNG;
                foreach (MMplayerNG pl in game.GetPlayers())
                {
                    await NotifyPlayer(pl, "gameadd");
                }
            } else if (aArgs.Action == NotifyCollectionChangedAction.Remove)
            {
                game = aArgs.NewItems[0] as MMgameNG;
                foreach (MMplayerNG pl in game.GetPlayers())
                {
                    await NotifyPlayer(pl, "gameremove");
                }
            }
        }

        private async void GameChanged(object sender, PropertyChangedEventArgs e)
        {
            GameStateChange gs = sender as GameStateChange;
            foreach (MMplayerNG pl in Games.Where(x => x.ID == gs.GameID).FirstOrDefault().GetPlayers()) 
                await NotifyPlayer(pl, "gamechanged");
        }

        

        async Task NotifyPlayer(MMplayerNG pl, string changed)
        {
            lock(pl)
                pl.Notify = true;
        }


        public async Task<MMplayerNG> LetmePlay(MMplayerNG sepl)
        {
            if (!_startUp.MMplayers.ContainsKey(sepl.Name))
            {
                _startUp.MMplayers.TryAdd(sepl.Name, sepl);
                await _startUp.Save();
            }
            else
            {
                MMplayerNG _sepl = sepl.ShallowCopy();
                ExitQ(sepl.Name);
                _startUp.MMplayers[sepl.Name].Mode = _sepl.Mode;
                _startUp.MMplayers[sepl.Name].Mode2 = _sepl.Mode2;
                _startUp.MMplayers[sepl.Name].Server = _sepl.Server;
                _startUp.MMplayers[sepl.Name].Random = _sepl.Random;
                _startUp.MMplayers[sepl.Name].Ladder = _sepl.Ladder;
                _startUp.MMplayers[sepl.Name].Ticks = _sepl.Ticks;
            }
            string lobby = _startUp.MMplayers[sepl.Name].Mode + _startUp.MMplayers[sepl.Name].Mode2 + _startUp.MMplayers[sepl.Name].Ladder;
            lock (Lobbies[lobby])
            {
                Lobbies[lobby].Add(_startUp.MMplayers[sepl.Name]);
            }
            return _startUp.MMplayers[sepl.Name];
        }

        public async Task Accept(string name, int id)
        {
            lock (_startUp.MMplayers[name])
                _startUp.MMplayers[name].Accepted = true;

            // lock needed?
            if (_startUp.MMplayers[name].Game.GetPlayers().Where(x => x.Accepted == true).Count() == _startUp.MMplayers[name].Game.GetPlayers().Count())
            {
                _startUp.MMplayers[name].Game.Accepted = true;
                Games.Add(_startUp.MMplayers[name].Game);
                await Save(_startUp.MMplayers[name].Game);
            }

            foreach (MMplayerNG pl in _startUp.MMplayers[name].Game.GetPlayers())
                await NotifyPlayer(_startUp.MMplayers[pl.Name], "gameaccept");
        }

        public async Task Decline(string name, int id)
        {
            lock (_startUp.MMplayers[name])
                _startUp.MMplayers[name].Declined = true;

            _startUp.MMplayers[name].Game.Declined = true;
            _startUp.MMplayers[name].Game.RemovePlayer(name);
            ExitQ(name);

            foreach (MMplayerNG pl in _startUp.MMplayers[name].Game.GetPlayers())
                await NotifyPlayer(_startUp.MMplayers[pl.Name], "gamedecline");
        }

        public void ExitQ(string name)
        {
            if (_startUp.MMplayers.ContainsKey(name))
            {
                foreach (var lobby in Lobbies.Values)
                    if (lobby.Contains(_startUp.MMplayers[name]))
                        lock (lobby)
                            lobby.Remove(_startUp.MMplayers[name]);

                ResetMMplayer(name);
            }
        }

        void ResetMMplayer(string name)
        {
            lock (_startUp.MMplayers[name])
            {
                _startUp.MMplayers[name].Game = new MMgameNG();
                _startUp.MMplayers[name].Accepted = false;
                _startUp.MMplayers[name].Declined = false;
                _startUp.MMplayers[name].Ticks = 0;
                _startUp.MMplayers[name].Random = false;
                _startUp.MMplayers[name].Ladder = false;
                _startUp.MMplayers[name].Lobby = new HashSet<MMplayerNG>();
            }
        }

        async Task Save(MMgameNG game)
        {
            string output = WorkDir + "/games/" + game.ID + "_found.json";

            using (FileStream fs = File.Create(output))
            {
                await JsonSerializer.SerializeAsync(fs, game);
            }
        }
    }
}
