using System;
namespace CloudFabric.JobHandler.Processor.Model;

public class Job
{
    public Guid Id { get; set; }
    public int JobTypeId { get; set; }
    public int JobStatusId { get; set; }
    public DateTime Created { get; set; }
    public int CreatorId { get; set; }
}

