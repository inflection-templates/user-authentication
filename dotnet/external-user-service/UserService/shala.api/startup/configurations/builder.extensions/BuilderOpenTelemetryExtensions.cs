using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace shala.api.startup.configurations;

public static class BuilderOpenTelemetryExtensions
{

    public static WebApplicationBuilder SetupOpenTelemetry(this WebApplicationBuilder builder)
    {
        const string serviceName = "shala.api";

        var addTelemetry = builder.Configuration.GetValue<bool>("Telemetry:Enabled");
        if (!addTelemetry)
        {
            return builder;
        }
        var addTracing = builder.Configuration.GetValue<bool>("Telemetry:Tracing:Enabled");
        var addMetrics = builder.Configuration.GetValue<bool>("Telemetry:Metrics:Enabled");
        if (!addTracing && !addMetrics)
        {
            return builder;
        }

        var otel = builder.Services.AddOpenTelemetry();

        if (addTracing)
        {
            var zipkinEndpoint = builder.Configuration.GetValue<string>("Telemetry:Tracing:Zipkin:Endpoint");
            if (string.IsNullOrEmpty(zipkinEndpoint))
            {
                zipkinEndpoint = "http://localhost:9411/api/v2/spans";
            }

            otel.WithTracing(tracing =>
            {
                tracing.AddHttpClientInstrumentation();
                tracing.AddAspNetCoreInstrumentation();
                tracing.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName));
                tracing.AddConsoleExporter();
                tracing.AddZipkinExporter(options =>
                {
                    options.Endpoint = new Uri(zipkinEndpoint);
                });
            });
        }

        if (addMetrics)
        {
            otel.WithMetrics(metrics =>
            {
                metrics.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName));
                metrics.AddHttpClientInstrumentation();
                metrics.AddAspNetCoreInstrumentation();
                metrics.AddPrometheusExporter();
            });
        }

        return builder;
    }

    // Add a scraping endpoint for Prometheus
    public static WebApplication MapTelemetryMetricsEndpoint(this WebApplication app)
    {
        var addTelemetry = app.Configuration.GetValue<bool>("Telemetry:Enabled");
        if (!addTelemetry)
        {
            return app;
        }
        var addMetrics = app.Configuration.GetValue<bool>("Telemetry:Metrics:Enabled");
        if (addMetrics)
        {
            //If OpenTelemetry prometheus exporter is added, this will add a scaping '/metrics' endpoint to the api
            // app.MapPrometheusScrapingEndpoint(); //???
            app.UseOpenTelemetryPrometheusScrapingEndpoint();
        }
        return app;
    }

}

/////////////////////////////////////////////////////////////////////////
/* Prometheus Metrics Setup

Prometheus is used for scraping metrics. Below is the configuration for scraping the metrics endpoint from your API.

prometheus.yml (configuration file):
yaml
```
scrape_configs:
  - job_name: 'product_api'
    scrape_interval: 5s
    static_configs:
      - targets: ['localhost:5000']  # Replace with your API's address
```
Start Prometheus using the following Docker command:

```
docker run -d --name=prometheus \
  -p 9090:9090 \
  -v /path/to/prometheus.yml:/etc/prometheus/prometheus.yml \
  prom/prometheus
```
You can now access Prometheus at http://localhost:9090 to see metrics scraped from your API.
*/
/////////////////////////////////////////////////////////////////////////
///
/* Zipkin Tracing Collector Setup

Zipkin is used for distributed tracing. Below is the configuration for setting up Zipkin.

docker-compose.yml (configuration file):
yaml
```
version: '3'
services:
  zipkin:
    image: openzipkin/zipkin
    container_name: zipkin
    ports:
      - 9411:9411
```
Start Zipkin using the following Docker command:

```
docker-compose up
```
You can now access Zipkin at http://localhost:9411 to see traces from your API.
*/
/////////////////////////////////////////////////////////////////////////
