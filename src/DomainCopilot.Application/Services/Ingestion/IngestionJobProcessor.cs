using DomainCopilot.Application.Interfaces;
using DomainCopilot.Application.Interfaces.Ingestion;
using DomainCopilot.Domain.Entities;

namespace DomainCopilot.Application.Services.Ingestion;

public class IngestionJobProcessor : IIngestionJobProcessor
{
    private readonly IIngestionJobRepository _jobRepository;
    private readonly IDocumentChunkRepository _chunkRepository;
    private readonly IDocumentContentStore _contentStore;
    private readonly DocumentExtractorResolver _extractorResolver;
    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly IVectorStore _vectorStore;
    private readonly DocumentChunker _chunker;

    public IngestionJobProcessor(
        IIngestionJobRepository jobRepository,
        IDocumentChunkRepository chunkRepository,
        IDocumentContentStore contentStore,
        DocumentExtractorResolver extractorResolver,
        IEmbeddingProvider embeddingProvider,
        IVectorStore vectorStore,
        DocumentChunker chunker)
    {
        _jobRepository = jobRepository;
        _chunkRepository = chunkRepository;
        _contentStore = contentStore;
        _extractorResolver = extractorResolver;
        _embeddingProvider = embeddingProvider;
        _vectorStore = vectorStore;
        _chunker = chunker;
    }

    public async Task ProcessAsync(
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(
            jobId,
            cancellationToken);

        if (job is null)
        {
            throw new InvalidOperationException(
                $"Ingestion job '{jobId}' was not found.");
        }

        var document = job.Document;

        try
        {
            job.Status = "Processing";
            job.StartedAt = DateTime.UtcNow;
            job.UpdatedAt = DateTime.UtcNow;

            document.Status = "Processing";
            document.UpdatedAt = DateTime.UtcNow;

            await _jobRepository.SaveChangesAsync(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            await UpdateProgressAsync(job, 10, cancellationToken);

            await using var documentStream =
                await _contentStore.OpenReadAsync(
                    document.Source,
                    cancellationToken);

            var fileName = document.Title;

            var extractor = _extractorResolver.Resolve(fileName);

            var extractedText =
                await extractor.ExtractTextAsync(
                    documentStream,
                    fileName,
                    cancellationToken);

            if (string.IsNullOrWhiteSpace(extractedText))
            {
                throw new InvalidOperationException(
                    "The document contains no extractable text.");
            }

            await UpdateProgressAsync(job, 25, cancellationToken);

            var chunks = _chunker.Chunk(
                document.Id,
                extractedText);

            if (chunks.Count == 0)
            {
                throw new InvalidOperationException(
                    "No chunks were produced from the document.");
            }

            await _chunkRepository.DeleteByDocumentIdAsync(
                document.Id,
                cancellationToken);

            await UpdateProgressAsync(job, 35, cancellationToken);

            for (var i = 0; i < chunks.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var chunk = chunks[i];

                var embedding =
                    await _embeddingProvider.GenerateEmbeddingAsync(
                        chunk.Content,
                        cancellationToken);

                await _vectorStore.UpsertAsync(
                    chunk,
                    embedding,
                    cancellationToken);

                var progress = 35 +
                    (int)(((i + 1) / (double)chunks.Count) * 55);

                await UpdateProgressAsync(
                    job,
                    progress,
                    cancellationToken);
            }

            await _chunkRepository.AddRangeAsync(
                chunks,
                cancellationToken);

            await _chunkRepository.SaveChangesAsync(
                cancellationToken);

            job.Status = "Completed";
            job.ProgressPercentage = 100;
            job.CompletedAt = DateTime.UtcNow;
            job.UpdatedAt = DateTime.UtcNow;

            document.Status = "Completed";
            document.FailureReason = null;
            document.UpdatedAt = DateTime.UtcNow;

            await _jobRepository.SaveChangesAsync(
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            job.Status = "Cancelled";
            job.CancellationRequested = true;
            job.UpdatedAt = DateTime.UtcNow;

            document.Status = "Cancelled";
            document.UpdatedAt = DateTime.UtcNow;

            await _jobRepository.SaveChangesAsync(
                CancellationToken.None);

            throw;
        }
        catch (Exception ex)
        {
            job.Status = "Failed";
            job.ErrorMessage = ex.Message;
            job.UpdatedAt = DateTime.UtcNow;

            document.Status = "Failed";
            document.FailureReason = ex.Message;
            document.UpdatedAt = DateTime.UtcNow;

            await _jobRepository.SaveChangesAsync(
                CancellationToken.None);

            throw;
        }
    }

    private async Task UpdateProgressAsync(
        IngestionJob job,
        int progress,
        CancellationToken cancellationToken)
    {
        job.ProgressPercentage = progress;
        job.UpdatedAt = DateTime.UtcNow;

        await _jobRepository.SaveChangesAsync(
            cancellationToken);
    }
}