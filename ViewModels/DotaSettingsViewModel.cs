using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UmbrellaProject.Services;

namespace UmbrellaProject.ViewModels
{
    class DotaSettingsViewModel : Screen
    {
        private readonly ISettingsService settingsService;

        public DotaSettingsViewModel(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
        }

        //public bool IsCheckCrcEnabled
        //{
        //    get
        //    {
        //        var val = settingsService.ReadValue<int>(SettingsType.DotaCheckCRC);
        //        return val == 1 ? true : false;
        //    }
        //    set
        //    {
        //        var val = value == true ? 1 : 0;
        //        settingsService.SaveValue(SettingsType.DotaCheckCRC, val);
        //    }
        //}

        public string LaunchOptions
        {
            get
            {
                return settingsService.ReadValue<string>(SettingsType.DotaLaunchOptions);
            }
            set
            {
                settingsService.SaveValue(SettingsType.DotaLaunchOptions, value);
            }
        }

        public bool IsAutoUpdateEnabled
        {
            get
            {
                var val = settingsService.ReadValue<int>(SettingsType.DotaScriptsAutoUpdate);
                return val == 1 ? true : false;
            }
            set
            {
                var val = value == true ? 1 : 0;
                settingsService.SaveValue(SettingsType.DotaScriptsAutoUpdate, val);
            }
        }

        public bool IsAntiVACEnabled
        {
            get
            {
                var val = settingsService.ReadValue<int>(SettingsType.AntiVac);
                return val == 1 ? true : false;
            }
            set
            {
                var val = value == true ? 1 : 0;
                settingsService.SaveValue(SettingsType.AntiVac, val);
            }
        }

        public bool IsDotaLaunchTriggerEnabled
        {
            get
            {
                var val = settingsService.ReadValue<int>(SettingsType.DotaLaunchTrigger);
                return val == 1 ? true : false;
            }
            set
            {
                var val = value == true ? 1 : 0;
                settingsService.SaveValue(SettingsType.DotaLaunchTrigger, val);
            }
        }

        public List<string> DXVersions { get; } = new List<string> { "dx9", "dx11" };
        public string SelectedDXVersion
        {
            get
            {
                if (string.IsNullOrEmpty(settingsService.ReadValue<string>(SettingsType.DXVersion)))
                    settingsService.SaveValue(SettingsType.DXVersion, DXVersions[1]);
                return settingsService.ReadValue<string>(SettingsType.DXVersion);
            }
            set
            {
                settingsService.SaveValue(SettingsType.DXVersion, value);
            }
        }
    }
}
