using DSmm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSmm.Repositories
{
    public interface IMMrepository
    {
        Task<BasePlayer> LetmePlay(SEplayer sepl);
        Task<RetFindGame> FindGame(string name);
        Task<string> ExitQ(string name);
        Task<string> Decline(string name, string id);
        Task<string> Accept(string name, string id);
        Task<MMgame> Status(string name);
        Task<MMgame> Report(dsweb_electron6.Models.dsreplay replay, string id);
        Task<string> Deleteme(string name);
        Task<string> Random(string name);
        Task<string> Manual(string file, string id);
        Task<DSladder> Ladder(string name);
        Task GetLadder();

        DSladder Ranking { get; }
        DSladder PubRanking { get; }
    }
}
