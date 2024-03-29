using System;
namespace CloudFabric.JobHandler.Processor.Model;

public class Job
{
    public Guid Id { get; set; }
    public int JobTypeId { get; set; }
    public int JobStatusId { get; set; }
    public DateTime Created { get; set; }
    public int CreatorId { get; set; }
    public string? Parameters { get; set; }
    public Guid TenantId { get; set; }
}