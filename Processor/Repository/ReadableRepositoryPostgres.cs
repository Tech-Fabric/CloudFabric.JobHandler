using System;
using System.Data;
using CloudFabric.JobHandler.Processor.Repository.Interface;
using Npgsql;
using Dapper;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using CloudFabric.JobHandler.Processor.Model.Settings;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace CloudFabric.JobHandler.Processor.Repository;

public class ReadableRepositoryPostgres<T>: IReadableRepository<T>
{
    private readonly string _selectString;
    private readonly string _getByIdString;
    private readonly JobHandlerSettings _settings;
    private readonly string _keyField = "Id";

    protected NpgsqlConnection GetConnection() =>
        new NpgsqlConnection(_settings.ConnectionString);

    public ReadableRepositoryPostgres(IOptions<JobHandlerSettings> settings)
    {
        _settings = settings.Value;
        string _tableName = $"{typeof(T).Name}";
        _selectString = $"select * from \"{_tableName}\"";
        _getByIdString = $"{_selectString} where {_keyField} = @id";
    }

    public T Get(object id)
    {
        using var conn = GetConnection();
        var dictionary = new Dictionary<string, object>
        {
            { "@id", id }
        };
        var parameters = new DynamicParameters(dictionary);
        var queryResult = conn.QuerySingle<T>(_getByIdString, parameters);
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

    public IEnumerable<T> Query(Dictionary<string, object> parameters, int? recordCount)
    {
        using var connection = GetConnection();
        var dynamicParams = new DynamicParameters();
        StringBuilder sqlBuilder = GetSearchQuery(parameters, dynamicParams, recordCount);
        
        return connection.Query<T>(sqlBuilder.ToString(), dynamicParams);
    }

    private StringBuilder GetSearchQuery(Dictionary<string, object> parameters, DynamicParameters dynamicParams, int? recordCount)
    {
        StringBuilder sqlBuilder = new StringBuilder($"{_selectString} where 1 = 1 ");

        foreach (var p in parameters)
        {
            sqlBuilder.Append($" and {p.Key} = @{p.Key}");
            dynamicParams.Add(p.Key, p.Value);
        }

        if (recordCount != null)
            sqlBuilder.Append($" limit {recordCount}");

        return sqlBuilder;
    }
}

