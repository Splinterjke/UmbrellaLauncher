using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Stylet;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using UmbrellaProject.Extensions;
using UmbrellaProject.Models;
using UmbrellaProject.Services;

namespace UmbrellaProject.ViewModels
{
	internal class MainViewModel : Conductor<IScreen>
	{
		public static string AssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

		public ISettingsService SettingsService { get; }
		public LoginViewModel LoginVm { get; }
		public GameBrowserViewModel GameBrowserVm { get; }
		public bool IsLoading { get; set; }
		public string LoadingText { get; set; } = "LoadingText";
		public string LoadingProgress { get; set; }
		public bool ShowAuthorized { get; set; }
		public bool IsErrored { get; set; }
		public bool IsSuccess { get; set; }
		public string ErrorText { get; set; }
		public string SuccessText { get; set; }
		public string Version { get; private set; }
		public bool IsManualDotaStartTriggerEnabled { get; set; }
		public bool IsPromoKeyDialogOpen { get; set; }
		public string PromoKey { get; set; }

		public MainViewModel(ISettingsService settingsService, LoginViewModel loginVm, GameBrowserViewModel gameBrowserVm)
		{
			settingsService.SaveValue<string>(SettingsType.ExePath, AssemblyPath);
			var ver = Assembly.GetExecutingAssembly().GetName().Version;
			Version = $"{ver.Major}.{ver.Minor}.{ver.Build}";
			SettingsService = settingsService;
			LoginVm = loginVm;
			GameBrowserVm = gameBrowserVm;
			NavigateTo(LoginVm);
		}

		public void NavigateTo(IScreen page) => ActiveItem = page;

		public void DragMove() => (this.View as Window).DragMove();

		public void ShowError(string message)
		{
			ErrorText = message;
			IsErrored = true;
		}

		public void ShowSuccess(string message)
		{
			SuccessText = message;
			IsSuccess = true;
		}

		public void CloseError() => IsErrored = false;

		public void CloseSuccess() => IsSuccess = false;

		public void ManualDotaStartTrigger() => IsManualDotaStartTriggerEnabled = false;

		public async void OnClosePromoKeyDialog(DialogClosingEventArgs e)
		{
			if((bool)e.Parameter)
			{
				IsLoading = true;
				GameBrowserVm.UserData.SubEndTime = DateTime.Now.AddDays(30);
				GameBrowserVm.Refresh();
				await Task.Delay(1000);
				ShowSuccess(Properties.Resources.PromoKey_success);
				IsLoading = false;
			}
			
			PromoKey = string.Empty;
		}

		public void MinimizeWindow() => Application.Current.MainWindow.WindowState = WindowState.Minimized;

		public void CloseWindow() => Application.Current.Shutdown();
	}
}