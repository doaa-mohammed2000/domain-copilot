using DomainCopilot.Application.Interfaces;
using DomainCopilot.Domain.Entities;
using DomainCopilot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DomainCopilot.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly DomainCopilotDbContext _dbContext;

    public DocumentRepository(DomainCopilotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Document?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Documents
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Document?> GetByContentHashAsync(
        string contentHash,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Documents
            .FirstOrDefaultAsync(x => x.ContentHash == contentHash, cancellationToken);
    }

    public async Task AddAsync(
        Document document,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Documents.AddAsync(document, cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}