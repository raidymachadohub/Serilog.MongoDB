using System.Diagnostics;
using System.Globalization;
using Serilog;
using Serilog.Core;
using Serilog.Core.Enrichers;
using Serilog.Events;
using Serilog.Debugging;
using ILogger = Serilog.ILogger;

namespace Medium.Serilog.MongoDB;

public class LoggerClass
{
    private readonly IConfiguration _configuration;
    public LoggerClass(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ExecutLogger(TypeLog typeLog, TypeLevel level, string userNameLog, string message)
    {
        Log.Logger = CreateLogger(typeLog, level);
        switch (level)
        {
            case TypeLevel.Verbose:
                Log.Logger.ForContext(GetLogEventEnrichers(userNameLog)).Verbose(message);
                break;
            case TypeLevel.Debug:
                Log.Logger.ForContext(GetLogEventEnrichers(userNameLog)).Debug(message);
                break;
            case TypeLevel.Information:
                Log.Logger.ForContext(GetLogEventEnrichers(userNameLog)).Information(message);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(level), level, null);
        }
        
        Log.CloseAndFlush();
        
        SelfLog.Enable(msg =>
        {
            Console.WriteLine(msg);
            Debugger.Break();
        });
    }

    private static IEnumerable<ILogEventEnricher> GetLogEventEnrichers(string userNameLog)
    {
        List<ILogEventEnricher> logEventEnrichers = new();

        if (!string.IsNullOrWhiteSpace(userNameLog))
        {
            logEventEnrichers.Add(new PropertyEnricher("userNameLog", userNameLog));
        }

        logEventEnrichers.Add(new PropertyEnricher("DateLog", DateTime.Now.ToString(CultureInfo.InvariantCulture)));

        return logEventEnrichers;
    }

    private ILogger CreateLogger(TypeLog typeLog, TypeLevel level)
    {
        var configuration = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentUserName();

        var tableName = typeLog == TypeLog.Audit ? _configuration.GetSection("MongoDB:MongoDBTableAuditName").Value 
                                                 : _configuration.GetSection("MongoDB:MongoDBTableName").Value;
        var host =
            $"{_configuration.GetSection("MongoDB:Connection").Value}/{_configuration.GetSection("MongoDB:DBName").Value}";

        configuration.WriteTo.MongoDB(
            host,
            tableName ?? string.Empty,
            GetEventLevel(level));

        SelfLog.Enable(msg =>
        {
            Console.WriteLine(msg);
            Debugger.Break();
        });

        return configuration.CreateLogger();
    }

    private static LogEventLevel GetEventLevel(TypeLevel typeLevel) =>
        typeLevel switch
        {
            TypeLevel.Information => LogEventLevel.Information,
            TypeLevel.Debug => LogEventLevel.Debug,
            TypeLevel.Verbose => LogEventLevel.Verbose,
            _ => LogEventLevel.Fatal
        };
}