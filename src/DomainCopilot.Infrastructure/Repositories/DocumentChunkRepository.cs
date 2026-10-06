using DomainCopilot.Application.Interfaces.Ingestion;
using DomainCopilot.Domain.Entities;
using DomainCopilot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DomainCopilot.Infrastructure.Repositories;

public class DocumentChunkRepository : IDocumentChunkRepository
{
    private readonly DomainCopilotDbContext _dbContext;

    public DocumentChunkRepository(DomainCopilotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task DeleteByDocumentIdAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.DocumentChunks
            .Where(x => x.DocumentId == documentId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task AddRangeAsync(
        IReadOnlyCollection<DocumentChunk> chunks,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.DocumentChunks.AddRangeAsync(
            chunks,
            cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}