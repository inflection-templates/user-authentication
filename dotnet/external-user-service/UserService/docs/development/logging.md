# Logging

## Overview

This service uses Serilog for logging. The logs are written to the console and to a file.

## Configuration

The logging configuration as defined in the `appsettings.json` file is as follows:

```json
    "Logging": {
        "LogLevel": {
            "Default": "Information",
            "Microsoft.AspNetCore": "Information",
            "Microsoft.Hosting.Lifetime": "Information"
        }
    },
    "Serilog": {
        "Using": [
            "Serilog.Sinks.Console",
            "Serilog.Sinks.File",
        ],
        "MinimumLevel": {
            "Default": "Information",
            "Override": {
                "Microsoft": "Warning",
                "System": "Warning",
                "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
            }
        },
        "WriteTo": [
            {
                "Name": "Console",
                "Args": {
                    "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}",
                    "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console"
                }
            },
            {
                "Name": "File",
                "Args": {
                    "path": "bin/logs/log-.txt",
                    "rollingInterval": "Day",
                    "retainedFileCountLimit": 15,
                    "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"
                }
            }
        ],
        "Enrich": [
            "FromLogContext",
            "WithMachineName",
            "WithThreadId",
            "WithExceptionDetails"
        ]
    }
```

## Usage

Most of the time, one will be injecting the `ILogger` interface into the class constructors. The `ILogger` interface is available in the `Microsoft.Extensions.Logging` namespace.
In some cases where this is not possible, one can use the `Log` class from the `Serilog` namespace.
