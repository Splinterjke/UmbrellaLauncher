using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Stylet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using UmbrellaProject.Extensions;
using UmbrellaProject.Services;

namespace UmbrellaProject.ViewModels
{
    internal class DotaViewModel : Conductor<IScreen>
    {
        private readonly IHttpService httpService;
        private readonly ISettingsService settingsService;

        public string TabImgSrc => "pack://application:,,,/UmbrellaProject;component/Assets/dota_tab.png";

        public string CheatStatusText { get; private set; } = "Updated";
        public SolidColorBrush StatusColor { get; private set; } = Brushes.YellowGreen;
        public DotaNewsViewModel NewsVm { get; }
        public DotaScriptsViewModel ScriptsVm { get; }
        public DotaSettingsViewModel SettingsVm { get; }
        public GameBrowserViewModel GameBrowserVm { get; private set; }

        public DotaViewModel(DotaNewsViewModel newsVm, DotaScriptsViewModel scriptsVm, DotaSettingsViewModel settingsVm, IHttpService httpService, ISettingsService settingsService)
        {
            NewsVm = newsVm;
            ScriptsVm = scriptsVm;
            SettingsVm = settingsVm;
            if (settingsService.ReadValue<int>(SettingsType.IsUrlLanguageRemembered) == 4)
                settingsService.SaveValue(SettingsType.IsUrlLanguageRemembered, 1);
            if (settingsService.ReadValue<int>(SettingsType.AntiVac) == 4)
                settingsService.SaveValue(SettingsType.AntiVac, 1);
            if (settingsService.ReadValue<int>(SettingsType.DotaCheckCRC) == 4)
                settingsService.SaveValue(SettingsType.DotaCheckCRC, 1);
            if (settingsService.ReadValue<int>(SettingsType.DotaScriptsAutoUpdate) == 4)
                settingsService.SaveValue(SettingsType.DotaScriptsAutoUpdate, 0);
            if (settingsService.ReadValue<int>(SettingsType.DotaLaunchTrigger) == 4)
                settingsService.SaveValue(SettingsType.DotaLaunchTrigger, 0);
            this.httpService = httpService;
            this.settingsService = settingsService;
            ActiveItem = scriptsVm;
        }

        protected override void OnInitialActivate()
        {
            base.OnInitialActivate();
            GameBrowserVm = this.Parent as GameBrowserViewModel;
            CheatStatusText = GameBrowserVm.UserData.DotaCheatStatus;
            StatusColor = CheatStatusText == "Updated" ? Brushes.YellowGreen : Brushes.OrangeRed;
        }

		public void ShowNews() => ActiveItem = NewsVm;

		public void ShowScripts() => ActiveItem = ScriptsVm;

		public void ShowSettings() => ActiveItem = SettingsVm;

		public void UpgradeSubscription() => GameBrowserVm.MainViewModel.IsPromoKeyDialogOpen = true;

		public async Task StartDota()
        {
            try
            {
                GameBrowserVm.MainViewModel.IsLoading = true;
                GameBrowserVm.UserData.ServerTime = DateTime.UtcNow;
                if (GameBrowserVm.UserData.SubEndTime.ToUniversalTime() < GameBrowserVm.UserData.ServerTime.ToUniversalTime() && !GameBrowserVm.UserData.IsFreeDays && !GameBrowserVm.UserData.IsLifeTime)
                {
                    GameBrowserVm.MainViewModel.ShowError(Properties.Resources.SubTimeExpired);
                    return;
                }
                var antiVacSetting = settingsService.ReadValue<int>(SettingsType.AntiVac);
                if (antiVacSetting == 0 && Process.GetProcesses().FirstOrDefault(x => x.ProcessName == "Steam") == null)
                {
                    GameBrowserVm.MainViewModel.ShowError(Properties.Resources.AskForSteamStart);
                    return;
                }
                GameBrowserVm.MainViewModel.LoadingText = antiVacSetting == 1 ? "SteamRebooting" : "Injecting";
                var argsSettings = settingsService.ReadValue<string>(SettingsType.DotaLaunchOptions);
                var dxVersion = string.IsNullOrEmpty(settingsService.ReadValue<string>(SettingsType.DXVersion)) ? "dx11" : settingsService.ReadValue<string>(SettingsType.DXVersion);
				//if (settingsService.ReadValue<int>(SettingsType.DotaLaunchTrigger) == 0)
				//	await CheckSteamLaunched();
				//else
				//{
				//	GameBrowserVm.MainViewModel.IsManualDotaStartTriggerEnabled = true;
				//	while (GameBrowserVm.MainViewModel.IsManualDotaStartTriggerEnabled)
				//	{
				//		await Task.Delay(150);
				//		continue;
				//	}
				//}
				GameBrowserVm.MainViewModel.LoadingText = "LaunchingDota";
				await Task.Delay(1000);
				Process.Start("steam://run/570");
				GameBrowserVm.MainViewModel.MinimizeWindow();
			}
            catch (Exception ex)
            {
                GameBrowserVm.MainViewModel.ShowError(ex.Message);
                Log.Error(ex, "null");
            }
            finally
            {
                GameBrowserVm.MainViewModel.IsLoading = false;
                GameBrowserVm.MainViewModel.LoadingText = "LoadingText";
            }
        }

        private async Task CheckSteamLaunched()
        {
            var webHelpersIds = new List<int>();
            while (true)
            {
                var processes = Process.GetProcesses();
                foreach (Process process in processes)
                {
                    if (webHelpersIds.Count == 4)
                    {
                        await Task.Delay(5000);
                        return;
                    }

                    if (process.ProcessName == "steamwebhelper")
                    {
                        if (!webHelpersIds.Contains(process.Id))
                        {
                            webHelpersIds.Add(process.Id);
                        }
                    }

                }
                await Task.Delay(1000);
            }
        }
    }
}
