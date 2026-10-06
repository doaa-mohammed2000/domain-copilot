using System.Text.RegularExpressions;
using DomainCopilot.Domain.Entities;

namespace DomainCopilot.Application.Services.Ingestion;

public class DocumentChunker
{
    private const int ChunkSize = 700;
    private const int Overlap = 100;

    public IReadOnlyList<DocumentChunk> Chunk(
        Guid documentId,
        string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<DocumentChunk>();
        }

        var cleanedText = CleanText(text);

        var sections = SplitIntoSections(cleanedText);

        var chunks = new List<DocumentChunk>();
        var chunkIndex = 0;

        foreach (var section in sections)
        {
            var words = section.Content
                .Split(
                    (char[]?)null,
                    StringSplitOptions.RemoveEmptyEntries);

            if (words.Length == 0)
            {
                continue;
            }

            var start = 0;

            while (start < words.Length)
            {
                var length = Math.Min(
                    ChunkSize,
                    words.Length - start);

                var content = string.Join(
                    ' ',
                    words.Skip(start).Take(length));

                chunks.Add(new DocumentChunk
                {
                    Id = CreateDeterministicId(
                        documentId,
                        chunkIndex),
                    DocumentId = documentId,
                    Content = content,
                    ChunkIndex = chunkIndex,
                    Section = section.Title,
                    CreatedAt = DateTime.UtcNow
                });

                chunkIndex++;

                if (start + length >= words.Length)
                {
                    break;
                }

                start += ChunkSize - Overlap;
            }
        }

        return chunks;
    }

    private static string CleanText(string text)
    {
        text = text.Replace("\r\n", "\n");
        text = text.Replace('\r', '\n');

        text = Regex.Replace(
            text,
            @"[ \t]+",
            " ");

        text = Regex.Replace(
            text,
            @"\n{3,}",
            "\n\n");

        return text.Trim();
    }

    private static IReadOnlyList<DocumentSection> SplitIntoSections(
        string text)
    {
        var lines = text.Split('\n');

        var sections = new List<DocumentSection>();

        string? currentTitle = null;
        var currentContent = new List<string>();

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            if (trimmed.StartsWith("#"))
            {
                AddSection(
                    sections,
                    currentTitle,
                    currentContent);

                currentTitle = trimmed.TrimStart('#').Trim();
                currentContent.Clear();
                continue;
            }

            currentContent.Add(trimmed);
        }

        AddSection(
            sections,
            currentTitle,
            currentContent);

        return sections;
    }

    private static void AddSection(
        List<DocumentSection> sections,
        string? title,
        List<string> content)
    {
        var text = string.Join(
            '\n',
            content.Where(x => !string.IsNullOrWhiteSpace(x)));

        if (!string.IsNullOrWhiteSpace(text))
        {
            sections.Add(
                new DocumentSection(title, text));
        }
    }

    private static Guid CreateDeterministicId(
        Guid documentId,
        int chunkIndex)
    {
        var bytes = documentId.ToByteArray();

        var indexBytes = BitConverter.GetBytes(chunkIndex);

        for (var i = 0; i < indexBytes.Length; i++)
        {
            bytes[i] ^= indexBytes[i];
        }

        return new Guid(bytes);
    }

    private sealed record DocumentSection(
        string? Title,
        string Content);
}