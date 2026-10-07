using DomainCopilot.Application.Interfaces.Ingestion;
using SentenceTransformers.MiniLM;

namespace DomainCopilot.Infrastructure.AI;

public class LocalEmbeddingProvider : IEmbeddingProvider, IDisposable
{
    private readonly SentenceEncoder _encoder;

    public LocalEmbeddingProvider()
    {
        _encoder = new SentenceEncoder();
    }

    public async Task<IReadOnlyList<float>> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException(
                "Text cannot be empty.",
                nameof(text));
        }

        cancellationToken.ThrowIfCancellationRequested();

        var embeddings = await _encoder.EncodeAsync(
            new[] { text });

        cancellationToken.ThrowIfCancellationRequested();

        return embeddings[0];
    }

    public void Dispose()
    {
        _encoder.Dispose();
    }

}