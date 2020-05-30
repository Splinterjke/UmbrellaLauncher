using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using Serilog;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using UmbrellaProject.Models;
using UmbrellaProject.Services;

namespace UmbrellaProject.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    internal class DotaScriptsViewModel : Screen
    {
        private FileSystemWatcher fileWatcher;
        private string scriptDir = $@"{AppDomain.CurrentDomain.BaseDirectory}scripts";
        public DotaViewModel DotaViewModel { get; set; }
        private readonly IHttpService httpService;
        private readonly ISettingsService settingsService;
        private bool isScriptsAutoUpdated = false;
        PrivateScriptModel selectedScript;

        public bool IsAddScriptDialogOpen { get; set; }
        public bool IsOpenUrlDialogOpen { get; set; }
        public string LuaPath { get; set; }
        public string ScriptConflictName { get; set; }
        public bool IsUpdating { get; set; }
        public bool IsLanguageRemembered
        {
            get
            {
                var val = settingsService.ReadValue<int>(SettingsType.IsUrlLanguageRemembered);
                return val == 1 ? true : false;
            }
            set
            {
                var val = value == true ? 1 : 0;
                settingsService.SaveValue(SettingsType.IsUrlLanguageRemembered, val);
            }
        }
        private ObservableCollection<ScriptConfigModel> scriptsData;
        public ObservableCollection<ScriptConfigModel> ScriptsData
        {
            get
            {
                return scriptsData;
            }
            set
            {
                if (value == null) return;
                scriptsData = new ObservableCollection<ScriptConfigModel>(value.OrderBy(x => x.ScriptName));
            }
        }

        private ObservableCollection<PrivateScriptModel> privateScriptsData;
        public ObservableCollection<PrivateScriptModel> PrivateScriptsData
        {
            get
            {
                return privateScriptsData;
            }
            set
            {
                if (value == null) return;
                privateScriptsData = new ObservableCollection<PrivateScriptModel>(value.OrderBy(x => x.ScriptName));
            }
        }

        public DotaScriptsViewModel(IHttpService httpService, ISettingsService settingsService)
        {
            ScriptsData = new ObservableCollection<ScriptConfigModel>();
            PrivateScriptsData = new ObservableCollection<PrivateScriptModel>();
            this.httpService = httpService;
            this.settingsService = settingsService;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            LoadScripts();
            //((this.View as Window).FindName("PrivateScriptsListBox") as Expander).Expanded += DotaScriptsViewModel_Expanded;
        }

        public void ScriptListExpanded(object sender, RoutedEventArgs e)
        {
            switch ((sender as Expander).Name)
            {
                case "ScriptsListExpander":
                    ((this.View as UserControl).FindName("PrivateScriptsListExpander") as Expander).IsExpanded = false;
                    break;
                case "PrivateScriptsListExpander":
                    ((this.View as UserControl).FindName("ScriptsListExpander") as Expander).IsExpanded = false;
                    break;
            }
        }

        protected override async void OnInitialActivate()
        {
            base.OnInitialActivate();
            DotaViewModel = this.Parent as DotaViewModel;
            fileWatcher = new FileSystemWatcher
            {
                Path = scriptDir,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                Filter = "*.*",
            };
            fileWatcher.Renamed += FileWatcherHandle;
            fileWatcher.Created += FileWatcherHandle;
            fileWatcher.Deleted += FileWatcherHandle;
            fileWatcher.Changed += FileWatcherHandle;
            fileWatcher.EnableRaisingEvents = true;
            if (settingsService.ReadValue<int>(SettingsType.DotaScriptsAutoUpdate) == 1 && !isScriptsAutoUpdated)
            {
                await UpdateAllScripts();
                isScriptsAutoUpdated = true;
            }
            //CheckForScriptsUpdate();
        }

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            ((this.View as UserControl).FindName("PrivateScriptsListExpander") as Expander).IsExpanded = true;
        }

        private void FileWatcherHandle(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (!Regex.IsMatch(Path.GetExtension(e.FullPath), @"\.lua|\.bak", RegexOptions.IgnoreCase))
                {
                    return;
                }
                if (e.ChangeType == WatcherChangeTypes.Changed)
                {
                    var scriptConfig = ScriptsData.FirstOrDefault(x => x.ScriptLocalPath == e.FullPath.Replace(MainViewModel.AssemblyPath, "") && x.ScriptName == Path.GetFileNameWithoutExtension(e.Name));
                    if (scriptConfig != null)
                    {
                        scriptConfig.ScriptPath = null;
                        scriptConfig.RepositoryPath = null;
                        UpdateScriptConfig();
                        return;
                    }
                }
                if (e.ChangeType == WatcherChangeTypes.Renamed)
                {
                    var args = e as RenamedEventArgs;
                    var isEnabled = e.FullPath.EndsWith(".bak") ? false : true;
                    var scriptConfig = ScriptsData.FirstOrDefault(x => x.ScriptLocalPath == args.OldFullPath.Replace(MainViewModel.AssemblyPath, "") && x.ScriptName == Path.GetFileNameWithoutExtension(args.OldName));
                    if (scriptConfig != null)
                    {
                        scriptConfig.IsEnabled = isEnabled;
                        scriptConfig.ScriptLocalPath = args.FullPath.Replace(MainViewModel.AssemblyPath, "");
                        scriptConfig.ScriptName = Path.GetFileNameWithoutExtension(args.Name);
                        UpdateScriptConfig();
                    }
                    else
                    {
                        var newScriptConfig = new ScriptConfigModel
                        {
                            IsEnabled = isEnabled,
                            ScriptLocalPath = e.FullPath.Replace(MainViewModel.AssemblyPath, ""),
                            ScriptName = Path.GetFileNameWithoutExtension(e.Name)
                        };
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            ScriptsData.Add(newScriptConfig);
                        });
                        UpdateScriptConfig();
                    }
                    return;
                }
                if (e.ChangeType == WatcherChangeTypes.Created)
                {
                    var isEnabled = e.FullPath.EndsWith(".bak") ? false : true;
                    if (File.Exists(e.FullPath))
                    {
                        var scriptConfig = ScriptsData.FirstOrDefault(x => x.ScriptLocalPath == e.FullPath.Replace(MainViewModel.AssemblyPath, "") && x.ScriptName == Path.GetFileNameWithoutExtension(e.Name));
                        if (scriptConfig != null)
                        {
                            scriptConfig.ScriptPath = null;
                            scriptConfig.RepositoryPath = null;
                            UpdateScriptConfig();
                            return;
                        }
                    }
                    var newScriptConfig = new ScriptConfigModel
                    {
                        IsEnabled = isEnabled,
                        ScriptLocalPath = e.FullPath.Replace(MainViewModel.AssemblyPath, ""),
                        ScriptName = Path.GetFileNameWithoutExtension(e.Name)
                    };
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        ScriptsData.Add(newScriptConfig);
                    });
                    UpdateScriptConfig();
                    return;
                }
                if (e.ChangeType == WatcherChangeTypes.Deleted)
                {
                    var scriptConfig = ScriptsData.FirstOrDefault(x => x.ScriptLocalPath == e.FullPath.Replace(MainViewModel.AssemblyPath, "") && x.ScriptName == Path.GetFileNameWithoutExtension(e.Name));
                    if (scriptConfig != null)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            ScriptsData.Remove(scriptConfig);
                        });
                        UpdateScriptConfig();
                    }
                }
            }
            catch (Exception ex)
            {
                DotaViewModel.GameBrowserVm.MainViewModel.ShowError(ex.Message);
                Log.Error(ex, "null");
            }
        }

        protected override void OnDeactivate()
        {
            fileWatcher.Changed -= FileWatcherHandle;
            fileWatcher.Created -= FileWatcherHandle;
            fileWatcher.Deleted -= FileWatcherHandle;
            fileWatcher.EnableRaisingEvents = false;
            base.OnDeactivate();
        }

        public void UpdatePrivateScriptConfig()
        {
            var json = JsonConvert.SerializeObject(PrivateScriptsData, Formatting.Indented);
            File.WriteAllText("privatescripts.json", json);
        }

        public void UpdateScriptConfig(ScriptConfigModel scriptConfigModel = null)
        {
            try
            {
                if (scriptConfigModel != null)
                {
                    if (fileWatcher != null)
                        fileWatcher.EnableRaisingEvents = false;
                    if (!scriptConfigModel.IsEnabled)
                    {
                        if (File.Exists(MainViewModel.AssemblyPath + scriptConfigModel.ScriptLocalPath))
                        {
                            File.Move(MainViewModel.AssemblyPath + scriptConfigModel.ScriptLocalPath, MainViewModel.AssemblyPath + scriptConfigModel.ScriptLocalPath.Replace(".lua", ".bak"));
                            scriptConfigModel.ScriptLocalPath = scriptConfigModel.ScriptLocalPath.Replace(".lua", ".bak").Replace(MainViewModel.AssemblyPath, "");
                        }
                    }
                    else
                    {
                        if (File.Exists(MainViewModel.AssemblyPath + scriptConfigModel.ScriptLocalPath))
                        {
                            File.Move(MainViewModel.AssemblyPath + scriptConfigModel.ScriptLocalPath, MainViewModel.AssemblyPath + scriptConfigModel.ScriptLocalPath.Replace(".bak", ".lua"));
                            scriptConfigModel.ScriptLocalPath = scriptConfigModel.ScriptLocalPath.Replace(".bak", ".lua").Replace(MainViewModel.AssemblyPath, "");
                        }
                    }
                }
                var json = JsonConvert.SerializeObject(ScriptsData, Formatting.Indented);
                File.WriteAllText("scripts.json", json);
            }
            catch (Exception ex)
            {
                DotaViewModel.GameBrowserVm.MainViewModel.ShowError(ex.Message);
                Log.Error(ex, "null");
            }
            finally
            {
                if (fileWatcher != null)
                    fileWatcher.EnableRaisingEvents = true;
                ScriptsData = new ObservableCollection<ScriptConfigModel>(ScriptsData);
            }
        }

        public async Task UpdateAllScripts()
        {
            if (ScriptsData.Count == 0) return;
            IsUpdating = true;
            try
            {
                var updateRepos = new List<string>();
                var scripts = ScriptsData.ToList();
                foreach (var script in scripts)
                {
                    if (!string.IsNullOrEmpty(script.RepositoryPath))
                    {
                        if (updateRepos.Contains(script.RepositoryPath))
                            continue;
                        LuaPath = script.RepositoryPath;
                        await DownLoadSelectedPath();
                        updateRepos.Add(LuaPath);
                        continue;
                    }
                    if (!string.IsNullOrEmpty(script.ScriptPath))
                    {
                        LuaPath = script.ScriptPath;
                        await DownLoadSelectedPath();
                    }
                }
                UpdateScriptConfig();
            }

            catch (Exception ex)
            {
                DotaViewModel.GameBrowserVm.MainViewModel.ShowError(ex.Message);
                Log.Error(ex, "null");
            }
            finally
            {
                LuaPath = string.Empty;
                IsUpdating = false;
            }
        }

        //public async void UpdateAllScripts(ScriptConfigModel selectedScript)
        //{
        //    await DownLoadScript(selectedScript.ScriptPath);
        //    selectedScript.LastCommitTime = selectedScript.PreUpdateTime.Value;
        //    selectedScript.IsUpdateAvailable = false;
        //    selectedScript.PreUpdateTime = null;
        //    UpdateScriptConfig();
        //}

        public void DeleteScript(ScriptConfigModel selectedScript)
        {
            if (File.Exists(MainViewModel.AssemblyPath + selectedScript.ScriptLocalPath))
                File.Delete(MainViewModel.AssemblyPath + selectedScript.ScriptLocalPath);
            ScriptsData.Remove(selectedScript);
            UpdateScriptConfig();
        }

        public void OpenForumThread(PrivateScriptModel selectedScript)
        {
            if (string.IsNullOrEmpty(selectedScript.ForumUrl) && !string.IsNullOrEmpty(selectedScript.ForumUrlEN))
            {
                Process.Start(selectedScript.ForumUrlEN);
                return;
            }

            if (!string.IsNullOrEmpty(selectedScript.ForumUrl) && string.IsNullOrEmpty(selectedScript.ForumUrlEN))
            {
                Process.Start(selectedScript.ForumUrl);
                return;
            }
            var langType = settingsService.ReadValue<int>(SettingsType.UrlLangType);
            if (this.IsLanguageRemembered && langType != 0)
            {
                string url = langType == 1 ? selectedScript.ForumUrlEN : selectedScript.ForumUrl;
                Process.Start(url);
                return;
            }
            this.selectedScript = selectedScript;
            this.IsOpenUrlDialogOpen = true;
        }

        public void DeleteAllScripts()
        {
            try
            {
                fileWatcher.EnableRaisingEvents = false;
                Directory.Delete(scriptDir, true);
                ScriptsData.Clear();
                if (!Directory.Exists(scriptDir))
                    Directory.CreateDirectory(scriptDir);
                UpdateScriptConfig();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "null");
            }
            finally
            {
                fileWatcher.EnableRaisingEvents = true;
            }
        }

        private async void LoadScripts()
        {
            try
            {
                if (!Directory.Exists(scriptDir))
                {
                    Directory.CreateDirectory(scriptDir);
                }
                if (!File.Exists("scripts.json"))
                {
                    var scriptsFile = File.Create("scripts.json");
                    scriptsFile.Close();
                }
                string jsonString = string.Empty;
                using (var sr = new StreamReader("scripts.json"))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        jsonString += line;
                    }
                }
                HashSet<string> extensions = new HashSet<string> { ".lua", ".bak" };
                if (!string.IsNullOrEmpty(jsonString))
                {
                    var tempData = JsonConvert.DeserializeObject<IEnumerable<ScriptConfigModel>>(jsonString);
                    ScriptsData = new ObservableCollection<ScriptConfigModel>(tempData.Where(x => File.Exists(MainViewModel.AssemblyPath + x.ScriptLocalPath)));
                    foreach (var scriptData in ScriptsData)
                    {
                        if (scriptData.ScriptLocalPath.Contains(MainViewModel.AssemblyPath))
                            scriptData.ScriptLocalPath = scriptData.ScriptLocalPath.Replace(MainViewModel.AssemblyPath, "");
                    }
                    foreach (var scriptPath in Directory.EnumerateFiles(scriptDir, "*.*").Where(x => extensions.Contains(Path.GetExtension(x))))
                    {
                        var fileNameWOExt = Path.GetFileNameWithoutExtension(scriptPath);
                        if (ScriptsData.FirstOrDefault(x => x.ScriptName == fileNameWOExt) != null) continue;
                        var isEnabled = scriptPath.EndsWith(".bak") ? false : true;
                        var newScriptData = new ScriptConfigModel
                        {
                            IsEnabled = isEnabled,
                            ScriptName = fileNameWOExt,
                            ScriptLocalPath = isEnabled == true ? $@"{scriptDir}\{fileNameWOExt}.lua".Replace(MainViewModel.AssemblyPath, "") : $@"{scriptDir}\{fileNameWOExt}.bak".Replace(MainViewModel.AssemblyPath, "")
                        };
                        ScriptsData.Add(newScriptData);
                    }
                }
                else
                {
                    ScriptsData = new ObservableCollection<ScriptConfigModel>();
                    foreach (var scriptPath in Directory.EnumerateFiles(scriptDir, "*.*").Where(x => extensions.Contains(Path.GetExtension(x))))
                    {
                        var isEnabled = scriptPath.EndsWith(".bak") ? false : true;
                        var fileNameWOext = Path.GetFileNameWithoutExtension(scriptPath);
                        var newScriptData = new ScriptConfigModel
                        {
                            IsEnabled = isEnabled,
                            ScriptName = fileNameWOext,
                            ScriptLocalPath = isEnabled == true ? $@"{scriptDir}\{fileNameWOext}.lua".Replace(MainViewModel.AssemblyPath, "") : $@"{scriptDir}\{fileNameWOext}.lua.bak".Replace(MainViewModel.AssemblyPath, "")
                        };
                        ScriptsData.Add(newScriptData);
                    }
                }

                UpdateScriptConfig();
            }
            catch (Exception ex)
            {
                DotaViewModel.GameBrowserVm.MainViewModel.ShowError(ex.Message);
                Log.Error(ex, "null");
            }
        }

        //private async void CheckForScriptsUpdate()
        //{
        //    if (!httpService.HttpClient.DefaultRequestHeaders.Contains("Authorization"))
        //        httpService.HttpClient.DefaultRequestHeaders.Add("Authorization", "token 7325c18b19a6d8cd707123e3703204b22de7b61d");
        //    try
        //    {
        //        foreach (var scriptConfig in ScriptsData.Where(x => !string.IsNullOrEmpty(x.ScriptPath)))
        //        {

        //            var githubLink = scriptConfig.ScriptPath.StartsWith("https") ? scriptConfig.ScriptPath.Replace("https://github.com/", null).Split(new char[] { '/' }, 3) : scriptConfig.ScriptPath.Replace("http://github.com/", null).Split(new char[] { '/' }, 3);
        //            using (var commitsResponse = await httpService.HttpClient.GetAsync($"https://api.github.com/repos/{githubLink[0]}/{githubLink[1]}/commits"))
        //            {
        //                var json = await commitsResponse.Content.ReadAsStringAsync();
        //                var attemptsCount = commitsResponse.Headers.GetValues("X-RateLimit-Remaining")?.ToArray()[0];
        //                if (attemptsCount != null && attemptsCount == "0" && commitsResponse.Headers.TryGetValues("X-RateLimit-Reset", out var values))
        //                {
        //                    var now = DateTime.UtcNow;
        //                    var resetAt = UnixTimeStampToDateTime(Convert.ToDouble(values.ToArray()[0]));
        //                    var delayTime = resetAt - now;
        //                    await Task.Delay(delayTime);
        //                }
        //                dynamic commits = JArray.Parse(json);
        //                foreach (var commit in commits)
        //                {
        //                    using (var commitResponse = await httpService.HttpClient.GetAsync($"https://api.github.com/repos/{githubLink[0]}/{githubLink[1]}/commits/{commit.sha}"))
        //                    {
        //                        json = await commitResponse.Content.ReadAsStringAsync();
        //                        var commitFile = JObject.Parse(json)["files"].FirstOrDefault(x => x["filename"].ToString() == $"{scriptConfig.ScriptName}.lua");

        //                        if (commitFile != null)
        //                        {
        //                            var date = commitFile.Root["commit"]["author"]["date"].ToObject<DateTimeOffset>();
        //                            if (date > scriptConfig.LastCommitTime)
        //                            {
        //                                scriptConfig.IsUpdateAvailable = true;
        //                                scriptConfig.PreUpdateTime = date;
        //                                break;
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    finally
        //    {
        //        UpdateScriptConfig();
        //    }
        //}

        public void ShowAddScriptDialog()
        {
            LuaPath = string.Empty;
            IsAddScriptDialogOpen = true;
        }

        public async void OnCloseAddScriptDialog(DialogClosingEventArgs e)
        {
            if (e.Parameter == null || !(bool)e.Parameter) return;
            await DownLoadSelectedPath();
        }

        public void OnCloseOpenUrlDialog(DialogClosingEventArgs e)
        {
            if (e.Parameter != null)
            {
                if ((int)e.Parameter == 1)
                    Process.Start(selectedScript.ForumUrlEN);
                if ((int)e.Parameter == 2)
                    Process.Start(selectedScript.ForumUrl);
                if ((int)e.Parameter == 0)
                    return;
                if (IsLanguageRemembered)
                {
                    var val = (int)e.Parameter;
                    settingsService.SaveValue(SettingsType.UrlLangType, val);
                }
            }
        }

        private async Task DownLoadSelectedPath()
        {
            IsUpdating = true;
            try
            {
                if (!string.IsNullOrEmpty(LuaPath))
                {
                    if (RegExpressions.LuaRawPathRegex.Value.IsMatch(LuaPath) || RegExpressions.LuaPathRegex.Value.IsMatch(LuaPath))
                    {
                        var fileName = Path.GetFileNameWithoutExtension(LuaPath);
                        var scriptConfig = ScriptsData.FirstOrDefault(x => x.ScriptName == fileName);
                        //if (scriptConfig != null && (File.Exists($@"scripts\{fileName}.lua") || File.Exists($@"scripts\{fileName}.bak")))
                        //{
                        //    scriptConfig.ScriptPath = LuaPath;
                        //    UpdateScriptConfig();
                        //    return;
                        //}
                        if ((LuaPath.StartsWith("http://github") || LuaPath.StartsWith("https://github")) && LuaPath.Contains("blob"))
                        {
                            LuaPath = LuaPath.Replace("http://github", "https://raw.githubusercontent");
                            LuaPath = LuaPath.Replace("https://github", "http://raw.githubusercontent");
                            LuaPath = LuaPath.Replace("blob/", "");
                        }
                        LuaPath = LuaPath.Replace("https", "http");
                        fileWatcher.EnableRaisingEvents = false;
                        var isEnabled = true;
                        if (scriptConfig != null && !scriptConfig.IsEnabled)
                            isEnabled = false;
                        try
                        {
                            await DownLoadScript(LuaPath, isEnabled);
                        }
                        catch (WebException)
                        {
                            //if (scriptConfig != null)
                            //{
                            //    ScriptsData.Remove(scriptConfig);
                            //    if (File.Exists(scriptConfig.ScriptLocalPath))
                            //    {
                            //        File.Delete(scriptConfig.ScriptLocalPath);
                            //    }
                            //}
                        }
                        UpdateScriptConfig();
                        fileWatcher.EnableRaisingEvents = true;
                    }
                    else
                    {
                        if (LuaPath.Contains("tree/master"))
                        {
                            DotaViewModel.GameBrowserVm.MainViewModel.ShowError(Properties.Resources.IncorrectLuaPath);
                            return;
                        }
                        if (LuaPath.StartsWith("http://github") || LuaPath.StartsWith("https://github"))
                        {
                            LuaPath = LuaPath.Replace("https", "http");
                            fileWatcher.EnableRaisingEvents = false;
                            var scriptConfig = ScriptsData.FirstOrDefault(x => x.RepositoryPath == LuaPath);
                            try
                            {
                                await DownLoadRepository(LuaPath);
                            }
                            catch (WebException)
                            {
                                if (scriptConfig != null)
                                {
                                    ScriptsData.Remove(scriptConfig);
                                    if (File.Exists(MainViewModel.AssemblyPath + scriptConfig.ScriptLocalPath))
                                    {
                                        File.Delete(MainViewModel.AssemblyPath + scriptConfig.ScriptLocalPath);
                                    }
                                }
                            }
                            var zipPath = $@"{scriptDir}\master.zip";
                            if (File.Exists(zipPath))
                                File.Delete(zipPath);
                            UpdateScriptConfig();
                            fileWatcher.EnableRaisingEvents = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DotaViewModel.GameBrowserVm.MainViewModel.ShowError(ex.Message);
                Log.Error(ex, "null");
            }
            finally
            {
                IsUpdating = false;
                fileWatcher.EnableRaisingEvents = true;
            }
        }

        private async Task DownLoadRepository(string repPath)
        {
            repPath = $"{repPath}/archive/master.zip";
            var request = WebRequest.CreateHttp(repPath);
            request.Method = "GET";
            try
            {
                using (var response = await request.GetResponseAsync())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        using (var fileToDownload = new FileStream($@"{scriptDir}\{Path.GetFileName(repPath)}", FileMode.Create, FileAccess.ReadWrite))
                        {
                            await responseStream.CopyToAsync(fileToDownload);
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                throw ex;
            }
            var zipPath = $@"{scriptDir}\{Path.GetFileName(repPath)}";
            if (File.Exists(zipPath))
            {
                var directory = $"{repPath.Replace("http://github.com/", "").Split('/')[1]}-master";
                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    var result = from currEntry in archive.Entries
                                 where Path.GetDirectoryName(currEntry.FullName).Contains(directory)
                                 where !String.IsNullOrEmpty(currEntry.Name)
                                 select currEntry;


                    foreach (ZipArchiveEntry entry in result)
                    {
                        var outPath = $@"{scriptDir}\{entry.FullName.Replace($"{directory}/", "")}";
                        if (outPath.EndsWith(".lua") && File.Exists(outPath.Replace(".lua", ".bak")))
                        {
                            outPath = outPath.Replace(".lua", ".bak");
                        }
                        var localScriptVersion = ScriptsData.FirstOrDefault(x => x.ScriptLocalPath == outPath.Replace(MainViewModel.AssemblyPath, "") && x.ScriptPath == null && x.RepositoryPath == null);
                        var conflictResult = false;
                        if (localScriptVersion != null)
                        {
                            ScriptConflictName = entry.Name;
                            var dialogResult = await DialogHost.Show((this.View as UserControl).FindName("ScriptConflictContent"), "ScriptConflict", delegate (object sender, DialogClosingEventArgs args)
                            {
                                conflictResult = (bool)args.Parameter;
                            });
                        }
                        EnsureDirectoryExists(outPath);
                        entry.ExtractToFile(outPath, true);
                        if (Path.GetDirectoryName(outPath) == scriptDir)
                        {
                            var scriptConfig = ScriptsData.FirstOrDefault(x => x.ScriptName == Path.GetFileNameWithoutExtension(entry.Name));
                            if (scriptConfig != null)
                            {
                                if (conflictResult)
                                {
                                    scriptConfig.RepositoryPath = LuaPath;
                                    scriptConfig.ScriptPath = null;
                                }
                                continue;
                            }

                            var newScriptData = new ScriptConfigModel
                            {
                                IsEnabled = outPath.EndsWith(".bak") ? false : true,
                                ScriptName = Path.GetFileNameWithoutExtension(entry.Name),
                                ScriptLocalPath = outPath.Trim('/').Replace(MainViewModel.AssemblyPath, ""),
                                RepositoryPath = LuaPath
                            };
                            ScriptsData.Add(newScriptData);
                        }
                    }
                }
            }
        }

        private void EnsureDirectoryExists(string filePath)
        {
            FileInfo fi = new FileInfo(filePath);
            if (!fi.Directory.Exists)
            {
                System.IO.Directory.CreateDirectory(fi.DirectoryName);
            }
        }

        private async Task DownLoadScript(string url, bool isEnabled)
        {
            var fileName = Path.GetFileName(url);
            if (!isEnabled)
                fileName.Replace(".lua", ".bak");
            var request = WebRequest.CreateHttp(url);
            request.Method = "GET";
            try
            {
                using (var response = await request.GetResponseAsync())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        if (File.Exists($@"{scriptDir}\{fileName}"))
                        {
                            var scriptConfig = ScriptsData.FirstOrDefault(x => x.ScriptLocalPath == $@"{scriptDir}\{fileName}".Replace(MainViewModel.AssemblyPath, "") && x.ScriptPath == null && x.RepositoryPath == null);
                            var conflictResult = false;
                            if (scriptConfig != null)
                            {
                                ScriptConflictName = fileName;
                                var dialogResult = await DialogHost.Show((this.View as UserControl).FindName("ScriptConflictContent"), "ScriptConflict", delegate (object sender, DialogClosingEventArgs args)
                                {
                                    conflictResult = (bool)args.Parameter;
                                });
                                if (conflictResult)
                                {
                                    scriptConfig.RepositoryPath = null;
                                    scriptConfig.ScriptPath = LuaPath;
                                }
                                else return;
                            }
                        }
                        else
                        {
                            var newScriptData = new ScriptConfigModel
                            {
                                IsEnabled = isEnabled,
                                ScriptName = Path.GetFileNameWithoutExtension(fileName),
                                ScriptLocalPath = $@"{scriptDir}\{fileName}".Replace(MainViewModel.AssemblyPath, ""),
                                ScriptPath = LuaPath
                            };
                            ScriptsData.Add(newScriptData);
                        }
                        using (var fileToDownload = new FileStream($@"{scriptDir}\{fileName}", FileMode.Create, FileAccess.ReadWrite))
                        {
                            await responseStream.CopyToAsync(fileToDownload);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
