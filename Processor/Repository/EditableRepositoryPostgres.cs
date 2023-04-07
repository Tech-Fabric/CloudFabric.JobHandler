using System;
using System.Collections;
using System.Reflection;
using System.Text;
using CloudFabric.JobHandler.Processor.Model.Settings;
using CloudFabric.JobHandler.Processor.Repository.Interface;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace CloudFabric.JobHandler.Processor.Repository;

public class EditableRepositoryPostgres<T> : ReadableRepositoryPostgres<T>, IEditableRepository<T>
{
    private readonly string _tableName;
    private readonly string _keyField = "Id";

    private static IEnumerable<PropertyInfo> EnumerableProperties => typeof(T).GetProperties()
        .Where(p => p.PropertyType != typeof(string) && p.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)));

    private static IEnumerable<PropertyInfo> Properties => typeof(T).GetProperties().Except(EnumerableProperties);


    public EditableRepositoryPostgres(IOptions<JobHandlerSettings> settings) : base(settings)
    {
        _tableName = $"{typeof(T).Name}";
    }

    public virtual Guid Create(T entity)
    {
        var insertQuery = GenerateInsertQuery();

        insertQuery += $" RETURNING {_keyField};";
        using var connection = GetConnection();

        return connection.QuerySingle<Guid>(insertQuery, entity);
    }

    public virtual void Insert(T entity)
    {
        var insertQuery = GenerateInsertQuery();

        using var connection = GetConnection();
        connection.Execute(insertQuery, entity);
    }

    public T CreateAndLoad(T entity)
    {
        var id = Create(entity);
        return Get(id);
    }

    public virtual void Update(T entity)
    {
        var updateQuery = GenerateUpdateQuery();

        using var connection = GetConnection();
        connection.Execute(updateQuery, entity);
    }

    public void UpdateById(Guid uuid, Dictionary<string, object>? updateFields)
    {
        var entity = LoadAndReplace(uuid, updateFields);
        Update(entity);
    }

    public void Delete(T entity)
    {
        var deleteQuery = GenerateDeleteQuery();

        using var connection = GetConnection();
        connection.Execute(deleteQuery, entity);
    }

    private string GenerateInsertQuery()
    {
        var insertQuery = new StringBuilder($"insert into \"{_tableName}\" ");

        insertQuery.Append('(');

        var properties = GenerateListOfProperties(Properties);

        insertQuery.AppendJoin(",", properties);

        insertQuery.Append(") values (");

        insertQuery.AppendJoin(",", properties.Select(p => $"@{p}"));

        insertQuery.Append(')');

        return insertQuery.ToString();
    }

    private string GenerateUpdateQuery()
    {
        var updateQuery = new StringBuilder($"update \"{_tableName}\" set ");
        var properties = GenerateListOfProperties(Properties);

        updateQuery.AppendJoin(", ", properties.Where(p => p != _keyField).Select(prop => $"{prop}=@{prop}"));

        updateQuery.Append($" where {_keyField}=@{_keyField}");

        return updateQuery.ToString();
    }

    private string GenerateDeleteQuery() =>
        $" delete from \"{_tableName}\" where {_keyField} = @{_keyField} ";

    private static List<string> GenerateListOfProperties(IEnumerable<PropertyInfo> listOfProperties) =>
        listOfProperties
            .Select(p => p.Name)
            .ToList();

    public void DeleteById(Guid uuid)
    {
        var dynamicParameters = new DynamicParameters();
        dynamicParameters.Add(_keyField, uuid);

        var deleteQuery = GenerateDeleteQuery();

        using var connection = GetConnection();
        connection.Execute(deleteQuery, dynamicParameters);
    }
}
