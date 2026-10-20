using System.Diagnostics;
using Handicraft.Application.Contracts.Services;

namespace Handicraft.Infrastructure.Services;

public class FfmpegVideoProcessor : IVideoProcessor
{
    public async Task<VideoConversionResult> ConvertToHlsAsync(string inputFilePath, string outputDirectory, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(outputDirectory);

        var masterPlaylistPath = Path.Combine(outputDirectory, "master.m3u8");

        var ffmpegArgs =
            $"-i \"{inputFilePath}\" " +
            "-filter_complex \"[0:v]split=3[v1][v2][v3];" +
            "[v1]scale=w=640:h=360:force_original_aspect_ratio=decrease:force_divisible_by=2,setsar=1[v1out];" +
            "[v2]scale=w=854:h=480:force_original_aspect_ratio=decrease:force_divisible_by=2,setsar=1[v2out];" +
            "[v3]scale=w=1280:h=720:force_original_aspect_ratio=decrease:force_divisible_by=2,setsar=1[v3out]\" " +
            "-map [v1out] -c:v:0 libx264 -b:v:0 800k " +
            "-map [v2out] -c:v:1 libx264 -b:v:1 1400k " +
            "-map [v3out] -c:v:2 libx264 -b:v:2 2800k " +
            "-pix_fmt:v:0 yuv420p -pix_fmt:v:1 yuv420p -pix_fmt:v:2 yuv420p " +
            "-map a:0? -map a:0? -map a:0? " +
            // AAC in MPEG-TS is supported by HLS.js across Android Chrome,
            // Safari, and desktop browsers; MP3-in-TS is not consistently decoded.
            "-c:a aac -b:a 128k -ar:a 48000 -ac:a 2 " +
            "-var_stream_map \"v:0,a:0 v:1,a:1 v:2,a:2\" " +
            "-f hls -hls_time 6 -hls_playlist_type vod " +
            $"-master_pl_name master.m3u8 " +
            $"-hls_segment_filename \"{outputDirectory}/v%v/segment%d.ts\" " +
            $"\"{outputDirectory}/v%v/playlist.m3u8\"";

        await RunProcessAsync("ffmpeg", ffmpegArgs, cancellationToken);

        var duration = await GetDurationSecondsAsync(inputFilePath, cancellationToken);

        return new VideoConversionResult("master.m3u8", duration);
    }

    private static async Task<int> GetDurationSecondsAsync(string inputFilePath, CancellationToken cancellationToken)
    {
        var args = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{inputFilePath}\"";
        var output = await RunProcessAsync("ffprobe", args, cancellationToken, captureOutput: true);

        return double.TryParse(output.Trim(), out var seconds) ? (int)seconds : 0;
    }

    private static async Task<string> RunProcessAsync(string fileName, string arguments, CancellationToken cancellationToken, bool captureOutput = false)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)!;

        var stdOutTask = process.StandardOutput.ReadToEndAsync();
        var stdErrTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync(cancellationToken);

        var stdOut = await stdOutTask;
        var stdErr = await stdErrTask;

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"{fileName} failed: {stdErr}");

        return captureOutput ? stdOut : stdErr;
    }
}
