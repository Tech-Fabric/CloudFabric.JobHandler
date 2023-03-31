
using CloudFabric.JobHandler.Processor.Model;

namespace CloudFabric.JobHandler.Processor.Service.Interface;

public interface IJobService
{
    Job CreateJob(int jobTypeId, string parameters, Guid tenantId);

    JobProcess? CreateJobProcess(Guid jobId);

    void UpdateProgress(Guid jobProcessId, int progress);

    void CompleteJob(Guid jobId);

    void FailJob(Guid jobId, string errorMessage);


    IEnumerable<Job> GetListJobsByTenantId(Guid tenantId);
    IEnumerable<Job> GetListJobsByTenantId(Guid tenantId, int jobStatusId);

    IEnumerable<Job> GetListJobsByStatusId(int jobStatusId, int jobTypeId);

    IEnumerable<Job> GetAllJobs();

    void UpdateJobStatus(Guid jobId, int newJobStatusId);

    void DeleteJob(Guid jobId);

    JobProcess GetJobProcessByJobId(Guid jobId);

    IEnumerable<JobProcess> GetAllProcesses();
    void UpdateProcessStatus(Guid jobProcessId, int jobStatusId);
    void DeleteProcess(Guid processId);


    IEnumerable<JobCompleted> GetAllJobCompleted();

    void DeleteCompleted(Guid jobCompletedId);

    IEnumerable<JobType> GetJobTypes();
}

