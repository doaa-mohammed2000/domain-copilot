using DomainCopilot.Application.Interfaces.Ingestion;

namespace DomainCopilot.Application.Services.Ingestion;

public class DocumentExtractorResolver
{
    private readonly IEnumerable<IDocumentExtractor> _extractors;

    public DocumentExtractorResolver(
        IEnumerable<IDocumentExtractor> extractors)
    {
        _extractors = extractors;
    }

    public IDocumentExtractor Resolve(string fileName)
    {
        var extension = Path.GetExtension(fileName);

        var extractor = _extractors.FirstOrDefault(
            x => x.CanHandle(extension));

        return extractor
            ?? throw new NotSupportedException(
                $"No extractor is registered for format '{extension}'.");
    }
}