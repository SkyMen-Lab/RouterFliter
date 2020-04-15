using System;
using System.Collections.Generic;
using System.Text;
using Serilog;
using Serilog.Sinks.File;
using Serilog.Sinks.SystemConsole;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace TheP0ngServer
{
    public class LoggerService
    {
        
        public LoggerService()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File("logs/log.txt", LogEventLevel.Information, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
                .CreateLogger();
        }
        public void LogInformation(string message)
        {
            Log.Information(message);
        }
        public void LogError(string message)
        {
            Log.Error(message);
        }
        public void CloseLogger()
        {
            Log.CloseAndFlush();
        }
    }
}
