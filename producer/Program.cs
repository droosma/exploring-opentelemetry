using System.Diagnostics;
using System.Diagnostics.Metrics;

using Producer;

var builder = WebApplication.CreateBuilder(args);
builder.Services.WithOpenTelemetry();

var app = builder.Build();

app.MapGet("/Log", (ILogger<Program> logger) =>
                   {
                       logger.LogInformation("Hello World!");
                       return "Hello Log!";
                   });
app.MapGet("/Trace", async (ActivitySource activitySource) =>
                     {
                         using var activity = activitySource.StartActivity("random-duration");
                         
                         await Task.Delay(new Random().Next(1, 1000));

                         return "Hello Trace!";
                     });
app.MapGet("/Metric", (Meter meter) =>
                      {
                          var randomNumberMetric = meter.CreateHistogram<int>("random-number");
                          randomNumberMetric.Record(new Random().Next(1, 100));

                          return "Hello Metric!";
                      });

app.Run();

partial class Program
{
}