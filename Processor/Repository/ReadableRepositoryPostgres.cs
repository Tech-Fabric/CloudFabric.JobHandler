using System;
using System.Data;
using CloudFabric.JobHandler.Processor.Repository.Interface;
using Npgsql;
using Dapper;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace CloudFabric.JobHandler.Processor.Repository;

public class ReadableRepositoryPostgres<T>: IReadableRepository<T>
{
    private readonly string _tableName;
    private readonly string _selectString;
    private readonly IConfiguration _configuration;

    public string KeyField { get; set; } = "Id";

    protected NpgsqlConnection GetConnection() =>
        new NpgsqlConnection(_configuration.GetConnectionString("JobDb"));


    public ReadableRepositoryPostgres(IConfiguration configuration)
    {
        _configuration = configuration;
        _tableName = $"{typeof(T).Name}";
        _selectString = $"select * from \"{_tableName}\"";
    }

    public T Get(object id)
    {
        using var conn = GetConnection();
        var query = $"{_selectString} where {KeyField} = @id";
        var queryResult = conn.QueryFirst<T>(query, new { id = id });
        return queryResult;
    }

    public IEnumerable<T> GetAll()
    {
        using var connection = GetConnection();
        return connection.Query<T>(_selectString);

    }

    public T LoadAndReplace(object id, Dictionary<string, object>? updateFields)
    {
        T entity = Get(id);

        if (updateFields == null) return entity;

        Type info = typeof(T);

        foreach (var (key, value) in updateFields)
        {
            PropertyInfo? pi = info.GetProperty(key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (pi != null) pi.SetValue(entity, Convert.ChangeType(value, pi.PropertyType));
        }

        return entity;
    }

    public IEnumerable<T> Search(Dictionary<string, object> parameters)
    {
        using var connection = GetConnection();
        var dynamicParams = new DynamicParameters();
        var sql = $"{_selectString} where 1 = 1 ";

        foreach (var p in parameters)
        {
            sql += $" and \"{p.Key}\" = @{p.Key}";
            dynamicParams.Add(p.Key, p.Value);
        }

        return connection.Query<T>(sql, dynamicParams);
    }
}

