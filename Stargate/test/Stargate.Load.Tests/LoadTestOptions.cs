namespace Stargate.Load.Tests;

public class LoadTestOptions
{
    public string BaseUrl { get; set; } = null!;
    public int RequestCount { get; set; } = 10;
    public int Rate { get; set; } = 10;
    public int IntervalSeconds { get; set; } = 1;
    public int DurationSeconds { get; set; } = 30;
}
