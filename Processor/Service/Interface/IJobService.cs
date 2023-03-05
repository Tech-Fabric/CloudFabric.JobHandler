using System;
using CloudFabric.JobHandler.Processor.Model;

namespace CloudFabric.JobHandler.Processor.Service.Interface;

public interface IJobService
{
    Job CreateJob(int jobTypeId);

    IEnumerable<Job> GetListJobsByStatusId(int jobStatusId);
}

