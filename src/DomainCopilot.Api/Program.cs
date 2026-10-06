using DomainCopilot.Application.Interfaces;
using DomainCopilot.Infrastructure.Persistence;
using DomainCopilot.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using DomainCopilot.Application.Services;
using DomainCopilot.Application.Interfaces.Ingestion;
using DomainCopilot.Infrastructure.VectorStore;
using Qdrant.Client;
using DomainCopilot.Infrastructure.AI;
using DomainCopilot.Infrastructure.Storage;
using DomainCopilot.Infrastructure.Ingestion;
using DomainCopilot.Application.Services.Ingestion;
using StackExchange.Redis;
using DomainCopilot.Infrastructure.Queue;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(
        builder.Configuration["Redis:ConnectionString"]
        ?? "localhost:6379"));

builder.Services.AddSingleton<IIngestionQueue, RedisIngestionQueue>();
builder.Services.AddScoped<IDocumentContentStore, LocalDocumentContentStore>();

builder.Services.AddSingleton(new QdrantClient(
    builder.Configuration["Qdrant:Url"] ?? "http://localhost:6333"));

builder.Services.AddScoped<IVectorStore, QdrantVectorStore>();

builder.Services.AddDbContext<DomainCopilotDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IDocumentChunkRepository, DocumentChunkRepository>();
builder.Services.AddScoped<IIngestionJobRepository, IngestionJobRepository>();

builder.Services.AddScoped<DocumentService>();
builder.Services.AddScoped<IEmbeddingProvider, OpenAIEmbeddingProvider>();

builder.Services.AddScoped<IDocumentExtractor, TextDocumentExtractor>();
builder.Services.AddScoped<IDocumentExtractor, MarkdownDocumentExtractor>();

builder.Services.AddScoped<DocumentExtractorResolver>();
builder.Services.AddScoped<DocumentChunker>();
builder.Services.AddScoped<IIngestionJobProcessor, IngestionJobProcessor>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapHealthChecks("/health");
app.MapControllers();
app.Run();