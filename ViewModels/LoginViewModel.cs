using Goji;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Stylet;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using UmbrellaProject.Extensions;
using UmbrellaProject.Models;
using UmbrellaProject.Services;

namespace UmbrellaProject.ViewModels
{
	internal class LoginViewModel : Screen
	{
		private readonly ISettingsService settingsService;
		private readonly IHttpService httpService;

		public MainViewModel MainViewModel { get; private set; }
		private SecureString Password { get; set; }
		public string Login { get; set; }
		public Dictionary<string, string> ServerList { get; private set; }
		private int[] serverPorts;
		public int SelectedServer { get; set; }

		public LoginViewModel(ISettingsService settingsService, IHttpService httpService)
		{
			this.settingsService = settingsService;
			this.httpService = httpService;
			ServerList = new Dictionary<string, string> { { "127.0.0.1", "Netherlands" }, { "127.0.0.1", "Moscow" }, { "127.0.0.1", "Hongkong" } };
			serverPorts = new int[] { 999, 999, 999 };
			Login = settingsService.ReadValue<string>(SettingsType.Login);
			SelectedServer = settingsService.ReadValue<int>(SettingsType.ServerIndex);
			if (SelectedServer == 4)
			{
				SelectedServer = 0;
				settingsService.SaveValue(SettingsType.ServerIndex, SelectedServer);
			}
		}

		protected override async void OnViewLoaded()
		{
			base.OnViewLoaded();
			var pb = (this.View as UserControl).FindName("PasswordBox") as PasswordBox;
			pb.Password = settingsService.ReadValue<string>(SettingsType.Password);
			PasswordChanged(pb, null);
			var isAuth = settingsService.ReadValue<int>(SettingsType.IsAuthorized);
			if (isAuth == 1 && Login != null && Password?.Length > 0)
			{
				await LoginCommand();
			}
			MainViewModel.IsLoading = false;
			MainViewModel.LoadingText = "LoadingText";
			MainViewModel.LoadingProgress = string.Empty;
		}

		private void EnsureDirectoryExists(string filePath)
		{
			FileInfo fi = new FileInfo(filePath);
			if (!fi.Directory.Exists)
			{
				System.IO.Directory.CreateDirectory(fi.DirectoryName);
			}
		}

		private string SecureStringToString(SecureString value)
		{
			IntPtr bstr = Marshal.SecureStringToBSTR(value);

			try
			{
				return Marshal.PtrToStringBSTR(bstr);
			}
			finally
			{
				Marshal.FreeBSTR(bstr);
			}
		}

		protected override void OnInitialActivate()
		{
			base.OnInitialActivate();
			MainViewModel = this.Parent as MainViewModel;
		}

		public void PasswordChanged(object sender, System.Windows.RoutedEventArgs e) => Password = ((PasswordBox)sender).SecurePassword;

		public async void KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Enter && CheckAuthForm())
				await LoginCommand();
		}

		protected override void OnValidationStateChanged(IEnumerable<string> changedProperties)
		{
			base.OnValidationStateChanged(changedProperties);
			this.NotifyOfPropertyChange(() => this.CanLoginCommand);
		}

		public bool CanLoginCommand => CheckAuthForm();

		private bool CheckAuthForm()
		{
			if (string.IsNullOrEmpty(Login))
				return false;
			return Password != null && Password.Length != 0;
		}

		public async Task LoginCommand()
		{
			MainViewModel.LoadingText = "LoadingText";
			MainViewModel.IsLoading = true;
			var insecurePassword = SecureStringToString(Password);
			insecurePassword = insecurePassword.Trim();
			Login = Login.Trim();
			settingsService.SaveValue(SettingsType.Login, Login);
			settingsService.SaveValue(SettingsType.IsAuthorized, 0);
			settingsService.SaveValue(SettingsType.Password, insecurePassword);
			settingsService.SaveValue(SettingsType.ServerIndex, SelectedServer);
			MainViewModel.LoadingText = "Authorizing";
			settingsService.SaveValue(SettingsType.Password, insecurePassword);
			settingsService.SaveValue(SettingsType.IsAuthorized, 1);

			if (File.Exists("privatescripts.json"))
			{
				try
				{
					var privateScriptsJson = File.ReadAllText("privatescripts.json");
					var privateScripts = JsonConvert.DeserializeObject<ObservableCollection<PrivateScriptModel>>(privateScriptsJson);
					MainViewModel.GameBrowserVm.DotaVm.ScriptsVm.PrivateScriptsData = privateScripts;
				}
				catch
				{
					MainViewModel.ShowError("Couldn't parse privatescripts.json");
					return;
				}
			}
			await Task.Delay(1500);
			MainViewModel.ShowAuthorized = true;
			await Task.Delay(1900);
			MainViewModel.IsLoading = false;
			MainViewModel.ShowAuthorized = false;
			MainViewModel.NavigateTo(MainViewModel.GameBrowserVm);
			MainViewModel.IsLoading = false;
			MainViewModel.ShowAuthorized = false;
		}
	}
}
