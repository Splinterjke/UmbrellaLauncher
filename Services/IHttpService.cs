using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace UmbrellaProject.Services
{
    interface IHttpService
    {
        //HttpClient HttpClient { get; set; }
        Task<string> GetVersion();
        //Task<string> GetUserDataAsync(string login, string userKey);
        //Task<string> AuthorizeAsync(string user, string password);
        //Task<string> GetUserAvatarAsync(string login, string userKey);
    }
}
