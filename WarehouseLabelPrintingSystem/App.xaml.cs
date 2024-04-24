using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;
using System.Windows;

namespace WarehouseLabelPrintingSystem
{
    public partial class App : Application
    {
        // Factory for creating loggers throughout the application.
        public static ILoggerFactory? LoggerFactory { get; private set; }

        // Logger for the main application class.
        public static ILogger<App>? AppLogger { get; private set; }

        /// <summary>
        /// Called when the application starts. Sets up logging and performs initializations.
        /// </summary>
        /// <param name="e">Startup event arguments.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Ensure the log directory exists
            var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
                Console.WriteLine("Log directory created at: " + logDirectory);
            }

            // Configure Serilog to write to files with daily rolling
            var serilogLogger = new Serilog.LoggerConfiguration()
                .MinimumLevel.Information() // Set the minimum log level to Information
                .WriteTo.File(
                    Path.Combine(logDirectory, "log-.txt"),
                    rollingInterval: Serilog.RollingInterval.Day, // Rotate logs daily
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: 10_485_760, // Limit file size to 10 MB
                    retainedFileCountLimit: 7) // Keep the last 7 files
                .CreateLogger();

            // Initialize LoggerFactory and add Serilog to it
            LoggerFactory = new LoggerFactory();
            LoggerFactory.AddSerilog(serilogLogger);

            // Create the main logger for the application
            AppLogger = LoggerFactory.CreateLogger<App>();

            // Log the application startup
            AppLogger.LogInformation("Application started successfully.");
        }
    }
}
