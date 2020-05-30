using Goji;
using Stylet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UmbrellaProject.Services;

namespace UmbrellaProject.ViewModels
{
    class LauncherSettingsViewModel : Screen
    {
        private readonly ISettingsService settingsService;
        private string selectedLanguage;

        public string TabImgSrc => "pack://application:,,,/UmbrellaProject;component/Assets/settings_tab.png";
        public Dictionary<CultureInfo, string> LanguagesList { get; private set; }
        public string SelectedLanguage
        {
            get
            {
                return selectedLanguage;
            }
            set
            {
                if (selectedLanguage == value) return;
                selectedLanguage = value;
                var culture = LanguagesList.FirstOrDefault(x => x.Value == value).Key;
                settingsService.SaveValue(SettingsType.Language, culture.Name);
                settingsService.SaveValue(SettingsType.IsUrlLanguageRemembered, 0);
                Application.Current.SetCurrentUICulture(culture);
            }
        }

        public GameBrowserViewModel GameBrowserVm { get; private set; }

		public void UpgradeSubscription() => GameBrowserVm.MainViewModel.IsPromoKeyDialogOpen = true;

		protected override void OnInitialActivate()
        {
            base.OnInitialActivate();
            GameBrowserVm = this.Parent as GameBrowserViewModel;
        }

        public LauncherSettingsViewModel(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
            LanguagesList = new Dictionary<CultureInfo, string>
            {
                { new CultureInfo("en"), "English" },
                { new CultureInfo("ru"), "Russian" },
                { new CultureInfo("de"), "Deutsch" },
                { new CultureInfo("it"), "Italiano" },
                { new CultureInfo("es"),"Español" },
                { new CultureInfo("fr"),"Français" },
                { new CultureInfo("zh"), "中国" }
            };
            var selectedLanguage = settingsService.ReadValue<string>(SettingsType.Language);
            var languageName = string.Empty;
            if (string.IsNullOrEmpty(selectedLanguage))
            {
                var cultureName = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                if (LanguagesList.TryGetValue(new CultureInfo(cultureName), out languageName))
                    SelectedLanguage = languageName;
                else SelectedLanguage = "English";
                return;
            }            
            if (LanguagesList.TryGetValue(new CultureInfo(selectedLanguage), out languageName))
                SelectedLanguage = languageName;
            else SelectedLanguage = "English";
        }
    }
}