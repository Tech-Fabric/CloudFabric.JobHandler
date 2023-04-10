using System;
namespace CloudFabric.JobHandler.Processor.Repository.Interface;

public interface IEditableRepository<T>: IReadableRepository<T>
{
    Guid Create(T entity);
    T CreateAndLoad(T entity);
    void Insert(T entity);
    void Update(T entity);
    void UpdateById(Guid uuid, Dictionary<string, object>? updateFields);
    void Delete(T entity);
    void DeleteById(Guid uuid);
}
