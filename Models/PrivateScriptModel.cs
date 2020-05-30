using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmbrellaProject.Models
{
    public class PrivateScriptModel
    {
        [JsonProperty("name")]
        public string ScriptName { get; set; }

        [JsonProperty("isEnabled")]
        public bool IsEnabled { get; set; }

        [JsonProperty("forumUrl")]
        public string ForumUrl { get; set; }

        [JsonProperty("forumUrl_EN")]
        public string ForumUrlEN { get; set; }
    }
}
