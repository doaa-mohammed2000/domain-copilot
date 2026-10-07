using DomainCopilot.Application.Interfaces.Ingestion;
using Microsoft.Extensions.Configuration;

namespace DomainCopilot.Infrastructure.Storage;

public class LocalDocumentContentStore : IDocumentContentStore
{
    private readonly string _rootPath;

    public LocalDocumentContentStore(IConfiguration configuration)
    {
        _rootPath = configuration["Storage:DocumentsPath"]
            ?? Path.Combine(AppContext.BaseDirectory, "data", "documents");

        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(
        Guid documentId,
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);

        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new ArgumentException(
                "The document must have a file extension.",
                nameof(fileName));
        }

        var documentDirectory = Path.Combine(
            _rootPath,
            documentId.ToString());

        Directory.CreateDirectory(documentDirectory);

        var storedPath = Path.Combine(
            documentDirectory,
            $"source{extension.ToLowerInvariant()}");

        await using var fileStream = new FileStream(
            storedPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        await content.CopyToAsync(
            fileStream,
            cancellationToken);
        return storedPath;
    }

      public Task<Stream> OpenReadAsync(
        string storedPath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string resolvedPath;

        if (Path.IsPathRooted(storedPath))
        {
            resolvedPath = storedPath;
        }
        else
        {
            var normalizedPath = storedPath
                .Replace('\\', '/');

            const string marker = "data/documents/";

            var markerIndex = normalizedPath.IndexOf(
                marker,
                StringComparison.OrdinalIgnoreCase);

            if (markerIndex >= 0)
            {
                var relativePath = normalizedPath[(markerIndex + marker.Length)..];

                resolvedPath = Path.Combine(
                    _rootPath,
                    relativePath.Replace('/', Path.DirectorySeparatorChar));
            }
            else
            {
                resolvedPath = Path.Combine(
                    _rootPath,
                    normalizedPath.Replace('/', Path.DirectorySeparatorChar));
            }
        }

        if (!File.Exists(resolvedPath))
        {
            throw new FileNotFoundException(
                "Document file was not found.",
                resolvedPath);
        }

        Stream stream = new FileStream(
            resolvedPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            useAsync: true);

        return Task.FromResult(stream);
    }
}