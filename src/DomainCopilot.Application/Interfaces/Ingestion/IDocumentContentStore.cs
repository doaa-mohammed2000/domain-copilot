namespace DomainCopilot.Application.Interfaces.Ingestion;

public interface IDocumentContentStore
{
    Task<string> SaveAsync(
        Guid documentId,
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(
        string storedPath,
        CancellationToken cancellationToken = default);
}