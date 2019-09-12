using DSmm.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using sc2dsstats.Models;

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
        Task<MMgame> Report(dsreplay replay, string id);
        Task<string> Deleteme(string name);
        Task<string> Random(string name);
        Task<string> Manual(string file, string id);
        Task<DSladder> Ladder(string name);
        Task GetLadder();

        DSladder Ranking { get; }
        DSladder PubRanking { get; }
    }

    public interface IMMrepositoryNG
    {
        //ConcurrentDictionary<string, MMplayerNG> MMplayers { get; }
        ObservableCollection<MMgameNG> Games { get; }
        ConcurrentDictionary<int, MMgameNG> Reports { get; }
        List<dsreplay> Replays { get; set; }

        Task<MMplayerNG> LetmePlay(MMplayerNG pl);
        Task Accept(string name, int id);
        Task Decline(string name, int id);
        void ExitQ(string name);
        string FixUnitName(string unit);
    }

}
