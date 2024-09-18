using Hangfire;
using Hangfire.Sqlite.Services;
using Hangfire.Storage.SQLite;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHangfire(config => config
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSQLiteStorage("database/hangfire.db", new SQLiteStorageOptions
    {
        QueuePollInterval = TimeSpan.FromSeconds(5)
    }));

builder.Services.AddHangfireServer();

builder.Services.AddScoped<IJobService, JobService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHangfireDashboard();
app.UseHttpsRedirection();

app.MapGet("/job1", () =>
{
    var jobId = BackgroundJob.Enqueue<IJobService>((service) => service.ExecuteJob1());
    return $"Job {jobId} has been queued!";
});

app.MapGet("/job2", () =>
{
    var jobId = BackgroundJob.Enqueue<IJobService>((service) => service.ExecuteJob2());
    return $"Job {jobId} has been queued!";
});

var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
recurringJobManager.AddOrUpdate<IJobService>("job3", (service) => service.ExecuteJob3(), Cron.Minutely);

app.Run();