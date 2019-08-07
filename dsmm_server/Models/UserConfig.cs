using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dsmm_server.Models
{
    public class UserConfig
    {
        public int UserConfigId { get; set; }
        public string ID { get; set; }
        public string Player { get; set; } = "";
        public bool Credential { get; set; } = false;
        public string Mode1 { get; set; } = "Commander";
        public string Mode2 { get; set; } = "3v3";
        public string Server { get; set; } = "NA";
        public bool Deleted { get; set; } = false;
        public DateTime MMDeleted { get; set; } = new DateTime(2018, 1, 1);

        public UserConfig()
        {

        }

        public UserConfig(string name) : this()
        {
            ID = name;
        }

    }
}
