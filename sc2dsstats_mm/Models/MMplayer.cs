using dsmm_server.Data;
using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace DSmm.Models
{
    
    public class BasePlayer
    {
        public string Name { get; set; }
        public double EXP { get; set; } = 0;
        public double MU { get; set; } = 25;
        public double SIGMA { get; set; } = 25 / 3;
        public int Games { get; set; } = 0;
        public bool Accepted { get; set; } = false;

        public BasePlayer()
        {

        }

        public BasePlayer(string name) : this()
        {
            Name = name;
        }

        public BasePlayer(MMplayer mpl) : this()
        {
            Name = mpl.Name;
            EXP = mpl.EXP;
            MU = mpl.MU;
            SIGMA = mpl.SIGMA;
            Games = mpl.Games;
            Accepted = mpl.Accepted;
        }
    }

    public class MMplayer : BasePlayer
    {
        public MMgame Game { get; set; }
        public ConcurrentDictionary<MMplayer, byte> Lobby { get; set; } = new ConcurrentDictionary<MMplayer, byte>();
        public int Lobbysize { get; set; } = 0;
        public int Ticks { get; set; } = 0;
        public string Mode { get; set; }
        public string Server { get; set; }
        public string Mode2 { get; set; }
        public bool Random { get; set; } = false;
        

        public MMplayer()
        {

        }

        public MMplayer(string name) : this()
        {
            Name = name;
        }

        public MMplayer ShallowCopy()
        {
            return (MMplayer)this.MemberwiseClone();
        }

        public MMplayer(SEplayer sepl) : this()
        {
            Name = sepl.Name;
            Mode = sepl.Mode;
            Server = sepl.Server;
            Mode2 = sepl.Mode2;
        }

        public MMplayer(BasePlayer bpl) : this()
        {
            Name = bpl.Name;
            EXP = bpl.EXP;
            MU = bpl.MU;
            SIGMA = bpl.SIGMA;
            Games = bpl.Games;
        }
    }

    public class SEplayer : BasePlayer
    {
        public string Mode { get; set; } = "Commander";
        public string Server { get; set; } = "NA";
        public string Mode2 { get; set; } = "3v3";
        public bool Random { get; set; } = false;
    }

    public class RESplayer
    {
        public string Name { get; set; }
        public string Race { get; set; }
        public int Kills { get; set; }
        public int Team { get; set; }
        public int Result { get; set; }
        public int Pos { get; set; }
    }

    public class MMgame
    {
        public int ID { get; set; } = 0;
        public DateTime Gametime { get; set; } = DateTime.Now;
        public List<BasePlayer> Team1 { get; set; } = new List<BasePlayer>();
        public List<BasePlayer> Team2 { get; set; } = new List<BasePlayer>();
        public string Hash { get; set; }
        public double Quality { get; set; }
        public string Server { get; set; } = "NA";
        public bool Accepted { get; set; } = false;
        public bool Declined { get; set; } = false;
        public bool Reported { get; set; } = false;

        public List<BasePlayer> GetPlayers()
        {
            List<BasePlayer> ilist = new List<BasePlayer>();
            ilist.AddRange(Team1);
            ilist.AddRange(Team2);
            return ilist;
        }
    }

    public class RESgame : MMgame
    {
        public int Winner { get; set; }
        public List<RESplayer> Players { get; set; } = new List<RESplayer>();
        public List<BasePlayer> MMPlayers { get; set; } = new List<BasePlayer>();
    }

    public class RetFindGame
    {
        public MMgame Game { get; set; }
        public List<BasePlayer> Players { get; set; } = new List<BasePlayer>();
    }

    public class GameStateChange : INotifyPropertyChanged
    {
        private bool Update_value = false;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int GameID { get; set; }

        public bool Update
        {
            get { return this.Update_value; }
            set
            {
                if (value != this.Update_value)
                {
                    this.Update_value = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }

    [Serializable]
    public class MMPlRating
    {
        public double EXP { get; set; } = 0;
        public double MU { get; set; } = 25;
        public double SIGMA { get; set; } = 25 / 3;
        public int Games { get; set; } = 0;
        public bool Db { get; set; } = false;
        public DateTime Time { get; set; } = DateTime.UtcNow;

        public MMPlRating()
        {

        }

        public MMPlRating(MMdbRating dbrat) : this()
        {
            EXP = dbrat.EXP;
            MU = dbrat.MU;
            SIGMA = dbrat.SIGMA;
            Games = dbrat.Games;
            Db = true;
            Time = dbrat.Time;
        }

        public MMPlRating(MMdbRaceRating dbrat) : this()
        {
            EXP = dbrat.EXP;
            MU = dbrat.MU;
            SIGMA = dbrat.SIGMA;
            Games = dbrat.Games;
            Db = true;
            Time = dbrat.Time;
        }

        public MMPlRating ShallowCopy()
        {
            return (MMPlRating)this.MemberwiseClone();
        }
    }

    [Serializable]
    public class MMplayerNG
    {
        public string Name { get; set; } = "";
        public string AuthName { get; set; }
        public int DBId { get; set; } = 0;
        public bool DBupdate { get; set; } = false;
        public bool Credential { get; set; } = false;
        public bool Accepted { get; set; } = false;
        public bool Declined { get; set; } = false;
        public bool Random { get; set; } = false;
        [JsonIgnore]
        public MMgameNG Game { get; set; } = new MMgameNG();
        [JsonIgnore]
        public HashSet<MMplayerNG> Lobby { get; set; } = new HashSet<MMplayerNG>();
        [JsonIgnore]
        public int Ticks { get; set; } = 0;
        public string Mode { get; set; } = "Commander";
        public string Server { get; set; } = "NA";
        public string Mode2 { get; set; } = "3v3";
        public bool Ladder { get; set; } = false;
        public bool Notify { get; set; } = false;
        public bool Deleted { get; set; } = false;
        public DateTime MMDeleted { get; set; } = new DateTime(2018, 1, 1);
        public double ExpChange { get; set; } = 0;
        public Dictionary<string, List<MMPlRating>> Rating = new Dictionary<string, List<MMPlRating>>()
        {
            { "Commander3v3True", new List<MMPlRating>() },
            { "Commander2v2True", new List<MMPlRating>() },
            { "Commander1v1True", new List<MMPlRating>() },
            { "Standard3v3True", new List<MMPlRating>() },
            { "Standard2v2True", new List<MMPlRating>() },
            { "Standard1v1True", new List<MMPlRating>() },
            { "Commander3v3False", new List<MMPlRating>() },
            { "Commander2v2False", new List<MMPlRating>() },
            { "Commander1v1False", new List<MMPlRating>() },
            { "Standard3v3False", new List<MMPlRating>() },
            { "Standard2v2False", new List<MMPlRating>() },
            { "Standard1v1False", new List<MMPlRating>() }
        };

        public MMplayerNG()
        {
            foreach (var ent in Rating)
            {
                ent.Value.Add(new MMPlRating());
            }
        }

        public MMplayerNG(string name) : this()
        {
            Name = name;
        }

        public MMplayerNG(MMdbPlayer pl) : this()
        {
            Name = pl.Name;
            AuthName = pl.AuthName;
            DBId = pl.MMdbPlayerId;
            Credential = pl.Credential;
            Mode = pl.Mode;
            Server = pl.Server;
            Mode2 = pl.Mode2;
            Ladder = pl.Ladder;
            Deleted = pl.Deleted;
            MMDeleted = pl.MMDeleted;
            if (pl.MMdbRatings != null) {
                foreach (MMdbRating rat in pl.MMdbRatings.OrderBy(o => o.Time))
                {
                    var mrat = new MMPlRating();
                    mrat.EXP = rat.EXP;
                    mrat.Games = rat.Games;
                    mrat.MU = rat.MU;
                    mrat.SIGMA = rat.SIGMA;
                    mrat.Db = true;
                    mrat.Time = rat.Time;
                    Rating[rat.Lobby].Add(mrat);
                }
            }
        }

        public MMplayerNG(MMdbRace pl) : this()
        {
            Name = pl.Name;
            AuthName = pl.AuthName;
            DBId = pl.MMdbRaceId;
            Credential = pl.Credential;
            Mode = pl.Mode;
            Server = pl.Server;
            Mode2 = pl.Mode2;
            Ladder = pl.Ladder;
            Deleted = pl.Deleted;
            MMDeleted = pl.MMDeleted;
            if (pl.MMdbRaceRatings != null)
            {
                foreach (MMdbRaceRating rat in pl.MMdbRaceRatings)
                {
                    var mrat = new MMPlRating();
                    mrat.EXP = rat.EXP;
                    mrat.Games = rat.Games;
                    mrat.MU = rat.MU;
                    mrat.SIGMA = rat.SIGMA;
                    mrat.Db = true;
                    mrat.Time = rat.Time;
                    Rating[rat.Lobby].Add(mrat);
                }
            }
        }

        public MMplayerNG ShallowCopy()
        {
            return (MMplayerNG)this.MemberwiseClone();
        }

    }

    public class MMgameNG
    {
        public int ID { get; set; } = 0;
        public string Lobby { get; set; }
        public int Valid { get; set; } = 0;
        public DateTime Gametime { get; set; } = DateTime.Now;
        public List<MMplayerNG> Team1 { get; set; } = new List<MMplayerNG>();
        public List<MMplayerNG> Team2 { get; set; } = new List<MMplayerNG>();
        public string Hash { get; set; }
        public double Quality { get; set; }
        public string Server { get; set; } = "NA";
        public bool Accepted { get; set; } = false;
        public bool Declined { get; set; } = false;
        public bool Reported { get; set; } = false;
        public GameStateChange State { get; set; } = new GameStateChange();

        public List<MMplayerNG> GetPlayers()
        {
            List<MMplayerNG> ilist = new List<MMplayerNG>();
            ilist.AddRange(Team1);
            ilist.AddRange(Team2);
            return ilist;
        }

        public void RemovePlayer(string name)
        {
            foreach (var pl in Team1.ToArray())
                if (pl.Name == name)
                    Team1.Remove(pl);

            foreach (var pl in Team2.ToArray())
                if (pl.Name == name)
                    Team2.Remove(pl);
        }
    }
}
