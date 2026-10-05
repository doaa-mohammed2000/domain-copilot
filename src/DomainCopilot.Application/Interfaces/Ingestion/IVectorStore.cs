using DomainCopilot.Domain.Entities;

namespace DomainCopilot.Application.Interfaces.Ingestion;

public interface IVectorStore
{
    Task UpsertAsync(
        DocumentChunk chunk,
        IReadOnlyList<float> embedding,
        CancellationToken cancellationToken = default);
}