using System;
using CloudFabric.JobHandler.Processor.Enum;
using CloudFabric.JobHandler.Processor.Model;
using CloudFabric.JobHandler.Processor.Repository.Interface;
using CloudFabric.JobHandler.Processor.Service.Interface;

namespace CloudFabric.JobHandler.Processor.Service;

public class JobService: IJobService
{
    private readonly IEditableRepository<Job> _jobRepository;

    public JobService(IEditableRepository<Job> jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public Job CreateJob(int jobTypeId)
    {
        Job job = new Job()
        {
            Id = Guid.NewGuid(),
            CreatorId = 0,
            Created = DateTime.Now,
            JobStatusId = (int)JobStatusEnum.Ready,
            JobTypeId = jobTypeId
        };

        _jobRepository.Insert(job);
        return job;
    }

    public IEnumerable<Job> GetListJobsByStatusId(int jobStatusId) =>
        _jobRepository.Search(new Dictionary<string, object> { { "JobStatusId", jobStatusId } });
}

