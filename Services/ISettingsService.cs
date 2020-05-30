using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmbrellaProject.Services
{
    interface ISettingsService
    {
        T ReadValue<T>(SettingsType settingsType) where T : IConvertible;
        void SaveValue<T>(SettingsType settingsType, T value);
    }
}
