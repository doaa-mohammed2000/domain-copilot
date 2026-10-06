using DomainCopilot.Application.Interfaces.Ingestion;
using DomainCopilot.Domain.Entities;
using DomainCopilot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DomainCopilot.Infrastructure.Repositories;

public class IngestionJobRepository : IIngestionJobRepository
{
    private readonly DomainCopilotDbContext _dbContext;

    public IngestionJobRepository(DomainCopilotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IngestionJob?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.IngestionJobs
            .Include(x => x.Document)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IngestionJob?> GetByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.IngestionJobs
            .FirstOrDefaultAsync(
                x => x.IdempotencyKey == idempotencyKey,
                cancellationToken);
    }

    public async Task AddAsync(
        IngestionJob job,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.IngestionJobs.AddAsync(
            job,
            cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}