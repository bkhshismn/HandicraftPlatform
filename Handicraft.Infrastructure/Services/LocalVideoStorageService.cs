using Handicraft.Application.Contracts.Services;
using Microsoft.Extensions.Configuration;

namespace Handicraft.Application.Services;

public class LocalVideoStorageService : IVideoStorageService
{
    private readonly string _basePath;

    public LocalVideoStorageService(IConfiguration config)
    {
        var configuredPath = config["VideoStorage:BasePath"];

        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            var fallbackPath = Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                "..",
                "..",
                "VideoStorage");

            configuredPath = Path.GetFullPath(fallbackPath);
        }
        else if (!Path.IsPathRooted(configuredPath))
        {
            configuredPath = Path.GetFullPath(
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    configuredPath));
        }

        _basePath = configuredPath;
        Directory.CreateDirectory(_basePath);

        Console.WriteLine(
            $"[VIDEO STORAGE] BasePath: {_basePath}");
    }

    public async Task<string> SaveRawVideoAsync(
        Guid lessonId,
        Guid episodeId,
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        if (fileStream is null)
            throw new ArgumentNullException(nameof(fileStream));

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException(
                "File name is empty.",
                nameof(fileName));

        var folder = Path.Combine(
            _basePath,
            lessonId.ToString(),
            episodeId.ToString(),
            "raw");

        Directory.CreateDirectory(folder);

        var rawPath = GetContainedRawPath(folder, fileName);

        Console.WriteLine(
            $"[VIDEO STORAGE] Saving raw video: {rawPath}");

        await using var fs = new FileStream(
            rawPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 1024 * 1024,
            useAsync: true);

        await fileStream.CopyToAsync(
            fs,
            cancellationToken);

        await fs.FlushAsync(cancellationToken);

        Console.WriteLine(
            $"[VIDEO STORAGE] Saved: {rawPath}");

        Console.WriteLine(
            $"[VIDEO STORAGE] Exists: {File.Exists(rawPath)}");

        Console.WriteLine(
            $"[VIDEO STORAGE] Size: {new FileInfo(rawPath).Length} bytes");

        return rawPath;
    }

    public string GetRawVideoPath(
        Guid lessonId,
        Guid episodeId,
        string fileName)
    {
        var folder = Path.Combine(
            _basePath,
            lessonId.ToString(),
            episodeId.ToString(),
            "raw");

        return GetContainedRawPath(folder, fileName);
    }

    private static string GetContainedRawPath(string folder, string fileName)
    {
        // Treat both slash styles as separators, even when running on Windows.
        var normalizedName = fileName.Replace('\\', '/');
        if (string.IsNullOrWhiteSpace(normalizedName) ||
            normalizedName is "." or ".." ||
            normalizedName.Contains('/') ||
            Path.IsPathRooted(fileName))
        {
            throw new ArgumentException("Invalid raw video file name.", nameof(fileName));
        }

        var fullFolder = Path.GetFullPath(folder);
        var fullPath = Path.GetFullPath(Path.Combine(fullFolder, normalizedName));
        var folderPrefix = fullFolder.EndsWith(Path.DirectorySeparatorChar)
            ? fullFolder
            : fullFolder + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(folderPrefix, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Raw video path escapes its storage folder.", nameof(fileName));

        return fullPath;
    }

    public string GetHlsOutputDirectory(
        Guid lessonId,
        Guid episodeId)
    {
        var folder = Path.Combine(
            _basePath,
            lessonId.ToString(),
            episodeId.ToString(),
            "hls");

        Directory.CreateDirectory(folder);

        return folder;
    }

    public string GetHlsPhysicalFilePath(
        Guid lessonId,
        Guid episodeId,
        string relativePath)
    {
        return Path.Combine(
            GetHlsOutputDirectory(lessonId, episodeId),
            relativePath);
    }

    public string GetHlsPlaylistPath(
        Guid lessonId,
        Guid episodeId)
    {
        return Path.Combine(
            GetHlsOutputDirectory(lessonId, episodeId),
            "master.m3u8");
    }

    public Task DeleteEpisodeFilesAsync(
        Guid lessonId,
        Guid episodeId)
    {
        var folder = Path.Combine(
            _basePath,
            lessonId.ToString(),
            episodeId.ToString());

        if (Directory.Exists(folder))
        {
            Directory.Delete(
                folder,
                recursive: true);
        }

        return Task.CompletedTask;
    }
}
