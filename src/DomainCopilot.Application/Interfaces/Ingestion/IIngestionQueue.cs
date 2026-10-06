namespace DomainCopilot.Application.Interfaces.Ingestion;

public interface IIngestionQueue
{
    Task EnqueueAsync(
        Guid jobId,
        CancellationToken cancellationToken = default);

    Task<Guid?> DequeueAsync(
        CancellationToken cancellationToken = default);
}