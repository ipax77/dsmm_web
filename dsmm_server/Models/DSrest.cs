using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using dsweb_electron6.Models;
using dsweb_electron6;

namespace dsweb_electron6.Models
{
    public static class DSrest
    {
        
        public static RestClient Client = new RestClient("https://localhost:44393/");
        //public static RestClient Client = new RestClient("https://www.pax77.org:9128/");

        public static BasePlayer LetmePlay(SEplayer player)
        {
            var restRequest = new RestRequest("/mm/letmeplay", Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("Authorization", "DSupload77");
            restRequest.AddJsonBody(player);
            //var response = client.Execute(restRequest);
            try
            {
                var response = Client.Execute<BasePlayer>(restRequest);
                return response.Data;
            } catch {
                return null;
            }
        }

        public static RetFindGame FindGame(string name)
        {
            var restRequest = new RestRequest("/mm/findgame/" + name, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("Authorization", "DSupload77");
            try
            {
                var response = Client.Execute<RetFindGame>(restRequest);
                return response.Data;
            } catch
            {
                return null;
            }
        }

        public static void ExitQ (string name)
        {
            var restRequest = new RestRequest("/mm/exitq/" + name, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("Authorization", "DSupload77");
            try
            {
                var response = Client.Execute<RetFindGame>(restRequest);
            }
            catch
            {
            }
        }

        public static MMgame Status(int id)
        {
            var restRequest = new RestRequest("/mm/status/" + id, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("Authorization", "DSupload77");
            try
            {
                var response = Client.Execute<MMgame>(restRequest);
                return response.Data;
            }
            catch
            {
                return null;
            }
        }

        public static void Accept(string name, int id)
        {
            var restRequest = new RestRequest("/mm/accept/" + name + "/" + id, Method.GET);
            restRequest.AddHeader("Authorization", "DSupload77");
            try
            {
                var response = Client.Execute(restRequest);
            }
            catch
            {
            }
        }

        public static void Decline(string name, int id)
        {
            var restRequest = new RestRequest("/mm/decline/" + name + "/" + id, Method.GET);
            restRequest.AddHeader("Authorization", "DSupload77");
            try
            {
                var response = Client.Execute(restRequest);
            }
            catch
            {
            }
        }

        public static void Deleteme(string name)
        {
            var restRequest = new RestRequest("/mm/deleteme/" + name, Method.GET);
            restRequest.AddHeader("Authorization", "DSupload77");
            try
            {
                var response = Client.Execute(restRequest);
            }
            catch
            {
            }
        }

        public static MMgame Report(dsreplay rep, int id)
        {
            var json = JsonSerializer.Serialize(rep);
            var restRequest = new RestRequest("/mm/report/" + id, Method.POST);
            restRequest.AddHeader("Authorization", "DSupload77");
            restRequest.AddParameter("application/json; charset=utf-8", json, ParameterType.RequestBody);
            restRequest.AddJsonBody(rep);
            //var response = client.Execute(restRequest);
            try
            {
                var response = Client.Execute<MMgame>(restRequest);
                return response.Data;
            }
            catch
            {
                return null;
            }
        }

        public static void Random(string name)
        {
            var restRequest = new RestRequest("/mm/random/" + name, Method.GET);
            restRequest.AddHeader("Authorization", "DSupload77");
            try
            {
                var response = Client.Execute(restRequest);
            }
            catch
            {
            }
        }

        public static string GetHash(HashAlgorithm hashAlgorithm, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }

    [Serializable]
    public class DSinfo
    {
        public string Name { get; set; }
        public string Json { get; set; }
        public int Total { get; set; }
        public DateTime LastUpload { get; set; }
        public string LastRep { get; set; }
        public string Version { get; set; }
    }


    [Serializable]
    public class BasePlayer
    {
        public string Name { get; set; }
        public double EXP { get; set; } = 0;
        public double MU { get; set; } = 25;
        public double SIGMA { get; set; } = 25 / 3;
        public int Games { get; set; } = 0;
        public bool Accepted { get; set; } = false;
    }

    [Serializable]
    public class MMplayer : BasePlayer
    {
        public MMgame Game { get; set; }
        public ConcurrentDictionary<MMplayer, byte> Lobby { get; set; } = new ConcurrentDictionary<MMplayer, byte>();
        public int Lobbysize { get; set; } = 0;
        public int Ticks { get; set; } = 0;
        public string Mode { get; set; }
        public string Server { get; set; }
        public string Mode2 { get; set; }
        public bool Random { get; set; } = false;
        

        public MMplayer()
        {

        }

        public MMplayer(string name) : this()
        {
            Name = name;
        }

        public MMplayer(SEplayer sepl) : this()
        {
            Name = sepl.Name;
            Mode = sepl.Mode;
            Server = sepl.Server;
            Mode2 = sepl.Mode2;
        }
    }

    [Serializable]
    public class SEplayer : MMplayer
    {
        public string Mode { get; set; } = "Commander";
        public string Server { get; set; } = "NA";
        public string Mode2 { get; set; } = "3v3";
        public bool Random { get; set; } = false;
    }

    [Serializable]
    public class RESplayer
    {
        public string Name { get; set; }
        public string Race { get; set; }
        public int Kills { get; set; }
        public int Team { get; set; }
        public int Result { get; set; }
        public int Pos { get; set; }
    }

    [Serializable]
    public class RESgame
    {
        public int Winner { get; set; }
        public DateTime Gametime { get; set; } = DateTime.Now;
        public string Hash { get; set; }
        public List<RESplayer> Players { get; set; } = new List<RESplayer>();
        public List<MMplayer> MMPlayers { get; set; } = new List<MMplayer>();
    }

    [Serializable]
    public class MMgame
    {
        public int ID { get; set; } = 0;
        public DateTime Gametime { get; set; } = DateTime.Now;
        public List<BasePlayer> Team1 { get; set; } = new List<BasePlayer>();
        public List<BasePlayer> Team2 { get; set; } = new List<BasePlayer>();
        public string Hash { get; set; } = "";
        public double Quality { get; set; } = 0;
        public string Server { get; set; } = "NA";
        public bool Accepted { get; set; } = false;
        public bool Declined { get; set; } = false;

        public List<BasePlayer> Players ()
        {
            List<BasePlayer> ilist = new List<BasePlayer>();
            ilist.AddRange(Team1);
            ilist.AddRange(Team2);
            return ilist;
        }
    }

    [Serializable]
    public class RetFindGame
    {
        public MMgame Game { get; set; }
        public List<BasePlayer> Players { get; set; }
    }
}
