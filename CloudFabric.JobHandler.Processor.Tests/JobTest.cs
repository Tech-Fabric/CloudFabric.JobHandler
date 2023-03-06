using CloudFabric.JobHandler.Processor.Enum;
using CloudFabric.JobHandler.Processor.Service.Interface;
using Xunit;

namespace CloudFabric.JobHandler.Processor.Tests;

public class JobTest
{
    private readonly IJobService _jobService;


    public JobTest(IJobService jobService)
    {
        _jobService = jobService;
        ClearData();
    }

    private void ClearData()
    {
        var completedJobs = _jobService.GetAllJobCompleted();
        foreach (var com in completedJobs)
            _jobService.DeleteCompleted(com.Id);

        var processes = _jobService.GetAllProcesses();
        foreach (var proc in processes)
            _jobService.DeleteProcess(proc.Id);

        var jobs = _jobService.GetAllJobs();
        foreach (var job in jobs)
            _jobService.DeleteJob(job.Id);
    }

    [Fact]
    public void CreateAndCheckNewJob()
    {
        // Arrange
        var job = _jobService.CreateJob(1);

        // Act
        var jobs = _jobService.GetListJobsByStatusId((int)JobStatusEnum.Ready);

        // Assert
        Assert.Contains(jobs, item => item.Id == job.Id);
    }

    [Fact]
    public void CreateAndUpdateNewJob()
    {
        // Arrange
        var job = _jobService.CreateJob(1);

        // Act
        _jobService.UpdateJobStatus(job.Id, (int)JobStatusEnum.InProgress);

        // Assert
        var jobs = _jobService.GetListJobsByStatusId((int)JobStatusEnum.InProgress);
        Assert.Contains(jobs, item => item.Id == job.Id);
    }

    [Fact]
    public void CreateAndDeleteNewJob()
    {
        // Arrange
        var job = _jobService.CreateJob(1);

        // Act
        _jobService.DeleteJob(job.Id);
        var jobs = _jobService.GetAllJobs();
        // Assert
        Assert.DoesNotContain(jobs, item => item.Id == job.Id);
    }

    [Fact]
    public void CreateAndRunProcessJob()
    {
        // Arrange
        var job = _jobService.CreateJob(1);

        // Act
        var process = _jobService.CreateJobProcess(job.Id);

        // Assert
        Assert.NotNull(process);
        var processes = _jobService.GetAllProcesses();
        Assert.Contains(processes, item => item.Id == process?.Id && item.JobStatusId == (int)JobStatusEnum.InProgress);
    }

    [Fact]
    public void CreateAndRunProcessNonExistingJob()
    {
        // Arrange
        var job = _jobService.CreateJob(1);

        // Act
        var process = _jobService.CreateJobProcess(Guid.Empty);

        // Assert
        Assert.Null(process);
    }

    [Fact]
    public void CreateAndUpdateProcessStatus()
    {
        // Arrange
        var job = _jobService.CreateJob(1);

        // Act
        var process = _jobService.CreateJobProcess(job.Id);

        Assert.NotNull(process);

        Guid procId = process != null ? process.Id : Guid.Empty;
        _jobService.UpdateProcessStatus(procId, (int)JobStatusEnum.Success);

        // Assert
        var processes = _jobService.GetAllProcesses();
        Assert.Contains(processes, item => item.Id == procId && item.JobStatusId == (int)JobStatusEnum.Success);
    }

    [Fact]
    public void CreateAndUpdateProcessProgress()
    {
        // Arrange
        var job = _jobService.CreateJob(1);

        // Act
        var process = _jobService.CreateJobProcess(job.Id);

        Assert.NotNull(process);

        Guid procId = process != null ? process.Id : Guid.Empty;
        _jobService.UpdateProgress(procId, 57);

        // Assert
        var processes = _jobService.GetAllProcesses();
        Assert.Contains(processes, item => item.Id == procId && item.Progress == 57);
    }

    [Theory]
    [InlineData((int)JobStatusEnum.Success)]
    public void CreateAndCompleteJob(int statusId)
    {
        // Arrange
        var job = _jobService.CreateJob(1);
        var process = _jobService.CreateJobProcess(job.Id);

        Assert.NotNull(process);

        Guid procId = process != null ? process.Id : Guid.Empty;


        // Act
        for (int i = 1; i <= 100; i++)
            _jobService.UpdateProgress(procId, i);

        _jobService.CompleteJob(job.Id);

        // Assert
        var jobs = _jobService.GetAllJobs();
        Assert.Contains(jobs, item => item.Id == job.Id && item.JobStatusId == statusId);

        var processes = _jobService.GetAllProcesses();
        Assert.Contains(processes, item => item.Id == procId && item.JobStatusId == statusId);

        var completed = _jobService.GetAllJobCompleted();
        Assert.Contains(completed, item => item.JobId == job.Id && item.JobStatusId == statusId);
    }

    [Theory]
    [InlineData((int)JobStatusEnum.Failed)]
    public void CreateAndFailedJob(int statusId)
    {
        // Arrange
        var job = _jobService.CreateJob(1);
        var process = _jobService.CreateJobProcess(job.Id);

        Assert.NotNull(process);

        Guid procId = process != null ? process.Id : Guid.Empty;


        // Act
        for (int i = 1; i <= 54; i++)
            _jobService.UpdateProgress(procId, i);

        _jobService.FailJob(job.Id);

        // Assert
        var jobs = _jobService.GetAllJobs();
        Assert.Contains(jobs, item => item.Id == job.Id && item.JobStatusId == statusId);

        var processes = _jobService.GetAllProcesses();
        Assert.Contains(processes, item => item.Id == procId && item.JobStatusId == statusId && item.Progress == 54);

        var completed = _jobService.GetAllJobCompleted();
        Assert.Contains(completed, item => item.JobId == job.Id && item.JobStatusId == statusId);
    }
}