using CloudFabric.JobHandler.Processor.Model;
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
    public IConfigurationRoot GetIConfigurationRoot()
    {
        return new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }

    public IConfigurationRoot Configuration => GetIConfigurationRoot();

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IConfiguration>(GetIConfigurationRoot());
        services.AddTransient<IEditableRepository<Job>, EditableRepositoryPostgres<Job>>();
        services.AddScoped<IJobService, JobService>();
    }
}

