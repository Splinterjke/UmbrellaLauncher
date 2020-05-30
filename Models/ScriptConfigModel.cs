using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmbrellaProject.Models
{
    internal class ScriptConfigModel
    {
        [JsonProperty("scriptName")]
        public string ScriptName { get; set; }

        [JsonProperty("scriptPath")]
        public string ScriptPath { get; set; }

        [JsonProperty("scriptLocalPath")]
        public string ScriptLocalPath { get; set; }

        //[JsonProperty("lastCommitTime")]
        //public DateTimeOffset LastCommitTime { get; set; }

        //[JsonProperty("preUpdateTime")]
        //public DateTimeOffset? PreUpdateTime { get; set; }

        //[JsonProperty("isUpdateAvailable")]
        //public bool IsUpdateAvailable { get; set; }

        [JsonProperty("repositoryPath")]
        public string RepositoryPath { get; set; }

        [JsonProperty("isEnabled")]
        public bool IsEnabled { get; set; }
    }
}
