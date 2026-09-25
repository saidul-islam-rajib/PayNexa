using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using PayNexa.Common.Logging;

namespace PayNexa.SqlServer.Logging;

public sealed partial class SqlCommandLoggingInterceptor(ILogger<SqlCommandLoggingInterceptor> logger) : DbCommandInterceptor
{
    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        Started(command, eventData);
        return result;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    {
        Started(command, eventData);
        return ValueTask.FromResult(result);
    }

    public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
    {
        Succeeded(command, eventData);
        return result;
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
    {
        Succeeded(command, eventData);
        return ValueTask.FromResult(result);
    }

    public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
    {
        Started(command, eventData);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Started(command, eventData);
        return ValueTask.FromResult(result);
    }

    public override int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result)
    {
        Succeeded(command, eventData);
        return result;
    }

    public override ValueTask<int> NonQueryExecutedAsync(DbCommand command, CommandExecutedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        Succeeded(command, eventData);
        return ValueTask.FromResult(result);
    }

    public override InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
    {
        Started(command, eventData);
        return result;
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken = default)
    {
        Started(command, eventData);
        return ValueTask.FromResult(result);
    }

    public override object? ScalarExecuted(DbCommand command, CommandExecutedEventData eventData, object? result)
    {
        Succeeded(command, eventData);
        return result;
    }

    public override ValueTask<object?> ScalarExecutedAsync(DbCommand command, CommandExecutedEventData eventData, object? result, CancellationToken cancellationToken = default)
    {
        Succeeded(command, eventData);
        return ValueTask.FromResult(result);
    }

    public override void CommandFailed(DbCommand command, CommandErrorEventData eventData) => Failed(command, eventData);

    public override Task CommandFailedAsync(DbCommand command, CommandErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        Failed(command, eventData);
        return Task.CompletedTask;
    }

    private void Started(DbCommand command, CommandEventData eventData) =>
        LogStarted(logger, LevelFor(command), OperationOf(command), command.Connection?.Database ?? "-", eventData.CommandSource);

    private void Succeeded(DbCommand command, CommandExecutedEventData eventData) =>
        LogSucceeded(logger, LevelFor(command), OperationOf(command), command.Connection?.Database ?? "-", eventData.CommandSource, Math.Round(eventData.Duration.TotalMilliseconds, 2));

    private void Failed(DbCommand command, CommandErrorEventData eventData) =>
        LogFailed(logger, eventData.Exception, OperationOf(command), command.Connection?.Database ?? "-", eventData.CommandSource, Math.Round(eventData.Duration.TotalMilliseconds, 2));

    private static LogLevel LevelFor(DbCommand command) =>
        command.CommandText.Contains(QueryTags.BackgroundPolling, StringComparison.Ordinal) ? LogLevel.Debug : LogLevel.Information;

    private static string OperationOf(DbCommand command)
    {
        var match = SqlOperationPattern().Match(command.CommandText);
        return match.Success ? match.Value.ToUpperInvariant() : "COMMAND";
    }

    [GeneratedRegex(@"\b(SELECT|INSERT|UPDATE|DELETE|MERGE|CREATE|ALTER|DROP|EXEC)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex SqlOperationPattern();

    [LoggerMessage(EventId = LogEventIds.SqlServer,
        Message = "SQL Server {SqlOperation} on {Database} started ({CommandSource})")]
    private static partial void LogStarted(ILogger logger, LogLevel level, string sqlOperation, string database, CommandSource commandSource);

    [LoggerMessage(EventId = LogEventIds.SqlServer + 1,
        Message = "SQL Server {SqlOperation} on {Database} succeeded in {DurationMs} ms ({CommandSource})")]
    private static partial void LogSucceeded(ILogger logger, LogLevel level, string sqlOperation, string database, CommandSource commandSource, double durationMs);

    [LoggerMessage(EventId = LogEventIds.SqlServer + 2, Level = LogLevel.Error,
        Message = "SQL Server {SqlOperation} on {Database} failed in {DurationMs} ms ({CommandSource})")]
    private static partial void LogFailed(ILogger logger, Exception exception, string sqlOperation, string database, CommandSource commandSource, double durationMs);
}
