using DomainCopilot.Application.Interfaces.Ingestion;
using Microsoft.Extensions.Configuration;
using OpenAI.Embeddings;

namespace DomainCopilot.Infrastructure.AI;

public class OpenAIEmbeddingProvider : IEmbeddingProvider
{
    private readonly EmbeddingClient _client;

    public OpenAIEmbeddingProvider(IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "OpenAI API key is not configured.");
        }

        var model = configuration["OpenAI:EmbeddingModel"]
            ?? "text-embedding-3-small";

        _client = new EmbeddingClient(model, apiKey);
    }

    public async Task<IReadOnlyList<float>> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        var embedding = await _client.GenerateEmbeddingAsync(
            text,
            cancellationToken: cancellationToken);

        return embedding.Value.ToFloats().ToArray();
    }
}