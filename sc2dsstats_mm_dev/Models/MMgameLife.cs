 using DSmm.Models;
using pax.s2decode.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dsmm_server.Models
{
    public class MMgameLife
    {
        public int MMID { get; set; }
        public DateTime Time { get; set; }
        public MMgameNG GameFound { get; set; }
        public MMgameNG GameAccepted { get; set; }
        public MMgameNG GameReported { get; set; }
        public List<dsreplay> Replays { get; set; }
    }
}
