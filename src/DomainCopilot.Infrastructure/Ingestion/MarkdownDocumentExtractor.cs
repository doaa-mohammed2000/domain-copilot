using System.Text;
using DomainCopilot.Application.Interfaces.Ingestion;

namespace DomainCopilot.Infrastructure.Ingestion;

public class MarkdownDocumentExtractor : IDocumentExtractor
{
    public bool CanHandle(string fileExtension)
    {
        return fileExtension.Equals(
                   ".md",
                   StringComparison.OrdinalIgnoreCase)
               || fileExtension.Equals(
                   ".markdown",
                   StringComparison.OrdinalIgnoreCase);
    }

    public async Task<string> ExtractTextAsync(
        Stream documentStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        if (!CanHandle(Path.GetExtension(fileName)))
        {
            throw new NotSupportedException(
                $"Unsupported document format: {Path.GetExtension(fileName)}");
        }

        using var reader = new StreamReader(
            documentStream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true);

        return await reader.ReadToEndAsync(cancellationToken);
    }
}