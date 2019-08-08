using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSmm.Models;
using DSmm.Repositories;
using dsmm_server.Data;
using dsmm_server.Repositories;
using dsweb_electron6.Models;
using Moserware.Skills;

namespace DSmm.Trueskill
{
    public static class MMrating
    {
        static Regex rx_pl = new Regex(@"^\(([^\(]+)(.*)");

        public static MMgame RateGame(List<MMplayer> t1, List<MMplayer> t2)
        {
            int i = 0;
            var team1 = new Team();
            var team2 = new Team();

            foreach (var pl in t1)
            {
                team1.AddPlayer(new Player(i), new Rating(pl.MU, pl.SIGMA));
                pl.Games++;
                i++;
            }
            foreach (var pl in t2)
            {
                team2.AddPlayer(new Player(i), new Rating(pl.MU, pl.SIGMA));
                pl.Games++;
                i++;
            }

            var gameInfo = GameInfo.DefaultGameInfo;
            var teams = Teams.Concat(team1, team2);
            var newRatingsWinLoseExpected = TrueSkillCalculator.CalculateNewRatings(gameInfo, teams, 1, 2);

            MMgame game = new MMgame();

            i = 0;
            foreach (var pl in team1.AsDictionary().Keys)
            {
                var res = newRatingsWinLoseExpected[pl];
                t1[i].EXP = res.ConservativeRating;
                t1[i].MU = res.Mean;
                t1[i].SIGMA = res.StandardDeviation;
                game.Team1.Add(new BasePlayer(t1[i]));
                i++;
            }
            i = 0;
            foreach (var pl in team2.AsDictionary().Keys)
            {
                var res = newRatingsWinLoseExpected[pl];
                t2[i].EXP = res.ConservativeRating;
                t2[i].MU = res.Mean;
                t2[i].SIGMA = res.StandardDeviation;
                game.Team2.Add(new BasePlayer(t2[i]));
                i++;
            }

            return game;
        }

        public static MMgameNG RateGame(Team team1, Team team2, string lobby, StartUp _mm, bool cmdr = false)
        {
            var gameInfo = GameInfo.DefaultGameInfo;
            var teams = Teams.Concat(team1, team2);
            var newRatingsWinLoseExpected = TrueSkillCalculator.CalculateNewRatings(gameInfo, teams, 1, 2);

            MMgameNG game = new MMgameNG();
            game.Lobby = lobby;
            int i = 0;
            foreach (var pl in team1.AsDictionary().Keys)
            {
                var res = newRatingsWinLoseExpected[pl];
                string name = pl.Id.ToString();
                MMplayerNG mpl = new MMplayerNG();
                if (_mm.MMplayers.ContainsKey(name))
                    mpl = _mm.MMplayers[name];
                else
                    mpl.Name = "Dummy" + i;

                if (cmdr == true)
                    mpl = _mm.MMraces[name];

                double temp = mpl.Rating[lobby].EXP;
                mpl.Rating[lobby].EXP = res.ConservativeRating;
                mpl.ExpChange = mpl.Rating[lobby].EXP - temp;
                mpl.Rating[lobby].MU = res.Mean;
                mpl.Rating[lobby].SIGMA = res.StandardDeviation;
                game.Team1.Add(mpl);
                i++;
            }
            foreach (var pl in team2.AsDictionary().Keys)
            {
                var res = newRatingsWinLoseExpected[pl];
                string name = pl.Id.ToString();
                MMplayerNG mpl = new MMplayerNG();
                if (_mm.MMplayers.ContainsKey(name))
                    mpl = _mm.MMplayers[name];
                else
                    mpl.Name = "Dummy" + i;

                if (cmdr == true)
                    mpl = _mm.MMraces[name];

                double temp = mpl.Rating[lobby].EXP;
                mpl.Rating[lobby].EXP = res.ConservativeRating;
                mpl.ExpChange = mpl.Rating[lobby].EXP - temp;
                mpl.Rating[lobby].MU = res.Mean;
                mpl.Rating[lobby].SIGMA = res.StandardDeviation;
                game.Team2.Add(mpl);
                i++;
            }

            return game;
        }

        public static (MMgameNG, MMgameNG) RateGame(string result, string lobby, StartUp _mm)
        {
            List<RESplayer> pllist = new List<RESplayer>();
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
                            string myent = ent.Trim();
                            myent = myent.Replace(" ", string.Empty);
                            myent = myent.Replace("(", string.Empty);
                            myent = myent.Replace(")", string.Empty);
                            if (i == 0) plres.Name = myent;
                            if (i == 1) plres.Race = myent;
                            if (i == 2) plres.Kills = int.Parse(myent);
                            i++;
                        }
                        pllist.Add(plres);
                        m = rx_pl.Match(m.Groups[2].ToString());
                        p++;
                    }
                    t++;
                }
            }
            var team1 = new Team();
            var team2 = new Team();

            var rteam1 = new Team();
            var rteam2 = new Team();

            int j = 0;
            foreach (var pl in pllist)
            {
                MMplayerNG mpl = new MMplayerNG();
                if (_mm.MMplayers.ContainsKey(pl.Name))
                    mpl = _mm.MMplayers[pl.Name];
                else
                {
                    if (pl.Name.StartsWith("Random") || pl.Name.StartsWith("Dummy"))
                        mpl.Name = "Dummy" + j;
                    else
                    {
                        mpl.Name = pl.Name;
                        _mm.MMplayers.TryAdd(mpl.Name, mpl);
                    }
                }
                MMplayerNG rpl = _mm.MMraces[pl.Race];

                mpl.Rating[lobby].Games++;
                rpl.Rating[lobby].Games++;

                if (pl.Team == 0)
                {
                    team1.AddPlayer(new Player(mpl.Name), new Rating(mpl.Rating[lobby].MU, mpl.Rating[lobby].SIGMA));
                    rteam1.AddPlayer(new Player(pl.Race), new Rating(rpl.Rating[lobby].MU, rpl.Rating[lobby].SIGMA));
                }
                else
                {
                    team2.AddPlayer(new Player(mpl.Name), new Rating(mpl.Rating[lobby].MU, mpl.Rating[lobby].SIGMA));
                    rteam2.AddPlayer(new Player(pl.Race), new Rating(rpl.Rating[lobby].MU, rpl.Rating[lobby].SIGMA));
                }
                j++;
            }
            return (RateGame(team1, team2, lobby, _mm), RateGame(rteam1, rteam2, lobby, _mm, true));
        }

        public static (MMgameNG, MMgameNG) RateGame(dsreplay replay, StartUp _mm, string lobby)
        {
            var team1 = new Team();
            var team2 = new Team();

            var rteam1 = new Team();
            var rteam2 = new Team();

            //string lobby = _mm.Games.Where(x => x.ID == replay.ID).FirstOrDefault().Lobby;
            if (lobby == null || lobby == "") return (null, null);
            int i = 0;
            foreach (var pl in replay.PLAYERS)
            {
                MMplayerNG mpl = new MMplayerNG();
                if (_mm.MMplayers.ContainsKey(pl.NAME))
                    mpl = _mm.MMplayers[pl.NAME];
                else
                    mpl.Name = "Dummy" + i;

                MMplayerNG rpl = _mm.MMraces[pl.RACE];

                if (pl.TEAM == replay.WINNER)
                {
                    team1.AddPlayer(new Player(mpl.Name), new Rating(mpl.Rating[lobby].MU, mpl.Rating[lobby].SIGMA));
                    rteam1.AddPlayer(new Player(pl.RACE), new Rating(rpl.Rating[lobby].MU, rpl.Rating[lobby].SIGMA));
                }
                else
                {
                    team2.AddPlayer(new Player(mpl.Name), new Rating(mpl.Rating[lobby].MU, mpl.Rating[lobby].SIGMA));
                    rteam2.AddPlayer(new Player(pl.RACE), new Rating(rpl.Rating[lobby].MU, rpl.Rating[lobby].SIGMA));
                }
                rpl.Rating[lobby].Games++;
                mpl.Rating[lobby].Games++;
                i++;
            }
            return (RateGame(team1, team2, lobby, _mm), RateGame(rteam1, rteam2, lobby, _mm, true));
        }

        public static async Task<MMgameNG> GenMatch(List<MMplayerNG> qplayers, int size)
        {
            List<MMplayerNG> result = new List<MMplayerNG>();
            int c = 0;

            List<MMplayerNG> players = new List<MMplayerNG>();
            players = qplayers.Where(x => x.Game.ID == 0).ToList();

            if (players.Count < size) return null;

            string lobby = players.First().Mode + players.First().Mode2 + players.First().Ladder;

            double bestquality = 0;
            return await Task.Run(() => {
                while (true)
                {
                    c++;
                    int i = 0;
                    var team1 = new Team();
                    var team2 = new Team();

                    var rnd = new Random();
                    List<MMplayerNG> thisresult = new List<MMplayerNG>(players.Select(x => new { value = x, order = rnd.Next() })
                                .OrderBy(x => x.order).Select(x => x.value).Take(size).ToList());

                    
                    foreach (var pl in thisresult)
                    {
                        if (i < result.Count() / 2)
                            team1.AddPlayer(new Player(i), new Rating(pl.Rating[lobby].MU, pl.Rating[lobby].SIGMA));
                        else
                            team2.AddPlayer(new Player(i), new Rating(pl.Rating[lobby].MU, pl.Rating[lobby].SIGMA));
                        i++;
                    }

                    var gameInfo = GameInfo.DefaultGameInfo;
                    var teams = Teams.Concat(team1, team2);

                    double thisquality = TrueSkillCalculator.CalculateMatchQuality(gameInfo, teams);

                    if (thisquality > bestquality)
                    {
                        bestquality = thisquality;
                        result = new List<MMplayerNG>(thisresult);
                    }

                    if (c > 10 && bestquality > 0.5) break;
                    if (c > 50 && bestquality > 0.4) break;
                    if (c > 200) break;
                }

                MMgameNG game = new MMgameNG();
                game.Quality = bestquality;
                game.ID = 1;
                int j = 0;
                foreach (var pl in result)
                {
                    if (j < result.Count() / 2)
                        game.Team1.Add(pl);
                    else
                        game.Team2.Add(pl);
                    j++;
                }

                return game;
            });
        }

        public static async Task<MMgame> GenMatch(List<MMplayer> qplayers, int size)
        {
            List<MMplayer> result = new List<MMplayer>();
            int c = 0;

            List<MMplayer> players = new List<MMplayer>();
            players = qplayers.Where(x => x.Game == null).ToList();

            if (players.Count < size) return null;

            double bestquality = 0;
            return await Task.Run(() => { 
                while (true)
                {
                    c++;
                    int i = 0;
                    var team1 = new Team();
                    var team2 = new Team();

                    var rnd = new Random();
                    List<MMplayer> thisresult = new List<MMplayer>(players.Select(x => new { value = x, order = rnd.Next() })
                                .OrderBy(x => x.order).Select(x => x.value).Take(size).ToList());


                    foreach (var pl in thisresult)
                    {
                        if (i < result.Count() / 2)
                            team1.AddPlayer(new Player(i), new Rating(pl.MU, pl.SIGMA));
                        else
                            team2.AddPlayer(new Player(i), new Rating(pl.MU, pl.SIGMA));
                        i++;
                    }

                    var gameInfo = GameInfo.DefaultGameInfo;
                    var teams = Teams.Concat(team1, team2);

                    double thisquality = TrueSkillCalculator.CalculateMatchQuality(gameInfo, teams);
                    
                    if (thisquality > bestquality)
                    {
                        bestquality = thisquality;
                        result = new List<MMplayer>(thisresult);
                    }

                    if (c > 10 && bestquality > 0.5) break;
                    if (c > 50 && bestquality > 0.4) break;
                    if (c > 200) break;
                }

                MMgame game = new MMgame();
                game.Quality = bestquality;
                game.ID = 1;
                int j = 0;
                foreach (var pl in result)
                {
                    if (j < result.Count() / 2)
                        game.Team1.Add(new BasePlayer(pl));
                    else
                        game.Team2.Add(new BasePlayer(pl));
                    j++;
                }

                return game;
            });
        }

        private static void ThreeOnTwoTests()
        {
            // To make things interesting, here is a team of three people playing
            // a team of two people.
            // Initialize the players on the first team. Remember that the argument
            // passed to the Player constructor can be anything. It's strictly there
            // to help you uniquely identify people.
            var player1 = new Player(1);
            var player2 = new Player(2);
            var player3 = new Player(3);
            // Note the fluent-like API where you can add players to the Team and 
            // specify the rating of each using their mean and standard deviation
            // (for more information on these parameters, see the accompanying post
            // http://www.moserware.com/2010/03/computing-your-skill.html )
            var team1 = new Team()
                        .AddPlayer(player1, new Rating(28, 7))
                        .AddPlayer(player2, new Rating(27, 6))
                        .AddPlayer(player3, new Rating(26, 5));
            // Create players for the second team
            var player4 = new Player(4);
            var player5 = new Player(5);
            var team2 = new Team()
                        .AddPlayer(player4, new Rating(30, 4))
                        .AddPlayer(player5, new Rating(31, 3));
            // The default parameters are fine
            var gameInfo = GameInfo.DefaultGameInfo;
            // We only have two teams, combine the teams into one parameter
            var teams = Teams.Concat(team1, team2);
            // Specify that the outcome was a 1st and 2nd place
            var newRatingsWinLoseExpected = TrueSkillCalculator.CalculateNewRatings(gameInfo, teams, 1, 2);
            // Winners
            AssertRating(28.658, 6.770, newRatingsWinLoseExpected[player1]);
            AssertRating(27.484, 5.856, newRatingsWinLoseExpected[player2]);
            AssertRating(26.336, 4.917, newRatingsWinLoseExpected[player3]);
            // Losers
            AssertRating(29.785, 3.958, newRatingsWinLoseExpected[player4]);
            AssertRating(30.879, 2.983, newRatingsWinLoseExpected[player5]);
            // For fun, let's see what would have happened if there was an "upset" and the better players lost
            var newRatingsWinLoseUpset = TrueSkillCalculator.CalculateNewRatings(gameInfo, Teams.Concat(team1, team2), 2, 1);
            // Winners
            AssertRating(32.012, 3.877, newRatingsWinLoseUpset[player4]);
            AssertRating(32.132, 2.949, newRatingsWinLoseUpset[player5]);
            // Losers
            AssertRating(21.840, 6.314, newRatingsWinLoseUpset[player1]);
            AssertRating(22.474, 5.575, newRatingsWinLoseUpset[player2]);
            AssertRating(22.857, 4.757, newRatingsWinLoseUpset[player3]);
            // Note that we could have predicted this wasn't a very balanced game ahead of time because
            // it had low match quality.
            AssertMatchQuality(0.254, TrueSkillCalculator.CalculateMatchQuality(gameInfo, teams));
        }

        private static void AssertRating(double expectedMean, double expectedStandardDeviation, Rating actual)
        {
            const double ErrorTolerance = 0.085;
            Debug.Assert(Math.Abs(expectedMean - actual.Mean) < ErrorTolerance);
            Debug.Assert(Math.Abs(expectedStandardDeviation - actual.StandardDeviation) < ErrorTolerance);
        }

        private static void AssertMatchQuality(double expectedMatchQuality, double actualMatchQuality)
        {
            Debug.Assert(Math.Abs(expectedMatchQuality - actualMatchQuality) < 0.0005);
        }

    }
}
