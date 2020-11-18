using System;
using System.Collections.Generic;
using System.Text;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace RouterFilter
{
    public static class LoggerService
    {
        
        static LoggerService()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File($"logs/Log.log", LogEventLevel.Information, rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true)
                .CreateLogger();
        }
        public static void LogInformation(string message)
        {
            Log.Information(message);
        }

        public static void LogError(string message)
        {
            Log.Error(message);
        }


        public static void CloseLogger()
        {
            Log.CloseAndFlush();
        }
    }
}
