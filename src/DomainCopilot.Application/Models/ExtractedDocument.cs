namespace DomainCopilot.Application.Models;

public class ExtractedDocument
{
    public List<ExtractedSection> Sections { get; set; } = [];
}

public class ExtractedSection
{
    public string? Section { get; set; }

    public string Content { get; set; } = string.Empty;
}