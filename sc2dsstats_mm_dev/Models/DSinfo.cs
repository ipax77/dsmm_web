using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSmm.Models
{
    public class DSinfo
    {
        public string Name { get; set; }
        public string Json { get; set; }
        public int Total { get; set; }
        public DateTime LastUpload { get; set; }
        public string LastRep { get; set; }
        public string Version { get; set; }
    }
}
