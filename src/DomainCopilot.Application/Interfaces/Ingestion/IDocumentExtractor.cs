namespace DomainCopilot.Application.Interfaces.Ingestion;

public interface IDocumentExtractor
{
    Task<string> ExtractTextAsync(
        Stream documentStream,
        string fileName,
        CancellationToken cancellationToken = default);
}