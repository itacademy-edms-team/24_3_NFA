using Microsoft.EntityFrameworkCore;
using Svodka.Domain.Interfaces;
using Svodka.Infrastructure.Services;
using Svodka.Infrastructure.Data;
using Svodka.Infrastructure.Providers;
using Svodka.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                           ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<NewsAggregatorDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddHttpClient<IRssService, RssService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30); 
});


builder.Services.AddScoped<INewsItemRepository, NewsItemRepository>();
builder.Services.AddScoped<INewsSourceRepository, NewsSourceRepository>();
builder.Services.AddTransient<RssNewsProvider>();
builder.Services.AddTransient<INewsProviderFactory, NewsProviderFactory>();

builder.Services.AddHostedService<NewsAggregationBackgroundService>();

builder.Services.Configure<NewsAggregationOptions>(
    builder.Configuration.GetSection(NewsAggregationOptions.SectionName));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<NewsAggregatorDbContext>();
    try
    {
        context.Database.Migrate();
        Console.WriteLine("�������� ��������� �������.");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "������ ��� ���������� ��������.");
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

//app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }