using System.Data.Common;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Prometheus;

namespace Backend.Infrastructure.Data.Interceptors;

/// <summary>
/// EF Core interceptor that automatically records DB query duration as Prometheus metrics.
/// Uses the same histogram defined in AppMetrics (cni_db_query_duration_seconds).
/// </summary>
public partial class MetricsDbCommandInterceptor : DbCommandInterceptor
{
    private static readonly Histogram DbQueryDuration = Metrics.CreateHistogram(
        "cni_db_query_duration_seconds",
        "Duration of database queries in seconds",
        new HistogramConfiguration
        {
            LabelNames = ["operation", "table"],
            Buckets = Histogram.ExponentialBuckets(0.001, 2, 14)
        });

    [GeneratedRegex(
        @"(?:FROM|INTO|UPDATE|DELETE\s+FROM|JOIN)\s+""?(\w+)""?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 50)]
    private static partial Regex TableNamePattern();

    public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
    {
        RecordMetric(command, eventData);
        return base.ReaderExecuted(command, eventData, result);
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
    {
        RecordMetric(command, eventData);
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result)
    {
        RecordMetric(command, eventData);
        return base.NonQueryExecuted(command, eventData, result);
    }

    public override ValueTask<int> NonQueryExecutedAsync(DbCommand command, CommandExecutedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        RecordMetric(command, eventData);
        return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override object? ScalarExecuted(DbCommand command, CommandExecutedEventData eventData, object? result)
    {
        RecordMetric(command, eventData);
        return base.ScalarExecuted(command, eventData, result);
    }

    public override ValueTask<object?> ScalarExecutedAsync(DbCommand command, CommandExecutedEventData eventData, object? result, CancellationToken cancellationToken = default)
    {
        RecordMetric(command, eventData);
        return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    private static void RecordMetric(DbCommand command, CommandExecutedEventData eventData)
    {
        var durationSeconds = eventData.Duration.TotalSeconds;
        var sql = command.CommandText;
        var operation = InferOperation(sql);
        var table = InferTable(sql);

        DbQueryDuration.WithLabels(operation, table).Observe(durationSeconds);
    }

    private static string InferOperation(string sql)
    {
        var trimmed = sql.AsSpan().TrimStart();
        if (trimmed.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase)) return "read";
        if (trimmed.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase)) return "write";
        if (trimmed.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase)) return "update";
        if (trimmed.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase)) return "delete";
        return "other";
    }

    private static string InferTable(string sql)
    {
        var match = TableNamePattern().Match(sql);
        return match.Success ? match.Groups[1].Value : "unknown";
    }
}
