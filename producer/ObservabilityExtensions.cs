using System.Diagnostics;
using System.Diagnostics.Metrics;

using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Producer;

public static class ObservabilityExtensions
{
    private const string serviceName = "producer";
    private const string serviceVersion = "v1.0.0";

    private static ResourceBuilder ResourceBuilder(string name, string version)
        => OpenTelemetry.Resources.ResourceBuilder.CreateDefault()
                        .AddService(serviceName:name,
                                    serviceVersion:version);

    public static IServiceCollection WithOpenTelemetry(this IServiceCollection services)
    {
        var resourceBuilder = ResourceBuilder(serviceName, serviceVersion);

        services.AddSingleton(new Meter(serviceName, serviceVersion))
                .AddSingleton(new ActivitySource(serviceName, serviceVersion));

        services.AddLogging(builder => builder.WithOpenTelemetry(resourceBuilder));
        services.AddOpenTelemetry()
                .WithTracing(builder => builder.ConfigureTracing(resourceBuilder))
                .WithMetrics(builder => builder.ConfigureMetrics(resourceBuilder));

        return services;
    }

    private static TracerProviderBuilder ConfigureTracing(this TracerProviderBuilder builder,
                                                          ResourceBuilder resourceBuilder)
        => builder.AddOtlpExporter()
                  .AddSource(serviceName)
                  .SetResourceBuilder(resourceBuilder)
                  .AddAspNetCoreInstrumentation()
                  .AddHttpClientInstrumentation();

    private static MeterProviderBuilder ConfigureMetrics(this MeterProviderBuilder builder,
                                                         ResourceBuilder resourceBuilder)
        => builder.AddOtlpExporter()
                  .AddMeter(serviceName)
                  .SetResourceBuilder(resourceBuilder)
                  .AddAspNetCoreInstrumentation()
                  .AddHttpClientInstrumentation();

    private static ILoggingBuilder WithOpenTelemetry(this ILoggingBuilder builder,
                                                     ResourceBuilder resourceBuilder)
        => builder.AddOpenTelemetry(options =>
                                    {
                                        options.IncludeScopes = true;
                                        options.ParseStateValues = true;
                                        options.SetResourceBuilder(resourceBuilder)
                                               .AddOtlpExporter();
                                    });
}