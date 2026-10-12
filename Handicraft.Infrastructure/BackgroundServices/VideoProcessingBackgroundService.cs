using Handicraft.Application.Contracts.Persistence;
using Handicraft.Application.Contracts.Services;
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

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "========== VIDEO PROCESSING WORKER STARTED ==========");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope =
                    _scopeFactory.CreateScope();

                var episodeRepo =
                    scope.ServiceProvider
                        .GetRequiredService<ICourseEpisodeRepository>();

                var storage =
                    scope.ServiceProvider
                        .GetRequiredService<IVideoStorageService>();

                var processor =
                    scope.ServiceProvider
                        .GetRequiredService<IVideoProcessor>();

                _logger.LogInformation(
                    "[VIDEO WORKER] Checking pending episodes...");

                var episodesToCheck =
                    await episodeRepo.GetAsync(
                        e =>
                            e.Status == VideoProcessingStatus.Pending ||
                            e.Status == VideoProcessingStatus.Failed);

                var pendingEpisodes =
                    episodesToCheck
                        .Where(
                            episode =>
                                episode.Status == VideoProcessingStatus.Pending ||
                                (episode.Status == VideoProcessingStatus.Failed &&
                                 !string.IsNullOrWhiteSpace(episode.RawVideoFileName) &&
                                 File.Exists(
                                     storage.GetRawVideoPath(
                                         episode.LessonId,
                                         episode.Id,
                                         episode.RawVideoFileName))))
                        .ToList();

                _logger.LogInformation(
                    "[VIDEO WORKER] Eligible pending/retry count: {Count}",
                    pendingEpisodes.Count);

                foreach (var episode in pendingEpisodes)
                {
                    if (stoppingToken.IsCancellationRequested)
                        break;

                    _logger.LogInformation(
                        "========================================");

                    _logger.LogInformation(
                        "[VIDEO WORKER] Processing episode: {EpisodeId}",
                        episode.Id);

                    _logger.LogInformation(
                        "[VIDEO WORKER] LessonId: {LessonId}",
                        episode.LessonId);

                    _logger.LogInformation(
                        "[VIDEO WORKER] FileName: {FileName}",
                        episode.RawVideoFileName);

                    try
                    {
                        if (string.IsNullOrWhiteSpace(
                                episode.RawVideoFileName))
                        {
                            throw new InvalidOperationException(
                                "RawVideoFileName is empty.");
                        }

                        if (episode.Status == VideoProcessingStatus.Failed)
                        {
                            _logger.LogWarning(
                                "[VIDEO WORKER] Retrying failed episode {EpisodeId}",
                                episode.Id);
                        }

                        var rawPath =
                            storage.GetRawVideoPath(
                                episode.LessonId,
                                episode.Id,
                                episode.RawVideoFileName);

                        _logger.LogInformation(
                            "[VIDEO WORKER] Raw path: {RawPath}",
                            rawPath);

                        var rawExists =
                            File.Exists(rawPath);

                        _logger.LogInformation(
                            "[VIDEO WORKER] Raw file exists: {Exists}",
                            rawExists);

                        if (!rawExists)
                        {
                            throw new FileNotFoundException(
                                "Raw video file was not found.",
                                rawPath);
                        }

                        var rawInfo =
                            new FileInfo(rawPath);

                        _logger.LogInformation(
                            "[VIDEO WORKER] Raw file size: {Size} bytes",
                            rawInfo.Length);

                        // ---------------------------------
                        // Processing
                        // ---------------------------------

                        episode.Status =
                            VideoProcessingStatus.Processing;

                        await episodeRepo.UpdateAsync(episode);

                        _logger.LogInformation(
                            "[VIDEO WORKER] Status => Processing");

                        var hlsOutputDir =
                            storage.GetHlsOutputDirectory(
                                episode.LessonId,
                                episode.Id);

                        _logger.LogInformation(
                            "[VIDEO WORKER] HLS output directory: {HlsDirectory}",
                            hlsOutputDir);

                        _logger.LogInformation(
                            "[VIDEO WORKER] Starting FFmpeg...");

                        var result =
                            await processor.ConvertToHlsAsync(
                                rawPath,
                                hlsOutputDir,
                                stoppingToken);

                        _logger.LogInformation(
                            "[VIDEO WORKER] FFmpeg completed.");

                        _logger.LogInformation(
                            "[VIDEO WORKER] Playlist: {Playlist}",
                            result.MasterPlaylistRelativePath);

                        _logger.LogInformation(
                            "[VIDEO WORKER] Duration: {Duration} seconds",
                            result.DurationSeconds);

                        // ---------------------------------
                        // Validate generated HLS
                        // ---------------------------------

                        var masterPlaylistPath =
                            Path.Combine(
                                hlsOutputDir,
                                result.MasterPlaylistRelativePath);

                        _logger.LogInformation(
                            "[VIDEO WORKER] Master playlist path: {Path}",
                            masterPlaylistPath);

                        var playlistExists =
                            File.Exists(masterPlaylistPath);

                        _logger.LogInformation(
                            "[VIDEO WORKER] Master playlist exists: {Exists}",
                            playlistExists);

                        if (!playlistExists)
                        {
                            throw new InvalidOperationException(
                                $"FFmpeg completed but master playlist was not found: {masterPlaylistPath}");
                        }

                        // ---------------------------------
                        // Ready
                        // ---------------------------------

                        episode.VideoUrl =
                            result.MasterPlaylistRelativePath;

                        episode.DurationSeconds =
                            result.DurationSeconds;

                        episode.Status =
                            VideoProcessingStatus.Ready;

                        _logger.LogInformation(
                            "[VIDEO WORKER] Episode marked Ready for {EpisodeId}",
                            episode.Id);

                        await episodeRepo.UpdateAsync(episode);

                        _logger.LogInformation(
                            "[VIDEO WORKER] Status => Ready");

                        _logger.LogInformation(
                            "[VIDEO WORKER] VideoUrl: {VideoUrl}",
                            episode.VideoUrl);

                        _logger.LogInformation(
                            "[VIDEO WORKER] Episode {EpisodeId} processed successfully.",
                            episode.Id);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation(
                            "[VIDEO WORKER] Processing cancelled.");

                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "[VIDEO WORKER] FAILED processing episode {EpisodeId}",
                            episode.Id);

                        episode.Status =
                            VideoProcessingStatus.Failed;

                        await episodeRepo.UpdateAsync(episode);

                        _logger.LogInformation(
                            "[VIDEO WORKER] Status => Failed");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[VIDEO WORKER] Error in worker loop");
            }

            try
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation(
            "========== VIDEO PROCESSING WORKER STOPPED ==========");
    }
}