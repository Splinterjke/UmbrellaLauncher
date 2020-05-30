using Microsoft.Win32;
using Serilog;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Formatting.Json;
using Stylet;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using UmbrellaProject.Services;
using UmbrellaProject.ViewModels;

namespace UmbrellaProject
{
    internal class AppBootstrapper : Bootstrapper<MainViewModel>
    {
        protected override void OnStart()
        {
            base.OnStart();
            Log.Logger = new LoggerConfiguration()
                .Enrich.WithExceptionDetails()
                .WriteTo.Async(a => a.File(new JsonFormatter(renderMessage: false), "umbrella_logs/umbrella.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7))
                .CreateLogger();
        }

        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            base.ConfigureIoC(builder);
            builder.Bind<ISettingsService>().To<SettingsService>().InSingletonScope();
            builder.Bind<IHttpService>().To<HttpService>().InSingletonScope();
        }

        protected override void Configure()
        {
            // This is called after Stylet has created the IoC container, so this.Container exists, but before the
            // Root ViewModel is launched.
            // Configure your services, etc, in here
        }

        protected override void OnLaunch()
        {
            base.OnLaunch();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.CloseAndFlush();
            base.OnExit(e);
        }

        protected override void OnUnhandledException(DispatcherUnhandledExceptionEventArgs e)
        {
            this.RootViewModel.ShowError(e.Exception.Message);
            Log.Error(e.Exception, "null");
            // Called on Application.DispatcherUnhandledException
        }
    }
}
