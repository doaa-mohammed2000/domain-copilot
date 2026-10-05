using DomainCopilot.Application.Interfaces.Ingestion;
using DomainCopilot.Domain.Entities;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace DomainCopilot.Infrastructure.VectorStore;

public class QdrantVectorStore : IVectorStore
{
    private const string CollectionName = "document_chunks";

    private readonly QdrantClient _client;

    public QdrantVectorStore(QdrantClient client)
    {
        _client = client;
    }

    public async Task UpsertAsync(
        DocumentChunk chunk,
        IReadOnlyList<float> embedding,
        CancellationToken cancellationToken = default)
    {
        if (embedding.Count == 0)
        {
            throw new ArgumentException(
                "Embedding cannot be empty.",
                nameof(embedding));
        }

        await EnsureCollectionAsync(
            embedding.Count,
            cancellationToken);

        await _client.UpsertAsync(
            collectionName: CollectionName,
            points: new[]
            {
                new PointStruct
                {
                    Id = new PointId
                    {
                        Uuid = chunk.Id.ToString()
                    },
                    Vectors = embedding.ToArray(),
                    Payload =
                    {
                        ["document_id"] = chunk.DocumentId.ToString(),
                        ["content"] = chunk.Content,
                        ["chunk_index"] = chunk.ChunkIndex,
                        ["page_number"] = chunk.PageNumber,
                        ["section"] = chunk.Section,
                        ["clause"] = chunk.Clause
                    }
                }
            },
            cancellationToken: cancellationToken);

        chunk.VectorId = chunk.Id.ToString();
    }

    private async Task EnsureCollectionAsync(
        int vectorSize,
        CancellationToken cancellationToken)
    {
        var collections = await _client.ListCollectionsAsync(
            cancellationToken);

        if (collections.Contains(CollectionName))
        {
            return;
        }

        await _client.CreateCollectionAsync(
            collectionName: CollectionName,
            vectorsConfig: new VectorParams
            {
                Size = (ulong)vectorSize,
                Distance = Distance.Cosine
            },
            cancellationToken: cancellationToken);
    }
}