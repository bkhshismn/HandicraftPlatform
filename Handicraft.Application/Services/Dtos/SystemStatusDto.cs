namespace Handicraft.Application.Services.Dtos;

public sealed class SystemStatusDto
{
    public string MachineName { get; set; } = string.Empty;

    public DateTime ServerTimeUtc { get; set; }

    public double CpuPercent { get; set; }

    public int ProcessorCount { get; set; }

    public double Load1 { get; set; }

    public double Load5 { get; set; }

    public double Load15 { get; set; }

    public double MemoryTotalMb { get; set; }

    public double MemoryUsedMb { get; set; }

    public double MemoryFreeMb { get; set; }

    public double DiskTotalGb { get; set; }

    public double DiskUsedGb { get; set; }

    public double DiskFreeGb { get; set; }

    public double UptimeHours { get; set; }
}
