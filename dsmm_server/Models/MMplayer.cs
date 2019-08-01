using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
    
}
