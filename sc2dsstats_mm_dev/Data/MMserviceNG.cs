using System;
using System.Timers;
using DSmm.Models;

namespace dsmm_server.Data
{
    public class MMserviceNG
    {
        public double Done { get; set; } = 24;
        public string Info { get; set; } = "";
        public string InfoBadge { get; set; } = "badge-secondary";
        public string InfoBadgeMsg { get; set; } = "Offline";
        public bool isFindDisabled { get; set; } = false;
        public bool isDeclinedDisabled { get; set; } = false;
        public bool isAcceptDisabled { get; set; } = false;
        public bool MMDeleted { get; set; } = false;
        public double MMDel { get; set; } = 3;
        public string GameClass { get; set; } = "colapsed";
        public int dd { get; set; } = 0;
        public bool RepSelDisabled { get; set; } = true;
        public string temp_Playername { get; set; }
        public bool Failed { get; set; } = false;
        public bool Saved { get; set; } = false;
        public string Error { get; set; } = "";
        public bool UpdateRunning { get; set; } = false;
        public bool ReportIsClicked { get; set; } = false;
        public bool AllowRandoms { get; set; } = false;
        public string RandomIsDisabled { get; set; } = "d-none";
        public int RepID { get; set; } = 0;
        public TimeSpan _time { get; set; } = new TimeSpan(0);

        public bool GAMEFOUND { get; set; } = false;
        public MMgameNG Game { get; set; } = new MMgameNG();
        public MMgameNG preGame { get; set; } = new MMgameNG();

        public Timer aTimer;


    }
}
