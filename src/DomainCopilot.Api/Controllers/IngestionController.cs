using System.Security.Cryptography;
using DomainCopilot.Application.Interfaces;
using DomainCopilot.Application.Interfaces.Ingestion;
using DomainCopilot.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace DomainCopilot.Api.Controllers;

[ApiController]
[Route("api/ingestion")]
public class IngestionController : ControllerBase
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IIngestionJobRepository _jobRepository;
    private readonly IDocumentContentStore _contentStore;
    private readonly IIngestionQueue _ingestionQueue;

    public IngestionController(
        IDocumentRepository documentRepository,
        IIngestionJobRepository jobRepository,
        IDocumentContentStore contentStore,
        IIngestionQueue ingestionQueue)
    {
        _documentRepository = documentRepository;
        _jobRepository = jobRepository;
        _contentStore = contentStore;
        _ingestionQueue = ingestionQueue;
    }

    [HttpPost("documents")]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> UploadDocument(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new
            {
                message = "A non-empty document file is required."
            });
        }

        var extension = Path.GetExtension(file.FileName);

        if (!extension.Equals(".txt", StringComparison.OrdinalIgnoreCase) &&
            !extension.Equals(".md", StringComparison.OrdinalIgnoreCase) &&
            !extension.Equals(".markdown", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Supported formats are .txt, .md and .markdown."
            });
        }

        await using var inputStream = file.OpenReadStream();

        using var sha256 = SHA256.Create();

        var hashBytes = await sha256.ComputeHashAsync(
            inputStream,
            cancellationToken);

        var contentHash = Convert.ToHexString(hashBytes);

        inputStream.Position = 0;

        var existingDocument =
            await _documentRepository.GetByContentHashAsync(
                contentHash,
                cancellationToken);

        if (existingDocument is not null)
        {
            return Conflict(new
            {
                message = "A document with the same content already exists.",
                documentId = existingDocument.Id
            });
        }

        var documentId = Guid.NewGuid();

        var storedPath = await _contentStore.SaveAsync(
            documentId,
            file.FileName,
            inputStream,
            cancellationToken);

        var now = DateTime.UtcNow;

        var document = new Document
        {
            Id = documentId,
            Title = Path.GetFileName(file.FileName),
            Source = storedPath,
            Format = extension.TrimStart('.').ToLowerInvariant(),
            Version = "1.0",
            ContentHash = contentHash,
            Status = "Queued",
            CreatedAt = now,
            UpdatedAt = now
        };

        await _documentRepository.AddAsync(
            document,
            cancellationToken);

        await _documentRepository.SaveChangesAsync(
            cancellationToken);

        var job = new IngestionJob
        {
            Id = Guid.NewGuid(),
            DocumentId = document.Id,
            Status = "Queued",
            ProgressPercentage = 0,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _jobRepository.AddAsync(
            job,
            cancellationToken);

        await _jobRepository.SaveChangesAsync(
            cancellationToken);

        await _ingestionQueue.EnqueueAsync(
            job.Id,
            cancellationToken);

        return Accepted(new
        {
            jobId = job.Id,
            documentId = document.Id,
            status = job.Status,
            progress = job.ProgressPercentage
        });
    }

    [HttpGet("jobs/{jobId:guid}")]
    public async Task<IActionResult> GetJob(
        Guid jobId,
        CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(
            jobId,
            cancellationToken);

        if (job is null)
        {
            return NotFound(new
            {
                message = "Ingestion job not found."
            });
        }

        return Ok(new
        {
            job.Id,
            job.DocumentId,
            job.Status,
            job.ProgressPercentage,
            job.ErrorMessage,
            job.CreatedAt,
            job.StartedAt,
            job.CompletedAt
        });
    }

    [HttpPost("jobs/{jobId:guid}/cancel")]
    public async Task<IActionResult> CancelJob(
        Guid jobId,
        CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(
            jobId,
            cancellationToken);

        if (job is null)
        {
            return NotFound(new
            {
                message = "Ingestion job not found."
            });
        }

        if (job.Status is "Completed" or "Failed" or "Cancelled")
        {
            return Conflict(new
            {
                message =
                    $"Job cannot be cancelled because it is already {job.Status}."
            });
        }

        job.CancellationRequested = true;
        job.UpdatedAt = DateTime.UtcNow;

        await _jobRepository.SaveChangesAsync(
            cancellationToken);

        return Accepted(new
        {
            jobId = job.Id,
            status = "CancellationRequested"
        });
    }
}