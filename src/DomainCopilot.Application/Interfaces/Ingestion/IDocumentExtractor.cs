namespace DomainCopilot.Application.Interfaces.Ingestion;

public interface IDocumentExtractor
{
    bool CanHandle(string fileExtension);

    Task<string> ExtractTextAsync(
        Stream documentStream,
        string fileName,
        CancellationToken cancellationToken = default);
}