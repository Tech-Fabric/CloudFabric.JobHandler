
using CloudFabric.JobHandler.Processor.Model;

namespace CloudFabric.JobHandler.Processor.Service.Interface;

public interface IJobService
{
    Job CreateJob(int jobTypeId, string parameters, Guid tenantId);
    Job CreateAndLoadJob(int jobTypeId, string parameters, Guid tenantId);

    JobProcess? CreateJobProcess(Guid jobId);

    void UpdateProgress(Guid jobProcessId, int progress);

    Job GetJobById(Guid jobId);

    void CompleteJob(Guid jobId);

    void FailJob(Guid jobId, string errorMessage);


    IEnumerable<Job> GetListJobsByTenantId(Guid tenantId, int? recordCount);
    IEnumerable<Job> GetListJobsByTenantId(Guid tenantId, int jobStatusId, int? recordCount);

    IEnumerable<Job> GetListJobsByStatusId(int jobStatusId, int jobTypeId, int? rowCount);

    IEnumerable<Job> GetAllJobs();

    void UpdateJobStatus(Guid jobId, int newJobStatusId);

    void DeleteJob(Guid jobId);
    void DeleteJob(Job job);

    JobProcess GetJobProcessByJobId(Guid jobId);

    IEnumerable<JobProcess> GetAllProcesses();
    void UpdateProcessStatus(Guid jobProcessId, int jobStatusId);
    void DeleteProcess(Guid processId);


    IEnumerable<JobCompleted> GetAllJobCompleted();

    void DeleteCompleted(Guid jobCompletedId);

    IEnumerable<JobType> GetJobTypes();
    IEnumerable<JobStatus> GetJobStatuses();
}

