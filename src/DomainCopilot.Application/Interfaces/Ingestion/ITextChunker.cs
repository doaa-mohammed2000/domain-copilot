namespace DomainCopilot.Application.Interfaces.Ingestion;

public interface ITextChunker
{
    IReadOnlyList<string> Chunk(
        string text,
        int chunkSize,
        int overlap);
}