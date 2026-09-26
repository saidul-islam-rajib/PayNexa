using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver.Core.Events;
using PayNexa.Common.Logging;

namespace PayNexa.MongoDb.Logging;

internal sealed partial class MongoCommandLogger(ILogger<MongoCommandLogger> logger)
{
    private static readonly HashSet<string> IgnoredCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "hello", "isMaster", "buildInfo", "saslStart", "saslContinue", "ping", "endSessions", "killCursors", "getLastError",
    };

    private readonly ConcurrentDictionary<int, string> _collections = new();

    public void OnStarted(CommandStartedEvent @event)
    {
        if (IgnoredCommands.Contains(@event.CommandName))
        {
            return;
        }

        var collection = CollectionOf(@event.CommandName, @event.Command);
        _collections[@event.RequestId] = collection;
        LogStarted(logger, @event.CommandName, collection, @event.DatabaseNamespace.DatabaseName);
    }

    public void OnSucceeded(CommandSucceededEvent @event)
    {
        if (!_collections.TryRemove(@event.RequestId, out var collection))
        {
            return;
        }

        LogSucceeded(logger, @event.CommandName, collection, @event.DatabaseNamespace.DatabaseName, Math.Round(@event.Duration.TotalMilliseconds, 2));
    }

    public void OnFailed(CommandFailedEvent @event)
    {
        if (!_collections.TryRemove(@event.RequestId, out var collection))
        {
            return;
        }

        LogFailed(logger, @event.Failure, @event.CommandName, collection, @event.DatabaseNamespace.DatabaseName, Math.Round(@event.Duration.TotalMilliseconds, 2));
    }

    private static string CollectionOf(string commandName, BsonDocument command)
    {
        if (command.TryGetValue(commandName, out var target) && target.IsString)
        {
            return target.AsString;
        }

        return command.TryGetValue("collection", out var collection) && collection.IsString ? collection.AsString : "-";
    }

    [LoggerMessage(EventId = LogEventIds.MongoDb, Level = LogLevel.Information,
        Message = "MongoDB {MongoCommand} on {Collection} started ({Database})")]
    private static partial void LogStarted(ILogger logger, string mongoCommand, string collection, string database);

    [LoggerMessage(EventId = LogEventIds.MongoDb + 1, Level = LogLevel.Information,
        Message = "MongoDB {MongoCommand} on {Collection} succeeded in {DurationMs} ms ({Database})")]
    private static partial void LogSucceeded(ILogger logger, string mongoCommand, string collection, string database, double durationMs);

    [LoggerMessage(EventId = LogEventIds.MongoDb + 2, Level = LogLevel.Error,
        Message = "MongoDB {MongoCommand} on {Collection} failed in {DurationMs} ms ({Database})")]
    private static partial void LogFailed(ILogger logger, Exception exception, string mongoCommand, string collection, string database, double durationMs);
}
