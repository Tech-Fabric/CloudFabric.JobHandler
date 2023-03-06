using System;
using CloudFabric.JobHandler.Processor.Model;

namespace CloudFabric.JobHandler.Processor.Service.Interface;

public interface IJobService
{
    Job CreateJob(int jobTypeId);

    IEnumerable<Job> GetListJobsByStatusId(int jobStatusId);

    IEnumerable<Job> GetAllJobs();

    void UpdateJobStatus(Guid jobId, int newJobStatusId);

    void DeleteJob(Guid jobId);

    JobProcess? CreateJobProcess(Guid jobId);
    IEnumerable<JobProcess> GetAllProcesses();
    void UpdateProgress(Guid jobProcessId, int progress);
    void UpdateProcessStatus(Guid jobProcessId, int jobStatusId);
    void DeleteProcess(Guid processId);

    void CompleteJob(Guid jobId);
    void FailJob(Guid jobId);
    IEnumerable<JobCompleted> GetAllJobCompleted();
    void DeleteCompleted(Guid jobCompletedId);
}

