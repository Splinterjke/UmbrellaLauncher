using PropertyChanged;
using Stylet;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using UmbrellaProject.Models;
using UmbrellaProject.Services;

namespace UmbrellaProject.ViewModels
{
	internal class GameBrowserViewModel : Conductor<IScreen>.Collection.OneActive
    {
        private readonly ISettingsService settingsService;

        public MainViewModel MainViewModel { get; private set; }
        public DotaViewModel DotaVm { get; private set; }
        public UserData UserData { get; set; } = new UserData
        {
			AvatarPath = "https://www.explica.co/wp-content/uploads/2020/04/642951_600x315.jpg",
			DotaCheatStatus = "OnUpdatingStatus",
            ServerTime = DateTime.UtcNow,
            SubEndTime = DateTime.UtcNow.AddYears(-1),
            UserId = 0,
            Username = "Developer"
        };

        private IScreen lastActiveItem;
        private IReadOnlyList<IScreen> items;

        public GameBrowserViewModel(DotaViewModel dotaVm, LauncherSettingsViewModel lsVm, ISettingsService settingsService)
        {
            DotaVm = dotaVm;
            Items.Add(dotaVm);
            Items.Add(lsVm);
            //Items.Add(pubgVm);
            //Items.Add(csgoVm);
            items = Items.ToList();
            this.settingsService = settingsService;
        }

        protected override void OnInitialActivate()
        {
            base.OnInitialActivate();
            MainViewModel = this.Parent as MainViewModel;
        }

        public override void ActivateItem(IScreen item)
        {
            base.ActivateItem(item);
            lastActiveItem = item;
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            if (Items == null || Items.Count == 0)
                Items.AddRange(items);
            ActiveItem = lastActiveItem ?? Items[0];
		}

        public void LogOut()
        {
            settingsService.SaveValue(SettingsType.Password, string.Empty);
            settingsService.SaveValue(SettingsType.IsAuthorized, 0);
            MainViewModel.NavigateTo(MainViewModel.LoginVm);
        }
    }
}
