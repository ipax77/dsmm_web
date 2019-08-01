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
using DSmm.Models;

namespace dsweb_electron6.Models
{
    public static class DSrest
    {
        
        //public static RestClient Client = new RestClient("https://localhost:44393/");
        public static RestClient Client = new RestClient("https://www.pax77.org:9128/");

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


 
}
