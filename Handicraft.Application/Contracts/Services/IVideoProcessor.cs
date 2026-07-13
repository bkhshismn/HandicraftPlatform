namespace Handicraft.Application.Contracts.Services;

public record VideoConversionResult(string MasterPlaylistRelativePath, int DurationSeconds);

public interface IVideoProcessor
{
    Task<VideoConversionResult> ConvertToHlsAsync(string inputFilePath, string outputDirectory, CancellationToken cancellationToken = default);
}
