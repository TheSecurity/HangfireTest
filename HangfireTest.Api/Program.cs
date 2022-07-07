using Hangfire;
using Hangfire.SqlServer;
using HangfireTest.Api;
using HangfireTest.Api.Filters;
using HangfireTest.Api.Services;
using HangfireTest.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Configuration.AddEnvironmentVariables();

Console.WriteLine("Running in environment configuration: " + builder.Environment.EnvironmentName);

var connectionString = builder.Configuration.GetConnectionString("Database");
builder.Services.AddDbContext<ApplicationDbContext>(opt
    => opt.UseSqlServer(connectionString));

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true,
    }));

GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 3 });

builder.Services.AddHangfireServer();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<JobFactory>();

var app = builder.Build();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] {new DashboardAuthFilter()} 
});

if (app.Environment.IsDevelopment())
{
    Console.WriteLine("Enabling Swagger");
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();

    var jobFactory = services.GetRequiredService<JobFactory>();
    jobFactory.RegisterAllJobs();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHangfireDashboard();
});

app.MapControllers();
app.Run();
