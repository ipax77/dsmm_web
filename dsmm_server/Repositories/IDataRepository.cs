using System.Collections.Generic;
using System.Threading.Tasks;
using DSmm.Models;
using Microsoft.AspNetCore.Http;

namespace DSmm.Repositories
{
    public interface IDataRepository
    {
        string GetLast(string id, string last);
        Task<bool> GetFile(string id, string file);
        Task<string> Info(DSinfo info);
    }
}