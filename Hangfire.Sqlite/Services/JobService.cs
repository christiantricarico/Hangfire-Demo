namespace Hangfire.Sqlite.Services;

public interface IJobService
{
    [AutomaticRetry(Attempts = 0)]
    Task ExecuteJob1();

    Task ExecuteJob2();
    Task ExecuteJob3();
}

public class JobService : IJobService
{
    private readonly ILogger<JobService> _logger;

    public JobService(ILogger<JobService> logger)
    {
        _logger = logger;
    }

    public async Task ExecuteJob1()
    {
        _logger.LogInformation("Job 1 started ...");
        await Task.Delay(5000);
        throw new Exception("Job 1 failed!");
        _logger.LogInformation("Job 1 finished!");
    }

    public async Task ExecuteJob2()
    {
        _logger.LogInformation("Job 2 started ...");
        await Task.Delay(5000);
        _logger.LogInformation("Job 2 finished!");
    }

    public async Task ExecuteJob3()
    {
        _logger.LogInformation("Job 3 started ...");
        await Task.Delay(5000);
        _logger.LogInformation("Job 3 finished!");
    }
}
