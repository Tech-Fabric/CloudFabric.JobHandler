using System;
using System.Transactions;
using CloudFabric.JobHandler.Processor.Enum;
using CloudFabric.JobHandler.Processor.Model;
using CloudFabric.JobHandler.Processor.Repository.Interface;
using CloudFabric.JobHandler.Processor.Service.Interface;

namespace CloudFabric.JobHandler.Processor.Service;

public class JobService: IJobService
{
    private readonly IEditableRepository<Job> _jobRepository;
    private readonly IEditableRepository<JobProcess> _jobProcessRepository;
    private readonly IEditableRepository<JobCompleted> _jobCompleteRepository;

    public JobService(IEditableRepository<Job> jobRepository,
        IEditableRepository<JobProcess> jobProcessRepository,
        IEditableRepository<JobCompleted> jobCompleteRepository)
    {
        _jobRepository = jobRepository;
        _jobProcessRepository = jobProcessRepository;
        _jobCompleteRepository = jobCompleteRepository;
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

    public JobProcess? CreateJobProcess(Guid jobId)
    {
        try
        {
            JobProcess jobProcess = new JobProcess()
            {
                Id = Guid.NewGuid(),
                JobId = jobId,
                JobStatusId = (int)JobStatusEnum.InProgress,
                Progress = 0,
                LastProcess = DateTime.Now
            };

            using (TransactionScope trx = new TransactionScope())
            {
                _jobProcessRepository.Insert(jobProcess);

                UpdateJobStatus(jobId, (int)JobStatusEnum.InProgress);
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

    private JobProcess GetJobProcessByJobId(Guid jobId) =>
        _jobProcessRepository.Search(new Dictionary<string, object>() { { nameof(JobProcess.JobId), jobId } }).First();

    public void FailJob(Guid jobId)
    {
        JobCompleted jobCompleted = new JobCompleted()
        {
            Id = Guid.NewGuid(),
            JobId = jobId,
            JobStatusId = (int)JobStatusEnum.Failed,
            Completed = DateTime.Now
        };

        using (TransactionScope trx = new TransactionScope())
        {
            _jobCompleteRepository.Insert(jobCompleted);

            UpdateProcessStatus(GetJobProcessByJobId(jobId).Id, (int)JobStatusEnum.Failed);

            UpdateJobStatus(jobId, (int)JobStatusEnum.Failed);

            trx.Complete();
        }
    }

    public void CompleteJob(Guid jobId)
    {
        JobCompleted jobCompleted = new JobCompleted()
        {
            Id = Guid.NewGuid(),
            JobId = jobId,
            JobStatusId = (int)JobStatusEnum.Success,
            Completed = DateTime.Now
        };

        using (TransactionScope trx = new TransactionScope())
        {
            _jobCompleteRepository.Insert(jobCompleted);

            UpdateProcessStatus(GetJobProcessByJobId(jobId).Id, (int)JobStatusEnum.Success);

            UpdateJobStatus(jobId, (int)JobStatusEnum.Success);

            trx.Complete();
        }
    }

    public IEnumerable<Job> GetAllJobs() =>
        _jobRepository.GetAll();

    public IEnumerable<JobProcess> GetAllProcesses() =>
        _jobProcessRepository.GetAll();
    
    public IEnumerable<Job> GetListJobsByStatusId(int jobStatusId) =>
        _jobRepository.Search(new Dictionary<string, object> { { nameof(Job.JobStatusId), jobStatusId } });

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
}

