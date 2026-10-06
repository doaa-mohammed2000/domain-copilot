using DomainCopilot.Application.Interfaces.Ingestion;
using StackExchange.Redis;

namespace DomainCopilot.Infrastructure.Queue;

public class RedisIngestionQueue : IIngestionQueue
{
    private const string QueueName = "domain-copilot:ingestion";

    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;

    public RedisIngestionQueue(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _database = redis.GetDatabase();
    }

    public async Task EnqueueAsync(
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await _database.ListRightPushAsync(
            QueueName,
            jobId.ToString());
    }

    public async Task<Guid?> DequeueAsync(
        CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var result = await _database.ListLeftPopAsync(
                QueueName);

            if (result.HasValue &&
                Guid.TryParse(result.ToString(), out var jobId))
            {
                return jobId;
            }

            await Task.Delay(
                TimeSpan.FromSeconds(1),
                cancellationToken);
        }

        return null;
    }
}