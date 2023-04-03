using System;
namespace CloudFabric.JobHandler.Processor.Model;

public class JobProcess
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public int Progress { get; set; }
    public int JobStatusId { get; set; }
    public DateTime LastProcess { get; set; }
}

