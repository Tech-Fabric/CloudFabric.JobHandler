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
}
