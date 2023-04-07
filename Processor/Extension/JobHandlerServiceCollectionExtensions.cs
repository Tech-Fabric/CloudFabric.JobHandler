using System;
using CloudFabric.JobHandler.Processor.Model;
using CloudFabric.JobHandler.Processor.Model.Settings;
using CloudFabric.JobHandler.Processor.Repository;
using CloudFabric.JobHandler.Processor.Repository.Interface;
using CloudFabric.JobHandler.Processor.Service;
using CloudFabric.JobHandler.Processor.Service.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CloudFabric.JobHandler.Processor.Extension;

public static class JobHandlerServiceCollectionExtensions
{
    public static IServiceCollection AddJobHandler(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IEditableRepository<Job>, EditableRepositoryPostgres<Job>>();
        services.AddTransient<IEditableRepository<JobProcess>, EditableRepositoryPostgres<JobProcess>>();
        services.AddTransient<IEditableRepository<JobCompleted>, EditableRepositoryPostgres<JobCompleted>>();
        services.AddTransient<IReadableRepository<JobType>, ReadableRepositoryPostgres<JobType>>();
        services.AddTransient<IReadableRepository<JobStatus>, ReadableRepositoryPostgres<JobStatus>>();
        services.AddScoped<IJobService, JobService>();
        services.AddOptions<JobHandlerSettings>().Bind(configuration.GetSection(JobHandlerSettings.Position));

        return services;
    }
}