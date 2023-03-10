using System;
namespace CloudFabric.JobHandler.Processor.Model.Settings
{
	public class JobHandlerSettings
	{
		public const string Position = "JobHandlerSettings";

        public string? ConnectionString { get; set; }
	}
}

