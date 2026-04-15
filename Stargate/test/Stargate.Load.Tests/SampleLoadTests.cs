using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NBomber.Contracts;
using NBomber.Contracts.Stats;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using Newtonsoft.Json;
using Stargate.Api.Controllers;
using System.Diagnostics;
using System.Text;

namespace Stargate.Load.Tests;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHttpClient();

        var options = builder.Configuration.GetSection(nameof(LoadTestOptions)).Get<LoadTestOptions>()
            ?? throw new InvalidOperationException($"Configuration section '{nameof(LoadTestOptions)}' is missing or invalid.");

        var host = builder.Build();

        var httpClientFactory = host.Services.GetRequiredService<IHttpClientFactory>();
        using var httpClient = httpClientFactory.CreateClient();

        var badScenario = CreateScenario(httpClient, options, "AsyncDemo/Bad");
        var betterScenario = CreateScenario(httpClient, options, "AsyncDemo/Better");
        var bestScenario = CreateScenario(httpClient, options, "AsyncDemo/Best");

        NBomberRunner
            .RegisterScenarios(badScenario, betterScenario, bestScenario)
            .WithReportFolder("reports")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Md)
            .Run();

        Console.WriteLine("\nLoad test complete!");

        var reportFile = Directory.GetFiles("reports", "*.html", SearchOption.AllDirectories)
            .OrderByDescending(File.GetCreationTime)
            .FirstOrDefault();

        if (reportFile is not null)
        {
            Process.Start(new ProcessStartInfo(reportFile) { UseShellExecute = true });
        }
    }

    private static ScenarioProps CreateScenario(
        HttpClient httpClient,
        LoadTestOptions options,
        string endpoint)
    {
        var scenarioName = endpoint.Replace("/", "_");

        return Scenario.Create(scenarioName, async context =>
            {
                var request = new AsyncDemoRequest()
                {
                    RequestCount = options.RequestCount,
                };

                var httpRequest = Http.CreateRequest("POST", $"{options.BaseUrl}/{endpoint}")
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(new StringContent(
                        JsonConvert.SerializeObject(request),
                        Encoding.UTF8,
                        "application/json"));

                var response = await Http.Send(httpClient, httpRequest);
                return response;
            })
            .WithoutWarmUp()
            .WithLoadSimulations(
                Simulation.Inject(
                    rate: options.Rate,
                    interval: TimeSpan.FromSeconds(options.IntervalSeconds),
                    during: TimeSpan.FromSeconds(options.DurationSeconds))
            );
    }
}
