using DomainCopilot.Application.Interfaces;
using DomainCopilot.Domain.Entities;

namespace DomainCopilot.Application.Services;

public class DocumentService
{
    private readonly IDocumentRepository _documentRepository;

    public DocumentService(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<Document?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _documentRepository.GetByIdAsync(
            id,
            cancellationToken);
    }
}