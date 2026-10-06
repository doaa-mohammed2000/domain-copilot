using DomainCopilot.Domain.Entities;

namespace DomainCopilot.Application.Interfaces.Ingestion;

public interface IIngestionJobRepository
{
    Task<IngestionJob?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IngestionJob?> GetByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        IngestionJob job,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}