using Newtonsoft.Json;
using Stylet;
using System;
using System.Collections.Generic;

namespace UmbrellaProject.Models
{
	internal class UserData : PropertyChangedBase
	{
        public long UserId { get; set; }
        public string Username { get; set; }
        public string AvatarPath { get; set; }
        public DateTime SubEndTime { get; set; }
        public DateTime ServerTime { get; set; }
        public bool IsFreeDays { get; set; }
        public bool IsLifeTime { get; set; }
        public string DotaCheatStatus { get; set; }
    }
}
