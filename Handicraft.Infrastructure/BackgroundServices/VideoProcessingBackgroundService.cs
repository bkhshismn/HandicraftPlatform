using Handicraft.Application.Contracts.Persistence;
using Handicraft.Application.Contracts.Services;
using Handicraft.Application.Services;
using Handicraft.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Handicraft.Infrastructure.BackgroundServices;

public class VideoProcessingBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<VideoProcessingBackgroundService> _logger;

    public VideoProcessingBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<VideoProcessingBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var episodeRepo = scope.ServiceProvider
                    .GetRequiredService<ICourseEpisodeRepository>();
                var storage = scope.ServiceProvider
                    .GetRequiredService<IVideoStorageService>();
                var processor = scope.ServiceProvider
                    .GetRequiredService<IVideoProcessor>();

                var pendingEpisodes = await episodeRepo.GetAsync(
                    e => e.Status == VideoProcessingStatus.Pending);

                foreach (var episode in pendingEpisodes)
                {
                    episode.Status = VideoProcessingStatus.Processing;
                    await episodeRepo.UpdateAsync(episode);

                    try
                    {
                        var rawPath = storage.GetRawVideoPath(
                            episode.CourseId, episode.Id, episode.RawVideoFileName!);

                        var hlsOutputDir = storage.GetHlsOutputDirectory(
                            episode.CourseId, episode.Id);

                        var (masterPlaylistRelativePath, durationSeconds) =
                            await processor.ConvertToHlsAsync(rawPath, hlsOutputDir, stoppingToken);

                        episode.VideoUrl = masterPlaylistRelativePath;
                        episode.DurationSeconds = durationSeconds;
                        episode.Status = VideoProcessingStatus.Ready;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process episode {EpisodeId}", episode.Id);
                        episode.Status = VideoProcessingStatus.Failed;
                    }

                    await episodeRepo.UpdateAsync(episode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in VideoProcessingBackgroundService loop");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
