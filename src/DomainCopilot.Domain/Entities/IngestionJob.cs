namespace DomainCopilot.Domain.Entities;

public class IngestionJob
{
    public Guid Id { get; set; }

    public Guid DocumentId { get; set; }

    public string Status { get; set; } = "Queued";

    public int ProgressPercentage { get; set; }

    public string? ErrorMessage { get; set; }

    public bool CancellationRequested { get; set; }

    public int RetryCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? IdempotencyKey { get; set; }

    public Document Document { get; set; } = null!;
}