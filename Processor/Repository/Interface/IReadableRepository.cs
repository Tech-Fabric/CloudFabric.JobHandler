using System;
using System.Data;

namespace CloudFabric.JobHandler.Processor.Repository.Interface;

public interface IReadableRepository<T>
{
    T Get(object id);
    IEnumerable<T> GetAll();
    T LoadAndReplace(object id, Dictionary<string, object>? updateFields);
    IEnumerable<T> Query(Dictionary<string, object> parameters, int? recordCount);
}
