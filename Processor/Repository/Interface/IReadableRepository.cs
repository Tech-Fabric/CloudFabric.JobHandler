using System;
using System.Data;

namespace CloudFabric.JobHandler.Processor.Repository.Interface;

public interface IReadableRepository<T>
{
    string KeyField { get; set; }

    T Get(object id);
    IEnumerable<T> GetAll();
    T LoadAndReplace(object id, Dictionary<string, object>? updateFields);
    IEnumerable<T> Search(Dictionary<string, object> parameters);
    IEnumerable<T> SearchWithLimit(Dictionary<string, object> parameters, int? recordCount);
}