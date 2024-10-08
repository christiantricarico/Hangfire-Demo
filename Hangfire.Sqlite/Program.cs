using Hangfire;
using Hangfire.Sqlite.Dashboard;
using Hangfire.Sqlite.Services;
using Hangfire.Storage.SQLite;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSQLiteStorage("Database/hangfire.db", new SQLiteStorageOptions
    {
        QueuePollInterval = TimeSpan.FromSeconds(5)
    }));

builder.Services.AddHangfireServer();

builder.Services.AddScoped<IJobService, JobService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

var filter = new DashboardAuthorizationFilter(new DashboardAuthorizationFilterOptions
{
    RequireSsl = false,
    SslRedirect = false,
    LoginCaseSensitive = true,
    Users =
    [
        new DashboardAuthorizationUser
        {
            Login = "admin",
            PasswordClear =  "admin"
        }
    ]
});

app.UseHangfireDashboard(options: new DashboardOptions
{
    Authorization = [filter]
});

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