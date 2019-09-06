using DSmm.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace dsmm_server.Data
{
    public class MMdb : DbContext
    {
        public MMdb(DbContextOptions<MMdb> options)
            : base(options)
        {
        }
        public DbSet<MMdbPlayer> MMdbPlayers { get; set; }
        public DbSet<MMdbRace> MMdbRaces { get; set; }
        public DbSet<MMdbRating> MMdbRatings { get; set; }
        public DbSet<MMdbRaceRating> MMdbRaceRatings { get; set; }
    }

    public class MMdbPlayer
    {
        public int MMdbPlayerId { get; set; }
        public string Name { get; set; }
        public string AuthName { get; set; }
        public string Mode { get; set; } = "Commander";
        public string Server { get; set; } = "NA";
        public string Mode2 { get; set; } = "3v3";
        public bool Ladder { get; set; } = false;
        public bool Credential { get; set; } = false;
        public bool Deleted { get; set; } = false;
        public DateTime MMDeleted { get; set; } = new DateTime(2018, 1, 1);
        public ICollection<MMdbRating> MMdbRatings { get; set; }

        public MMdbPlayer()
        {
        }

        public MMdbPlayer(MMplayerNG pl)
        {
            Name = pl.Name;
            AuthName = pl.AuthName;
            if (pl.DBId > 0) MMdbPlayerId = pl.DBId;
            Mode = pl.Mode;
            Server = pl.Server;
            Mode2 = pl.Mode2;
            Ladder = pl.Ladder;
            Credential = pl.Credential;
            Deleted = pl.Deleted;
            MMDeleted = pl.MMDeleted;
            /**
            foreach (var lobby in pl.Rating.Keys)
            {
                MMdbRating dbrat = new MMdbRating();
                var rat = pl.Rating[lobby];
                dbrat.EXP = rat.EXP;
                dbrat.Games = rat.Games;
                dbrat.Lobby = lobby;
                dbrat.MU = rat.MU;
                dbrat.SIGMA = rat.SIGMA;

                MMdbRatings = new List<MMdbRating>();
                MMdbRatings.Add(dbrat);
            }
            **/
        }
    }

    public class MMdbRace
    {
        public int MMdbRaceId { get; set; }
        public string Name { get; set; }
        public string AuthName { get; set; }
        public string Mode { get; set; } = "Commander";
        public string Server { get; set; } = "NA";
        public string Mode2 { get; set; } = "3v3";
        public bool Ladder { get; set; } = false;
        public bool Credential { get; set; } = false;
        public bool Deleted { get; set; } = false;
        public DateTime MMDeleted { get; set; } = new DateTime(2018, 1, 1);
        public ICollection<MMdbRaceRating> MMdbRaceRatings { get; set; }

        public MMdbRace()
        {
        }

        public MMdbRace(MMplayerNG pl)
        {
            Name = pl.Name;
            AuthName = pl.AuthName;
            if (pl.DBId > 0) MMdbRaceId = pl.DBId;
            Mode = pl.Mode;
            Server = pl.Server;
            Mode2 = pl.Mode2;
            Ladder = pl.Ladder;
            Credential = pl.Credential;
            Deleted = pl.Deleted;
            MMDeleted = pl.MMDeleted;
            /**
            foreach (var lobby in pl.Rating.Keys)
            {
                MMdbRating dbrat = new MMdbRating();
                var rat = pl.Rating[lobby];
                dbrat.EXP = rat.EXP;
                dbrat.Games = rat.Games;
                dbrat.Lobby = lobby;
                dbrat.MU = rat.MU;
                dbrat.SIGMA = rat.SIGMA;

                MMdbRatings = new List<MMdbRating>();
                MMdbRatings.Add(dbrat);
            }
            **/
        }
    }

    public class MMdbRating
    {
        public int MMdbRatingId { get; set; }
        public string Lobby { get; set; }
        public double EXP { get; set; } = 0;
        public double MU { get; set; } = 25;
        public double SIGMA { get; set; } = 25 / 3;
        public int Games { get; set; } = 0;
        public DateTime Time { get; set; }

        public int MMdbPlayerId { get; set; }
        public MMdbPlayer MMdbPlayer { get; set; }
    }

    public class MMdbRaceRating
    {
        public int MMdbRaceRatingId { get; set; }
        public string Lobby { get; set; }
        public double EXP { get; set; } = 0;
        public double MU { get; set; } = 25;
        public double SIGMA { get; set; } = 25 / 3;
        public int Games { get; set; } = 0;
        public DateTime Time { get; set; }

        public int MMdbRaceId { get; set; }
        public MMdbRace MMdbRace { get; set; }
    }

}
