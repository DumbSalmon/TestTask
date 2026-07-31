using Microsoft.EntityFrameworkCore;
using Nazarov_Test_Task.Core.Data;
using NLog;
using NLog.Web;
using Nazarov_Test_Task.Middleware;
using Nazarov_Test_Task.Core.Services;

var logger = LogManager
    .Setup()
    .LoadConfigurationFromAppSettings()
    .GetCurrentClassLogger();

try
    {
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddCors(options =>
    {
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5500")
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
    });

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    var connectionString = builder.Configuration.GetConnectionString("Postgres");

    if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("CHANGE_ME"))
    {
        throw new InvalidOperationException(
            "Настройте строку подключения PostgreSQL. Посмотрите README.");
    }

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
    builder.Services.AddScoped<EmployeeService>();
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    

    app.UseMiddleware<ApiLoggingAndExceptionMiddleware>();

    app.UseHttpsRedirection();

    app.UseCors("FrontendPolicy");

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception exception)
{
    logger.Error(exception, "Приложение остановлено из-за необработанного исключения");
    throw;
}
finally
{
    LogManager.Shutdown();
}