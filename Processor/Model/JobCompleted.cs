using System;
namespace CloudFabric.JobHandler.Processor.Model;

public class JobCompleted
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public int JobStatusId { get; set; }
    public DateTime Completed { get; set; }
    public string? ErrorMessage { get; set; }
}

