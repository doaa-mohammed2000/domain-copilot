namespace DomainCopilot.Domain.Entities;

public class Document
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public string Format { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string ContentHash { get; set; } = string.Empty;

    public string Status { get; set; } = "Pending";

    public string? FailureReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}