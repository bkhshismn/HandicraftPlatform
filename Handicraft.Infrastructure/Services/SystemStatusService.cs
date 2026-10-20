using Handicraft.Application.Contracts.Services;
using Handicraft.Application.Services.Dtos;

namespace Handicraft.Infrastructure.Services;

/// <summary>
/// آمار منابع سرور را از پروک‌فایل‌های لینوکس می‌خواند. روی ویندوز
/// (محیط توسعه) مقادیر در دسترس صفر برمی‌گردند تا صفحه خطا ندهد.
/// </summary>
public sealed class SystemStatusService : ISystemStatusService
{
    private const double BytesInMb = 1024d * 1024d;
    private const double BytesInGb = 1024d * 1024d * 1024d;

    public async Task<SystemStatusDto> GetStatusAsync(
        CancellationToken cancellationToken = default)
    {
        var status = new SystemStatusDto
        {
            MachineName = Environment.MachineName,
            ServerTimeUtc = DateTime.UtcNow,
            ProcessorCount = Environment.ProcessorCount
        };

        status.CpuPercent =
            await GetCpuPercentAsync(cancellationToken);

        var (memoryTotal, memoryAvailable) =
            GetMemoryMb();

        status.MemoryTotalMb = memoryTotal;
        status.MemoryFreeMb = memoryAvailable;
        status.MemoryUsedMb =
            Math.Max(0, memoryTotal - memoryAvailable);

        var (diskTotal, diskFree) =
            GetDiskGb();

        status.DiskTotalGb = diskTotal;
        status.DiskFreeGb = diskFree;
        status.DiskUsedGb =
            Math.Max(0, diskTotal - diskFree);

        var (load1, load5, load15) =
            GetLoadAverage();

        status.Load1 = load1;
        status.Load5 = load5;
        status.Load15 = load15;

        status.UptimeHours = GetUptimeHours();

        return status;
    }

    // ============================================================
    // CPU
    // ============================================================

    private static async Task<double> GetCpuPercentAsync(
        CancellationToken cancellationToken)
    {
        var first = ReadCpuTimes();

        if (first is null)
            return 0;

        try
        {
            await Task.Delay(200, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return 0;
        }

        var second = ReadCpuTimes();

        if (second is null)
            return 0;

        var totalDelta =
            second.Value.Total - first.Value.Total;

        var idleDelta =
            second.Value.Idle - first.Value.Idle;

        if (totalDelta <= 0)
            return 0;

        var usage =
            (totalDelta - idleDelta) / (double)totalDelta * 100;

        return Math.Round(Math.Clamp(usage, 0, 100), 1);
    }

    private static (long Total, long Idle)? ReadCpuTimes()
    {
        try
        {
            var line = File.ReadLines("/proc/stat").FirstOrDefault();

            if (line is null || !line.StartsWith("cpu "))
                return null;

            var parts = line
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Skip(1)
                .Select(value => long.TryParse(value, out var parsed) ? parsed : 0)
                .ToArray();

            if (parts.Length < 4)
                return null;

            // user nice system idle iowait irq softirq steal
            var idle = parts[3] + (parts.Length > 4 ? parts[4] : 0);
            var total = parts.Sum();

            return (total, idle);
        }
        catch
        {
            return null;
        }
    }

    // ============================================================
    // Memory
    // ============================================================

    private static (double TotalMb, double AvailableMb) GetMemoryMb()
    {
        try
        {
            double totalKb = 0;
            double availableKb = 0;

            foreach (var line in File.ReadLines("/proc/meminfo"))
            {
                if (line.StartsWith("MemTotal:"))
                    totalKb = ParseKb(line);

                else if (line.StartsWith("MemAvailable:"))
                    availableKb = ParseKb(line);

                if (totalKb > 0 && availableKb > 0)
                    break;
            }

            return (totalKb / 1024d, availableKb / 1024d);
        }
        catch
        {
            return (0, 0);
        }
    }

    private static double ParseKb(string line)
    {
        var parts = line
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return parts.Length >= 2 &&
               double.TryParse(parts[1], out var value)
            ? value
            : 0;
    }

    // ============================================================
    // Disk
    // ============================================================

    private static (double TotalGb, double FreeGb) GetDiskGb()
    {
        try
        {
            var root =
                Path.GetPathRoot(AppContext.BaseDirectory) ?? "/";

            var drive = new DriveInfo(root);

            return (
                Math.Round(drive.TotalSize / BytesInGb, 2),
                Math.Round(drive.AvailableFreeSpace / BytesInGb, 2));
        }
        catch
        {
            return (0, 0);
        }
    }

    // ============================================================
    // Load average
    // ============================================================

    private static (double, double, double) GetLoadAverage()
    {
        try
        {
            var text = File.ReadAllText("/proc/loadavg").Trim();

            var parts = text
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 3)
                return (0, 0, 0);

            return (
                double.TryParse(parts[0], out var l1) ? l1 : 0,
                double.TryParse(parts[1], out var l5) ? l5 : 0,
                double.TryParse(parts[2], out var l15) ? l15 : 0);
        }
        catch
        {
            return (0, 0, 0);
        }
    }

    // ============================================================
    // Uptime
    // ============================================================

    private static double GetUptimeHours()
    {
        try
        {
            var text = File.ReadAllText("/proc/uptime").Trim();

            var first = text
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();

            if (double.TryParse(first, out var seconds))
                return Math.Round(seconds / 3600d, 1);
        }
        catch
        {
            // ignore
        }

        return Math.Round(
            TimeSpan.FromMilliseconds(
                Environment.TickCount64).TotalHours,
            1);
    }
}
