using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using Svodka.Domain.Interfaces;
using Svodka.Infrastructure.Services;
using Svodka.Infrastructure.Data;
using Svodka.Infrastructure.Providers;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<NewsAggregatorDbContext>(options =>
    options.UseSqlServer(connectionString));

// HttpClient для RSS
builder.Services.AddHttpClient<IRssService, RssService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// HttpClient для GitHub
builder.Services.AddHttpClient<IGitHubService, GitHubService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.BaseAddress = new Uri("https://api.github.com");
});

// HttpClient для Reddit
builder.Services.AddHttpClient<IRedditService, RedditService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.BaseAddress = new Uri("https://www.reddit.com");
});

// Репозитории и сервисы
builder.Services.AddScoped<INewsItemRepository, NewsItemRepository>();
builder.Services.AddScoped<INewsSourceRepository, NewsSourceRepository>();
builder.Services.AddTransient<RssNewsProvider>();
builder.Services.AddTransient<GitHubNewsProvider>();
builder.Services.AddTransient<RedditNewsProvider>();
builder.Services.AddTransient<INewsProviderFactory, NewsProviderFactory>();
builder.Services.AddSingleton<INewsAggregationJob, NewsAggregationJob>();
builder.Services.AddHostedService<NewsAggregationBackgroundService>();

builder.Services.Configure<NewsAggregationOptions>(
    builder.Configuration.GetSection(NewsAggregationOptions.SectionName));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:3000",
                "http://localhost:3001",
                "http://localhost:8080",
                "http://localhost:8000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });

    options.AddPolicy("DevelopmentCORS", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173",
                "http://localhost:3000",
                "http://localhost:3001",
                "http://localhost:8080",
                "http://localhost:8000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Настройка JSON для контроллеров
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

var app = builder.Build();



// CORS до остального middleware
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevelopmentCORS");
}
else
{
    app.UseCors("AllowReactApp");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

// app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
