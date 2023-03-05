using System;
using System.Collections;
using System.Reflection;
using System.Text;
using CloudFabric.JobHandler.Processor.Repository.Interface;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace CloudFabric.JobHandler.Processor.Repository;

public class EditableRepositoryPostgres<T>: ReadableRepositoryPostgres<T>, IEditableRepository<T>
{
    private readonly string _tableName;

    private static IEnumerable<PropertyInfo> EnumerableProperties => typeof(T).GetProperties()
        .Where(p => p.PropertyType != typeof(string) && p.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)));

    private static IEnumerable<PropertyInfo> Properties => typeof(T).GetProperties().Except(EnumerableProperties);


    public EditableRepositoryPostgres(IConfiguration configuration): base(configuration)
	{
        _tableName = $"{typeof(T).Name}";
    }

    public virtual Guid Create(T entity)
    {
        var insertQuery = GenerateInsertQuery(true);

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

    public void UpdateById(Guid id, Dictionary<string, object>? updateFields)
    {
        var entity = LoadAndReplace(id, updateFields);
        Update(entity);
    }

    public void Delete(T entity)
    {
        var deleteQuery = GenerateDeleteQuery();

        using var connection = GetConnection();
        connection.Execute(deleteQuery, entity);
    }

    private string GenerateInsertQuery(bool returnIdentity = false)
    {
        var insertQuery = new StringBuilder($"insert into \"{_tableName}\" ");

        insertQuery.Append('(');

        var properties = GenerateListOfProperties(Properties);
        properties.ForEach(prop =>
        {
            insertQuery.Append((string?)$"\"{prop}\",");
        });

            insertQuery
                .Remove(insertQuery.Length - 1, 1)
                .Append(") values (");

        properties.ForEach(prop =>
        {
            insertQuery.Append((string?)$"(@{prop}),");
        });

        insertQuery
            .Remove(insertQuery.Length - 1, 1)
            .Append(')');

        return insertQuery.ToString();
    }

    private string GenerateUpdateQuery()
    {
        var updateQuery = new StringBuilder($"update \"{_tableName}\" set ");
        var properties = GenerateListOfProperties(Properties);

        properties.ForEach(prop =>
        {
            if (!string.Equals(prop, KeyField, StringComparison.OrdinalIgnoreCase))
            {
                updateQuery.Append($"{prop}=@{prop},");
            }
        });

        //remove last comma
        updateQuery.Remove(updateQuery.Length - 1, 1);
        updateQuery.Append($" where {KeyField}=@{KeyField}");

        return updateQuery.ToString();
    }

    private string GenerateDeleteQuery() =>
        $"delete from \"{_tableName}\" where {KeyField}=@{KeyField}";

    private static List<string> GenerateListOfProperties(IEnumerable<PropertyInfo> listOfProperties) =>
        listOfProperties
            .Select(p => p.Name)
            .ToList();

    public void DeleteById(Guid id)
    {
        var deleteQuery = GenerateDeleteQuery().Replace($"@{KeyField}", id.ToString());

        using var connection = GetConnection();
        connection.Execute(deleteQuery);
    }
}

