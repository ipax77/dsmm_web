using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sc2dsstats_mm_dev.Models
{
    [Serializable]
    public class GameComment
    {
        public int RepId { get; set; }
        public List<UserComment> Comments { get; set; } = new List<UserComment>();
        public int Upvotes { get; set; }
        public int Downvotes { get; set; }
        public int Stars { get; set; }
    }

    [Serializable]
    public class UserComment
    {
        public string User { get; set; }
        public string Comment { get; set; }
        public DateTime Time { get; set; }
    }
}
