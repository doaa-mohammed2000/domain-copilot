using DomainCopilot.Application.Interfaces;
using DomainCopilot.Infrastructure.Persistence;
using DomainCopilot.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using DomainCopilot.Application.Services;
using DomainCopilot.Application.Interfaces.Ingestion;
using DomainCopilot.Infrastructure.VectorStore;
using Qdrant.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

builder.Services.AddSingleton(new QdrantClient(
    builder.Configuration["Qdrant:Url"] ?? "http://localhost:6333"));

builder.Services.AddScoped<IVectorStore, QdrantVectorStore>();

builder.Services.AddDbContext<DomainCopilotDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<DocumentService>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapHealthChecks("/health");

app.Run();