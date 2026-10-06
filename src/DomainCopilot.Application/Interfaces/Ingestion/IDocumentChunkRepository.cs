using DomainCopilot.Domain.Entities;

namespace DomainCopilot.Application.Interfaces.Ingestion;

public interface IDocumentChunkRepository
{
    Task DeleteByDocumentIdAsync(
        Guid documentId,
        CancellationToken cancellationToken = default);

    Task AddRangeAsync(
        IReadOnlyCollection<DocumentChunk> chunks,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}