using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using System.Diagnostics.Metrics;

var builder = WebApplication.CreateBuilder(args);

var openTelemetry = builder.Services.AddOpenTelemetry();

openTelemetry.ConfigureResource(resource => resource.AddService(serviceName: builder.Environment.ApplicationName));

openTelemetry.WithMetrics(metrics => metrics
    .AddMeter(Metrics.Key)
    .AddPrometheusExporter());

builder.Services.AddTransient<MetricsService>();

var app = builder.Build();

app.MapGet("/Metrics1", ([FromServices] MetricsService metrics) => metrics.One.Add(1));
app.MapGet("/Metrics2", ([FromServices] MetricsService metrics) => metrics.Two.Add(2));

app.MapPrometheusScrapingEndpoint();

app.Run();

internal class MetricsService
{
    public MetricsService()
    {
        var meter = new Meter(Metrics.Key);
        One = meter.CreateCounter<int>(Metrics.Counters.One);
        Two = meter.CreateCounter<int>(Metrics.Counters.Two);
    }

    public Counter<int> One { get; }

    public Counter<int> Two { get; }
}

internal static class Metrics
{
    public const string Key = "Metrics";

    internal static class Counters
    {
        public const string One = "metrics.1";
        public const string Two = "metrics.2";
    }
}