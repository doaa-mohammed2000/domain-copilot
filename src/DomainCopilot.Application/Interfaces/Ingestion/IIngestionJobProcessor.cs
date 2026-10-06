namespace DomainCopilot.Application.Interfaces.Ingestion;

public interface IIngestionJobProcessor
{
    Task ProcessAsync(
        Guid jobId,
        CancellationToken cancellationToken = default);
}