using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sc2dsstats_mm_dev.Models
{
    [Serializable]
    public class GameComment
    {
        public int RepId { get; set; } = 0;
        public List<UserComment> Comments { get; set; } = new List<UserComment>();
        public HashSet<string> Upvotes { get; set; } = new HashSet<string>();
        public HashSet<string> Downvotes { get; set; } = new HashSet<string>();
        public int Stars { get; set; }
        public string YouTube { get; set; }
    }

    [Serializable]
    public class UserComment
    {
        public string User { get; set; }
        public string Comment { get; set; }
        public DateTime Time { get; set; }
    }
}
