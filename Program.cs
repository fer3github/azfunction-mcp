using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Functions.Worker;
using McpAzFunction.Data;
using McpAzFunction.Functions;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        
        // Register WorkerRepository and WorkerTools as singletons
        services.AddSingleton<WorkerRepository>();
        services.AddSingleton<WorkerTools>();
        
        // Register ProjectRepository and ProjectTools as singletons
        services.AddSingleton<ProjectRepository>();
        services.AddSingleton<ProjectTools>();
    })
    .Build();

host.Run();
