using DomainCopilot.Application.Interfaces.Ingestion;

namespace DomainCopilot.Worker;

public class Worker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IServiceScopeFactory scopeFactory,
        ILogger<Worker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Domain Copilot ingestion worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var queue =
                    scope.ServiceProvider
                        .GetRequiredService<IIngestionQueue>();

                var processor =
                    scope.ServiceProvider
                        .GetRequiredService<IIngestionJobProcessor>();

                var jobId = await queue.DequeueAsync(
                    stoppingToken);

                if (jobId is null)
                {
                    continue;
                }

                _logger.LogInformation(
                    "Processing ingestion job {JobId}.",
                    jobId);

                try
                {
                    await processor.ProcessAsync(
                        jobId.Value,
                        stoppingToken);

                    _logger.LogInformation(
                        "Ingestion job {JobId} completed.",
                        jobId);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation(
                        "Worker shutdown requested.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Ingestion job {JobId} failed.",
                        jobId);
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error in ingestion worker.");

                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    stoppingToken);
            }
        }

        _logger.LogInformation(
            "Domain Copilot ingestion worker stopped.");
    }
}