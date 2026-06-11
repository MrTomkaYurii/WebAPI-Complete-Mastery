using Serilog;
using StructuredLogging.WebApi.Services;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    // Запис інформаційного повідомлення в лог при запуску сервера
    Log.Information("starting server.");
    var builder = WebApplication.CreateBuilder(args);

    // Конфігурація Serilog як основного логгера для додатку
    builder.Host.UseSerilog((context, loggerConfiguration) =>
    {
        // Додавання виводу логів у консоль для зручного відлагодження
        loggerConfiguration.WriteTo.Console();

        // Зчитування додаткових налаштувань Serilog з файлу конфігурації (appsettings.json)
        // Це дозволяє налаштовувати рівень логування, формат повідомлень та інші параметри
        loggerConfiguration.ReadFrom.Configuration(context.Configuration);
    });

    builder.Services.AddTransient<IDummyService, DummyService>();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Увімкнення автоматичного логування HTTP запитів і відповідей
    // Serilog автоматично записуватиме деталі кожного запиту (метод, URL, час виконання, статус код)
    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();
    app.MapGet("/", (IDummyService svc) => svc.DoSomething());
    app.Run();
}
catch (Exception ex)
{
    // Запис критичної помилки в лог, якщо сервер завершився аварійно
    // Log.Fatal використовується для найсерйозніших помилок, які призводять до зупинки додатку
    Log.Fatal(ex, "server terminated unexpectedly");
}
finally
{
    // Обов'язкове закриття та очищення ресурсів Serilog
    // Гарантує, що всі повідомлення будуть записані перед завершенням програми
    Log.CloseAndFlush();
}