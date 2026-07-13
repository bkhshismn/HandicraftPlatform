using Handicraft.Application.Contracts.Services;
using Microsoft.Extensions.Configuration;

namespace Handicraft.Application.Services
{
    public class LocalVideoStorageService : IVideoStorageService
    {
        private readonly string _basePath;

        public LocalVideoStorageService(IConfiguration config)
        {
            _basePath = config["VideoStorage:BasePath"] ?? "/data/videos";
        }

        public async Task<string> SaveRawVideoAsync(Guid courseId, Guid episodeId, Stream fileStream, string fileName, CancellationToken cancellationToken = default)
        {
            var folder = Path.Combine(_basePath, courseId.ToString(), episodeId.ToString(), "raw");
            Directory.CreateDirectory(folder);

            var rawPath = Path.Combine(folder, fileName);
            await using var fs = new FileStream(rawPath, FileMode.Create);
            await fileStream.CopyToAsync(fs, cancellationToken);

            return rawPath;
        }

        public string GetRawVideoPath(Guid courseId, Guid episodeId, string fileName)
            => Path.Combine(_basePath, courseId.ToString(), episodeId.ToString(), "raw", fileName);

        public string GetHlsOutputDirectory(Guid courseId, Guid episodeId)
        {
            var folder = Path.Combine(_basePath, courseId.ToString(), episodeId.ToString(), "hls");
            Directory.CreateDirectory(folder);
            return folder;
        }

        public string GetHlsPhysicalFilePath(Guid courseId, Guid episodeId, string relativePath)
            => Path.Combine(GetHlsOutputDirectory(courseId, episodeId), relativePath);

        public string GetHlsPlaylistPath(Guid courseId, Guid episodeId)
            => Path.Combine(GetHlsOutputDirectory(courseId, episodeId), "master.m3u8");

        public Task DeleteEpisodeFilesAsync(Guid courseId, Guid episodeId)
        {
            var folder = Path.Combine(_basePath, courseId.ToString(), episodeId.ToString());
            if (Directory.Exists(folder))
                Directory.Delete(folder, recursive: true);

            return Task.CompletedTask;
        }
    }
}
