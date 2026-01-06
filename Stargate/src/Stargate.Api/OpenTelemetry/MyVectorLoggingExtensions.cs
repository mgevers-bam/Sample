using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using Serilog.Sinks.OpenSearch;
using Serilog.Sinks.OpenTelemetry;
using System.Data;
using System.Reflection;

namespace Stargate.Api.OpenTelemetry;

public static class MyVectorLoggingExtensions
{
    public static IHostApplicationBuilder UseMyVectorLogging(this IHostApplicationBuilder builder, LoggingOptions options)
    {
        var serviceName = builder.Configuration["OTEL:SERVICE:NAME"]
            ?? Assembly.GetEntryAssembly()!.GetName().Name!.Replace("MyVector.", string.Empty);

        var loggerConfiguration = GetLoggerConfiguration(builder.Configuration, options, serviceName);

        Log.Logger = loggerConfiguration.CreateLogger();
        AppDomain.CurrentDomain.ProcessExit += (s, e) => Log.CloseAndFlush();
        builder.Services.AddSerilog();

        if (options.LogSink.HasFlag(SerilogSinks.OpenTelemetry))
        {
            builder.ConfigureOpenTelemetry(serviceName);
        }

        Log.Logger.Information("Application Starting, Serilog Initializing: {ApplicationName}", Assembly.GetEntryAssembly()!.FullName);

        return builder;
    }

    private static LoggerConfiguration GetLoggerConfiguration(
        IConfiguration configuration,
        LoggingOptions options,
        string serviceName)
    {
        var loggerConfiguration = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration);

        loggerConfiguration
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("SourceName", Assembly.GetEntryAssembly()?.GetName().Name);

        if (options.LogSink.HasFlag(SerilogSinks.OpenSearch))
        {
            loggerConfiguration
                .WriteTo
                .OpenSearch(new OpenSearchSinkOptions(new Uri(options.LoggingConnection))
                {
                    IndexFormat = options.LogName + "-{0:yyyy.MM.dd}",
                    IndexAliases = [options.LogName],
                    EmitEventFailure = EmitEventFailureHandling.ThrowException,
                });
        }
        if (options.LogSink.HasFlag(SerilogSinks.Sql))
        {
            loggerConfiguration
                .WriteTo
                .MSSqlServer(connectionString: options.LoggingConnection,
                    sinkOptions: new MSSqlServerSinkOptions
                    {
                        AutoCreateSqlDatabase = false,
                        AutoCreateSqlTable = true,
                        BatchPostingLimit = 150,
                        SchemaName = "dbo",
                        TableName = options.LogName
                    },
                    restrictedToMinimumLevel: LogEventLevel.Verbose,
                    columnOptions: GetColumnOptions()
                );
        }
        if (options.LogSink.HasFlag(SerilogSinks.Console))
        {
            loggerConfiguration.WriteTo.Console();
        }
        if (options.LogSink.HasFlag(SerilogSinks.OpenTelemetry))
        {
            loggerConfiguration.WriteTo.OpenTelemetry(options =>
            {
                options.Endpoint = "http://otel-collector:4317";
                options.Protocol = OtlpProtocol.Grpc;
                options.ResourceAttributes = new Dictionary<string, object>
                {
                    ["service.name"] = serviceName,
                    ["service.instance.id"] = Environment.MachineName
                };
            });
        }

        return loggerConfiguration;
    }

    private static ColumnOptions GetColumnOptions()
    {
        var columnOpts = new ColumnOptions();
        columnOpts.Store.Remove(StandardColumn.Properties);
        columnOpts.AdditionalColumns = new[] {
            new SqlColumn
            {
                AllowNull = true,
                ColumnName = "RequestPath",
                DataType = SqlDbType.VarChar,
                DataLength = 5000
            },
            new SqlColumn
            {
                AllowNull = true,
                ColumnName = "FullUrl",
                DataType = SqlDbType.VarChar,
                DataLength = 5000
            },
            new SqlColumn
            {
                AllowNull = true,
                ColumnName = "Verb",
                DataType = SqlDbType.VarChar,
                DataLength = 50
            },
            new SqlColumn
            {
                AllowNull = true,
                ColumnName = "ResponseCode",
                DataType = SqlDbType.VarChar,
                DataLength = 3
            },
            new SqlColumn
            {
                AllowNull = true,
                ColumnName = "ElapsedMilliseconds",
                DataType = SqlDbType.Decimal
            },
            new SqlColumn
            {
                AllowNull = true,
                ColumnName = "SessionId",
                DataType = SqlDbType.UniqueIdentifier
            },
            new SqlColumn
            {
                AllowNull = true,
                ColumnName = "PersonId",
                DataType = SqlDbType.UniqueIdentifier
            },
            new SqlColumn
            {
                AllowNull = true,
                ColumnName = "CorrelationId",
                DataType = SqlDbType.UniqueIdentifier
            },
            new SqlColumn
            {
                AllowNull = true,
                ColumnName = "MachineName",
                DataType = SqlDbType.VarChar,
                DataLength = 128
            },
            new SqlColumn
            {
                AllowNull = true,
                ColumnName = "SourceName",
                DataType = SqlDbType.VarChar,
                DataLength = 128
            },
            new SqlColumn
            {
                AllowNull = true,
                ColumnName = "ClientId",
                DataType = SqlDbType.VarChar,
                DataLength = 128
            }
        };
        columnOpts.Store.Add(StandardColumn.LogEvent);
        columnOpts.PrimaryKey = columnOpts.Id;
        columnOpts.TimeStamp.NonClusteredIndex = true;
        columnOpts.TimeStamp.ConvertToUtc = true;
        return columnOpts;
    }
}
