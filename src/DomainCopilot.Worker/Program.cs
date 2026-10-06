using DomainCopilot.Application.Interfaces.Ingestion;
using DomainCopilot.Application.Services.Ingestion;
using DomainCopilot.Infrastructure.AI;
using DomainCopilot.Infrastructure.Ingestion;
using DomainCopilot.Infrastructure.Persistence;
using DomainCopilot.Infrastructure.Queue;
using DomainCopilot.Infrastructure.Repositories;
using DomainCopilot.Infrastructure.Storage;
using DomainCopilot.Infrastructure.VectorStore;
using DomainCopilot.Worker;
using Microsoft.EntityFrameworkCore;
using Qdrant.Client;
using StackExchange.Redis;


var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<DomainCopilotDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var qdrantUrl = new Uri(
    builder.Configuration["Qdrant:Url"]
        ?? "http://localhost:6333");

builder.Services.AddSingleton(new QdrantClient(
    qdrantUrl.Host,
    qdrantUrl.Port,
    qdrantUrl.Scheme.Equals(
        "https",
        StringComparison.OrdinalIgnoreCase)));


builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(
        builder.Configuration["Redis:ConnectionString"]
        ?? "localhost:6379"));

builder.Services.AddScoped<IDocumentChunkRepository, DocumentChunkRepository>();
builder.Services.AddScoped<IIngestionJobRepository, IngestionJobRepository>();
builder.Services.AddScoped<IDocumentContentStore, LocalDocumentContentStore>();

builder.Services.AddScoped<IEmbeddingProvider, OpenAIEmbeddingProvider>();
builder.Services.AddScoped<IVectorStore, QdrantVectorStore>();

builder.Services.AddScoped<IDocumentExtractor, TextDocumentExtractor>();
builder.Services.AddScoped<IDocumentExtractor, MarkdownDocumentExtractor>();

builder.Services.AddScoped<DocumentExtractorResolver>();
builder.Services.AddScoped<DocumentChunker>();
builder.Services.AddScoped<IIngestionJobProcessor, IngestionJobProcessor>();

builder.Services.AddSingleton<IIngestionQueue, RedisIngestionQueue>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

await host.RunAsync();