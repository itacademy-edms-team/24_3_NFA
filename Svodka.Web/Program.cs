using Microsoft.EntityFrameworkCore;
using Svodka.Domain.Interfaces;
using Svodka.Infrastructure.Services;
using Svoka.Infrastructure.Data;
using Svoka.Infrastructure.Providers;
using Svoka.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") // Убедитесь, что имя совпадает с appsettings.json
                           ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<NewsAggregatorDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddHttpClient<IRssService, RssService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30); // Пример настройки таймаута
    // User-Agent уже устанавливается в конструкторе RssService
});

// 4. Регистрация сервисов из Infrastructure
builder.Services.AddScoped<INewsItemRepository, NewsItemRepository>();
builder.Services.AddScoped<INewsSourceRepository, NewsSourceRepository>();


builder.Services.AddTransient<RssNewsProvider>();



builder.Services.AddHostedService<NewsAggregationBackgroundService>();

builder.Services.Configure<NewsAggregationOptions>(
    builder.Configuration.GetSection(NewsAggregationOptions.SectionName));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<NewsAggregatorDbContext>();
    try
    {
        context.Database.Migrate();
        Console.WriteLine("Миграции применены успешно.");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ошибка при применении миграций.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}



app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

public partial class Program { }