namespace DomainCopilot.Application.Interfaces.Ingestion;

public interface IEmbeddingProvider
{
    Task<IReadOnlyList<float>> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default);
}