using CloudFabric.JobHandler.Processor.Extension;
using CloudFabric.JobHandler.Processor.Model;
using CloudFabric.JobHandler.Processor.Model.Settings;
using CloudFabric.JobHandler.Processor.Repository;
using CloudFabric.JobHandler.Processor.Repository.Interface;
using CloudFabric.JobHandler.Processor.Service;
using CloudFabric.JobHandler.Processor.Service.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CloudFabric.JobHandler.Processor.Tests;

public class Startup
{
    public static IConfiguration GetConfiguration()
    {
        return new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }

    public static void ConfigureServices(IServiceCollection services)
    {
        var configuration = GetConfiguration();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddJobHandler(configuration);
    }
}

