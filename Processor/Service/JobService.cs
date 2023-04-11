
using System.Transactions;
using CloudFabric.JobHandler.Processor.Enum;
using CloudFabric.JobHandler.Processor.Model;
using CloudFabric.JobHandler.Processor.Repository.Interface;
using CloudFabric.JobHandler.Processor.Service.Interface;

namespace CloudFabric.JobHandler.Processor.Service;

public class JobService : IJobService
{
    private readonly IEditableRepository<Job> _jobRepository;
    private readonly IEditableRepository<JobProcess> _jobProcessRepository;
    private readonly IEditableRepository<JobCompleted> _jobCompleteRepository;
    private readonly IReadableRepository<JobType> _jobTypeRepository;
    private readonly IReadableRepository<JobStatus> _jobStatusRepository;

    public JobService(IEditableRepository<Job> jobRepository,
        IEditableRepository<JobProcess> jobProcessRepository,
        IEditableRepository<JobCompleted> jobCompleteRepository,
        IReadableRepository<JobType> jobTypeRepository,
        IReadableRepository<JobStatus> jobStatusRepository)
    {
        _jobRepository = jobRepository;
        _jobProcessRepository = jobProcessRepository;
        _jobCompleteRepository = jobCompleteRepository;
        _jobTypeRepository = jobTypeRepository;
        _jobStatusRepository = jobStatusRepository;
    }

    public Job CreateJob(int jobTypeId, string parameters, Guid tenantId)
    {
        Job job = new Job()
        {
            Id = Guid.NewGuid(),
            CreatorId = 0,
            Created = DateTime.Now,
            JobStatusId = (int)JobStatuses.Ready,
            JobTypeId = jobTypeId,
            Parameters = parameters,
            TenantId = tenantId
        };

        _jobRepository.Insert(job);
        return job;
    }

    public Job CreateAndLoadJob(int jobTypeId, string parameters, Guid tenantId)
    {
        Job job = new Job()
        {
            Id = Guid.NewGuid(),
            CreatorId = 0,
            Created = DateTime.Now,
            JobStatusId = (int)JobStatuses.Ready,
            JobTypeId = jobTypeId,
            Parameters = parameters,
            TenantId = tenantId
        };

        var newJob = _jobRepository.CreateAndLoad(job);
        return newJob;
    }

    public JobProcess? CreateJobProcess(Guid jobId)
    {
        try
        {
            JobProcess jobProcess = new JobProcess()
            {
                Id = Guid.NewGuid(),
                JobId = jobId,
                JobStatusId = (int)JobStatuses.InProgress,
                Progress = 0,
                LastProcess = DateTime.Now
            };

            using (TransactionScope trx = new TransactionScope())
            {
                _jobProcessRepository.Insert(jobProcess);

                UpdateJobStatus(jobId, (int)JobStatuses.InProgress);
                trx.Complete();
                return jobProcess;
            }
        }
        catch
        {
            return null;
        }
    }

    public void DeleteJob(Guid jobId) =>
        _jobRepository.DeleteById(jobId);

    public void DeleteJob(Job job) =>
        _jobRepository.Delete(job);

    public JobProcess GetJobProcessByJobId(Guid jobId) =>
        _jobProcessRepository.Query(new Dictionary<string, object>() { { nameof(JobProcess.JobId), jobId } }, 1).First();

    public void FailJob(Guid jobId, string errorMessage)
    {
        JobCompleted jobCompleted = new JobCompleted()
        {
            Id = Guid.NewGuid(),
            JobId = jobId,
            JobStatusId = (int)JobStatuses.Failed,
            Completed = DateTime.Now,
            ErrorMessage = errorMessage
        };

        using (TransactionScope trx = new TransactionScope())
        {
            _jobCompleteRepository.Insert(jobCompleted);

            var job = GetJobById(jobId);

            if (job.JobStatusId == (int)JobStatuses.InProgress)
                UpdateProcessStatus(GetJobProcessByJobId(jobId).Id, (int)JobStatuses.Failed);

            UpdateJobStatus(jobId, (int)JobStatuses.Failed);

            trx.Complete();
        }
    }

    public void CompleteJob(Guid jobId)
    {
        JobCompleted jobCompleted = new JobCompleted()
        {
            Id = Guid.NewGuid(),
            JobId = jobId,
            JobStatusId = (int)JobStatuses.Success,
            Completed = DateTime.Now
        };

        using (TransactionScope trx = new TransactionScope())
        {
            _jobCompleteRepository.Insert(jobCompleted);

            UpdateProcessStatus(GetJobProcessByJobId(jobId).Id, (int)JobStatuses.Success);

            UpdateJobStatus(jobId, (int)JobStatuses.Success);

            trx.Complete();
        }
    }

    public IEnumerable<Job> GetAllJobs() =>
        _jobRepository.GetAll();

    public IEnumerable<JobProcess> GetAllProcesses() =>
        _jobProcessRepository.GetAll();

    public IEnumerable<Job> GetListJobsByStatusId(int jobStatusId, int jobTypeId, int? rowCount) =>
        _jobRepository.Query(new Dictionary<string, object> { { nameof(Job.JobStatusId), jobStatusId }, { nameof(Job.JobTypeId), jobTypeId } }, rowCount);

    public void UpdateJobStatus(Guid jobId, int newJobStatusId) =>
        _jobRepository.UpdateById(jobId, new Dictionary<string, object> { { nameof(Job.JobStatusId), newJobStatusId } });

    public void UpdateProcessStatus(Guid jobProcessId, int jobStatusId) =>
        _jobProcessRepository.UpdateById(jobProcessId, new Dictionary<string, object> {
            { nameof(JobProcess.JobStatusId), jobStatusId },
            { nameof(JobProcess.LastProcess), DateTime.Now } });

    public void UpdateProgress(Guid jobProcessId, int progress) =>
        _jobProcessRepository.UpdateById(jobProcessId, new Dictionary<string, object> {
            { nameof(JobProcess.Progress), progress },
            { nameof(JobProcess.LastProcess), DateTime.Now } });

    public IEnumerable<JobCompleted> GetAllJobCompleted() =>
        _jobCompleteRepository.GetAll();

    public void DeleteProcess(Guid processId) =>
        _jobProcessRepository.DeleteById(processId);

    public void DeleteCompleted(Guid jobCompletedId) =>
        _jobCompleteRepository.DeleteById(jobCompletedId);

    public IEnumerable<JobType> GetJobTypes() =>
        _jobTypeRepository.GetAll();

    public IEnumerable<JobStatus> GetJobStatuses() =>
        _jobStatusRepository.GetAll();

    public IEnumerable<Job> GetListJobsByTenantId(Guid tenantId, int? recordCount) =>
        _jobRepository.Query(new Dictionary<string, object> { { nameof(Job.TenantId), tenantId } }, recordCount);

    public IEnumerable<Job> GetListJobsByTenantId(Guid tenantId, int jobStatusId, int? recordCount) =>
        _jobRepository.Query(new Dictionary<string, object> { { nameof(Job.TenantId), tenantId }, { nameof(Job.JobStatusId), jobStatusId } }, recordCount);

    public Job GetJobById(Guid jobId) =>
        _jobRepository.Get(jobId);
}