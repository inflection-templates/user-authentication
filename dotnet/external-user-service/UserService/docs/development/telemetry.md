# Open Telemetry

## Overview

Open Telemetry is a set of APIs, libraries, agents, and instrumentation to provide observability to your applications. It is a project under the Cloud Native Computing Foundation (CNCF) and is a merger of OpenTracing and OpenCensus. Open Telemetry provides a single set of APIs and libraries to instrument your code and collect telemetry data from your applications.

In this service, we are using Open Telemetry for `Tracing` and `Metrics`.
Tracing is used to trace the flow of requests through the application and to identify bottlenecks and performance issues.
Metrics are used to collect data about the performance and behavior of the application.

## Configuration

The Open Telemetry configuration as defined in the `appsettings.json` file is as follows:

```json
    "Telemetry": {
        "Enabled": true,
        "Tracing": {
            "Enabled": true,
            "Zipkin": {
                "Endpoint": "http://localhost:9411/api/v2/spans"
            }
        },
        "Metrics": {
            "Enabled": true
        }
    }
```

## Injections

OpenTelemetry is injected into the builder's service collection in the file `BuilderOpenTelemetryExtensions.cs` in the `/startup/configurations/builder.extensions` folder.

### Tracing

For tracing, the `Zipkin` and `Console` exporters are used to export the traces to the collectors.

Also please note that apart from having a Trace span for each request, the children custom spans are also added. This is mainly done in the `services` layer.

The base class for every service (`BaseService`) has a `TraceAsync` method in which `System.Diagnostics.ActivitySource` creates a new `Activity` and wrapps the given `Func` (Actual service's method call) in it.
It is to be noted that this is triggered only when `Telemetry` and `Tracing` has been enabled in `appsettings.json`.

### Metrics

For metrics, the `Prometheus` exporter is used to export the metrics to the Prometheus server. Also please note that an endpoint (`/metrics`) is exposed for Prometheus to scrape the metrics. Configure your Prometheus server to scrape the metrics from this endpoint.
