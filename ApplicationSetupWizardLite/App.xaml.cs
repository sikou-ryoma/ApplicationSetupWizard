using Serilog;
using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;

namespace ApplicationSetupWizardLite
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static ILoggerFactory LoggerFactory { get; private set; }
            = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string logDirectory = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "ApplicationSetupWizardLite",
                "Logs");

            Directory.CreateDirectory(logDirectory);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(
                    Path.Combine(logDirectory, "installer-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 14)
                .CreateLogger();

            LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(
                builder =>
                {
                    builder.AddSerilog(Log.Logger, dispose: false);
                });

            Log.Information("インストーラーを起動しました。");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("インストーラーを終了します。");
            LoggerFactory?.Dispose();
            Log.CloseAndFlush();

            base.OnExit(e);
        }
    }
}