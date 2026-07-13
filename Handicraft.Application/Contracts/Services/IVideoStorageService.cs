namespace Handicraft.Application.Contracts.Services;

public interface IVideoStorageService
{
    Task<string> SaveRawVideoAsync(Guid courseId, Guid episodeId, Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    string GetRawVideoPath(Guid courseId, Guid episodeId, string fileName);
    string GetHlsOutputDirectory(Guid courseId, Guid episodeId);
    string GetHlsPhysicalFilePath(Guid courseId, Guid episodeId, string relativePath);
    string GetHlsPlaylistPath(Guid courseId, Guid episodeId);
    Task DeleteEpisodeFilesAsync(Guid courseId, Guid episodeId);
}
