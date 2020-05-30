using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UmbrellaProject.Extensions;

namespace UmbrellaProject.Services
{
    enum SettingsType
    {
        IsAuthorized,
        Login,
        Password,
        DotaScriptsAutoUpdate,
        DotaCheckCRC,
        DotaLaunchOptions,
        AntiVac,
        DotaLaunchTrigger,
        Language,
        ServerIndex,
        DXVersion,
        IsUrlLanguageRemembered,
        UrlLangType,
        ExePath
    }

    internal class SettingsService : ISettingsService
    {
        public T ReadValue<T>(SettingsType settingsType) where T : IConvertible
        {
            try
            {
                var software = Registry.CurrentUser.OpenSubKey("Software", true);
                var umbrella = software.CreateSubKey("Umbrella");
                switch (settingsType)
                {
                    case SettingsType.IsUrlLanguageRemembered:
                        return ValueTypeCaster<T>.From(umbrella.GetValue(settingsType.ToString().ToLower(), RegistryValueKind.DWord));
                    case SettingsType.IsAuthorized:
                        return ValueTypeCaster<T>.From(umbrella.GetValue(settingsType.ToString().ToLower(), RegistryValueKind.DWord));
                    case SettingsType.DotaScriptsAutoUpdate:
                        return ValueTypeCaster<T>.From(umbrella.GetValue(settingsType.ToString().ToLower(), RegistryValueKind.DWord));
                    case SettingsType.DotaCheckCRC:
                        return ValueTypeCaster<T>.From(umbrella.GetValue(settingsType.ToString().ToLower(), RegistryValueKind.DWord));
                    case SettingsType.AntiVac:
                        return ValueTypeCaster<T>.From(umbrella.GetValue(settingsType.ToString().ToLower(), RegistryValueKind.DWord));
                    case SettingsType.DotaLaunchTrigger:
                        return ValueTypeCaster<T>.From(umbrella.GetValue(settingsType.ToString().ToLower(), RegistryValueKind.DWord));
                    case SettingsType.ServerIndex:
                        return ValueTypeCaster<T>.From(umbrella.GetValue(settingsType.ToString().ToLower(), RegistryValueKind.DWord));
                    case SettingsType.DotaLaunchOptions:
                        return (T)Convert.ChangeType(umbrella.GetValue(settingsType.ToString().ToLower()), typeof(T));
                    case SettingsType.Login:
                        return (T)Convert.ChangeType(umbrella.GetValue(settingsType.ToString().ToLower()), typeof(T));
                    case SettingsType.Password:
                        return (T)Convert.ChangeType(umbrella.GetValue(settingsType.ToString().ToLower()), typeof(T));
                    case SettingsType.Language:
                        return (T)Convert.ChangeType(umbrella.GetValue(settingsType.ToString().ToLower()), typeof(T));
                    case SettingsType.DXVersion:
                        return (T)Convert.ChangeType(umbrella.GetValue(settingsType.ToString().ToLower()), typeof(T));
                    case SettingsType.UrlLangType:
                        return (T)Convert.ChangeType(umbrella.GetValue(settingsType.ToString().ToLower()), typeof(T));
                    default:
                        return default(T);
                }
            }            
            catch
            {
                return default(T);
            }
        }

        public void SaveValue<T>(SettingsType settingsType, T value)
        {
            var software = Registry.CurrentUser.OpenSubKey("Software", true);
            var umbrella = software.CreateSubKey("Umbrella");
            switch (settingsType)
            {
                case SettingsType.ExePath:
                    umbrella.SetValue(settingsType.ToString().ToLower(), (T)Convert.ChangeType(value, typeof(T)));
                    break;
                case SettingsType.IsUrlLanguageRemembered:
                    umbrella.SetValue(settingsType.ToString().ToLower(), ValueTypeCaster<T>.From(value), RegistryValueKind.DWord);
                    break;
                case SettingsType.IsAuthorized:
                    umbrella.SetValue(settingsType.ToString().ToLower(), ValueTypeCaster<T>.From(value), RegistryValueKind.DWord);
                    break;
                case SettingsType.DotaScriptsAutoUpdate:
                    umbrella.SetValue(settingsType.ToString().ToLower(), ValueTypeCaster<T>.From(value), RegistryValueKind.DWord);
                    break;
                case SettingsType.DotaCheckCRC:
                    umbrella.SetValue(settingsType.ToString().ToLower(), ValueTypeCaster<T>.From(value), RegistryValueKind.DWord);
                    break;
                case SettingsType.AntiVac:
                    umbrella.SetValue(settingsType.ToString().ToLower(), ValueTypeCaster<T>.From(value), RegistryValueKind.DWord);
                    break;
                case SettingsType.DotaLaunchTrigger:
                    umbrella.SetValue(settingsType.ToString().ToLower(), ValueTypeCaster<T>.From(value), RegistryValueKind.DWord);
                    break;
                case SettingsType.ServerIndex:
                    umbrella.SetValue(settingsType.ToString().ToLower(), ValueTypeCaster<T>.From(value), RegistryValueKind.DWord);
                    break;
                case SettingsType.DotaLaunchOptions:
                    umbrella.SetValue(settingsType.ToString().ToLower(), (T)Convert.ChangeType(value, typeof(T)));
                    break;
                case SettingsType.Login:
                    umbrella.SetValue(settingsType.ToString().ToLower(), (T)Convert.ChangeType(value, typeof(T)));
                    break;
                case SettingsType.Password:
                    umbrella.SetValue(settingsType.ToString().ToLower(), (T)Convert.ChangeType(value, typeof(T)));
                    break;
                case SettingsType.Language:
                    umbrella.SetValue(settingsType.ToString().ToLower(), (T)Convert.ChangeType(value, typeof(T)));
                    break;
                case SettingsType.DXVersion:
                    umbrella.SetValue(settingsType.ToString().ToLower(), (T)Convert.ChangeType(value, typeof(T)));
                    break;
                case SettingsType.UrlLangType:
                    umbrella.SetValue(settingsType.ToString().ToLower(), (T)Convert.ChangeType(value, typeof(T)));
                    break;
                default:
                    break;
            }
        }
    }
}
