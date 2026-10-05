namespace DomainCopilot.Domain.Entities;

public class DocumentChunk
{
    public Guid Id { get; set; }

    public Guid DocumentId { get; set; }

    public string Content { get; set; } = string.Empty;

    public int ChunkIndex { get; set; }

    public int? PageNumber { get; set; }

    public string? Section { get; set; }

    public string? Clause { get; set; }

    public string? VectorId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Document Document { get; set; } = null!;
}