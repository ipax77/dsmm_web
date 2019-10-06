using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sc2dsstats_mm_dev.Models
{
    [Serializable]
    public class TournamentInfo
    {
        public string Info { get; set; }
        public string Bracket { get; set; }
        public string[] Teams { get; set; }
        public string Winner { get; set; }
    }
}
