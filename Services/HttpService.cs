using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using UmbrellaProject.Models;

namespace UmbrellaProject.Services
{
    internal class HttpService : IHttpService
    {
        //public HttpClient HttpClient { get; set; }

        //public HttpService()
        //{
        //    HttpClient = new HttpClient
        //    {
        //        Timeout = TimeSpan.FromSeconds(10),
        //    };
        //    HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.139 Safari/537.36");
        //}

        //private async Task<string> GetAsync(string url)
        //{
        //    var response = await HttpClient.GetAsync(url, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
        //    response.EnsureSuccessStatusCode();
        //    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        //    return content;
        //}

        public async Task<string> GetVersion()
        {
            var request = (HttpWebRequest)WebRequest.Create("http://uc.zone/dist/loaderinfo.php?action=getUmbrellaVersion");
            request.Timeout = 10000;
            request.Method = "GET";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.139 Safari/537.36";
            try
            {
                using (var response = (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (var stream = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                        {
                            return stream.ReadToEnd();
                        }
                    }
                    return null;

                }
            }
            catch (WebException)
            {
                return null;
            }
        }

        //public async Task<string> AuthorizeAsync(string login, string password)
        //{            
        //    var response = await GetAsync($"authenticate&username={login}&password={password}").ConfigureAwait(false);
        //    return response;
        //}

        //public async Task<string> GetUserDataAsync(string login, string userKey)
        //{
        //    var response = await GetAsync($"getUser&hash={login}:{userKey}").ConfigureAwait(false);
        //    return response;
        //}

        //public async Task<string> GetUserAvatarAsync(string login, string userKey)
        //{
        //    var response = await GetAsync($"getAvatar&hash={login}:{userKey}").ConfigureAwait(false);
        //    return response;
        //}
    }
}
