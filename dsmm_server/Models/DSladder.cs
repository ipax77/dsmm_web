using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSmm.Models
{
    public class DSladder
    {
        public Dictionary<string, MMplayer> MMplayers { get; set; } = new Dictionary<string, MMplayer>();
        public Dictionary<string, MMplayer> MMraces { get; set; } = new Dictionary<string, MMplayer>();
    }
}
