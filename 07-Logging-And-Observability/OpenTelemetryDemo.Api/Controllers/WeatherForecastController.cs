using Microsoft.AspNetCore.Mvc;
using OpenTelemetryDemo.Api.Services;
using System.Diagnostics;

namespace OpenTelemetryDemo.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly TelemetryService _telemetry;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, TelemetryService telemetry)
    {
        _logger = logger;
        _telemetry = telemetry;
    }

    /// <summary>
    /// Gets weather forecast for specified number of days
    /// </summary>
    /// <param name="days">Number of days to forecast (1-30)</param>
    /// <returns>Array of weather forecasts</returns>
    /// <response code="200">Returns the weather forecast</response>
    /// <response code="400">If days parameter is invalid</response>
    [HttpGet(Name = "GetWeatherForecast")]
    [ProducesResponseType(typeof(IEnumerable<WeatherForecast>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IEnumerable<WeatherForecast> Get([FromQuery] int days = 5)
    {
        // ========== ТРЕЙСИНГ: Початок Activity (span) ==========
        // Створює span з назвою "GetWeatherForecast" для distributed tracing
        // Перевірити: Aspire Dashboard → Traces → знайди "GetWeatherForecast"
        using var activity = _telemetry.ActivitySource.StartActivity("GetWeatherForecast");

        // Додає тег (мітку) до спану - для фільтрації в системі моніторингу
        // Перевірити: В Aspire Dashboard → Traces → відкрий span → бачиш "forecast.days: 5"
        activity?.SetTag("forecast.days", days);

        // ========== ЛОГУВАННЯ ==========
        // Структуроване логування - {Days} стає окремим полем
        // Перевірити: Aspire Dashboard → Logs → фільтр по "Generating weather forecast"
        _logger.LogInformation("Generating weather forecast for {Days} days", days);

        // ========== МЕТРИКИ: Лічильник запитів ==========
        // Збільшує counter на 1 з мітками endpoint та days
        // Перевірити: Aspire Dashboard → Metrics → пошук "api.requests" → 
        // фільтр по dimensions: endpoint="get_forecast", days="5"
        _telemetry.RequestCounter.Add(1,
            new KeyValuePair<string, object?>("endpoint", "get_forecast"),
            new KeyValuePair<string, object?>("days", days.ToString()));

        // ========== МЕТРИКИ: Вимірювання тривалості ==========
        // Запускає таймер для запису часу виконання методу
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Валідація параметрів
            if (days < 1 || days > 30)
            {
                // Логування попередження
                // Перевірити: Aspire Dashboard → Logs → Level: Warning
                _logger.LogWarning("Invalid number of days requested: {Days}", days);

                // ========== ТРЕЙСИНГ: Позначення помилки ==========
                // Встановлює статус Error для спану - він буде червоним в UI
                // Перевірити: Aspire Dashboard → Traces → спани з червоним статусом
                activity?.SetStatus(ActivityStatusCode.Error, "Invalid number of days");

                throw new ArgumentException("Days must be between 1 and 30");
            }

            var forecasts = Enumerable.Range(1, days).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

            // Логування успішного результату
            // Перевірити: Aspire Dashboard → Logs → фільтр "Successfully generated"
            _logger.LogInformation("Successfully generated {Count} weather forecasts", forecasts.Length);

            // ========== ТРЕЙСИНГ: Додавання результату ==========
            // Додає тег з кількістю згенерованих прогнозів
            // Перевірити: Aspire Dashboard → Traces → відкрий span → бачиш "forecast.count: 5"
            activity?.SetTag("forecast.count", forecasts.Length);

            return forecasts;
        }
        finally
        {
            // ========== МЕТРИКИ: Запис тривалості запиту ==========
            // Finally виконується ЗАВЖДИ (навіть при помилці)
            stopwatch.Stop();

            // Записує histogram метрику - скільки мілісекунд виконувався метод
            // Перевірити: Aspire Dashboard → Metrics → пошук "api.request.duration" →
            // бачиш P50, P95, P99 percentiles (медіанний, 95% та 99% час відгуку)
            // Можна фільтрувати по endpoint="get_forecast"
            _telemetry.RequestDuration.Record(
                stopwatch.ElapsedMilliseconds,
                new KeyValuePair<string, object?>("endpoint", "get_forecast"));
        }
        // ========== ЯК ПЕРЕВІРИТИ В ASPIRE DASHBOARD ==========
        // 
        // 1. Запусти AppHost проєкт:
        //    dotnet run --project YourProject.AppHost
        //
        // 2. Відкрий браузер: http://localhost:15888
        //
        // 3. TRACES (трейсинг):
        //    - Вкладка "Traces"
        //    - Знайди "GetWeatherForecast"
        //    - Відкрий span → бачиш теги: forecast.days, forecast.count
        //    - Бачиш тривалість (200ms)
        //    - Якщо була помилка → span червоний
        //
        // 4. METRICS (метрики):
        //    - Вкладка "Metrics"
        //    - Пошук: "api.requests" → бачиш кількість запитів
        //    - Фільтр dimensions: endpoint="get_forecast", days="5"
        //    - Пошук: "api.request.duration" → бачиш графік часу відгуку
        //    - Можна побачити P50 (медіана), P95, P99 percentiles
        //
        // 5. LOGS (логи):
        //    - Вкладка "Logs"
        //    - Фільтр по сервісу: "api"
        //    - Пошук: "Generating weather forecast"
        //    - Level: Information / Warning / Error
    }

    /// <summary>
    /// Gets weather forecast for a specific date
    /// </summary>
    /// <param name="date">Date to get forecast for (must be future date)</param>
    /// <returns>Weather forecast for the specified date</returns>
    /// <response code="200">Returns the weather forecast</response>
    /// <response code="400">If date is in the past</response>
    [HttpGet("{date}")]
    [ProducesResponseType(typeof(WeatherForecast), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WeatherForecast>> GetByDate(DateOnly date)
    {
        // ========== ТРЕЙСИНГ: Початок Activity ==========
        // Створює span "GetWeatherByDate" для відстеження цього запиту
        // Перевірити: Aspire Dashboard → Traces → пошук "GetWeatherByDate"
        using var activity = _telemetry.ActivitySource.StartActivity("GetWeatherByDate");

        // Додає тег з датою для фільтрації трейсів
        // Перевірити: Aspire Dashboard → Traces → відкрий span → бачиш "forecast.date: 2025-10-20"
        activity?.SetTag("forecast.date", date.ToString());

        // ========== ЛОГУВАННЯ ==========
        // Структуроване логування з параметром {Date}
        // Перевірити: Aspire Dashboard → Logs → пошук "Getting weather forecast for date"
        _logger.LogInformation("Getting weather forecast for date: {Date}", date);

        // ========== МЕТРИКИ: Лічильник запитів ==========
        // Збільшує counter для endpoint="get_by_date"
        // Перевірити: Aspire Dashboard → Metrics → пошук "api.requests" → 
        // dimension filter: endpoint="get_by_date"
        _telemetry.RequestCounter.Add(1,
            new KeyValuePair<string, object?>("endpoint", "get_by_date"));

        // ========== МЕТРИКИ: Вимірювання тривалості ==========
        // Запускає таймер для запису часу виконання
        // Перевірити: Aspire Dashboard → Metrics → пошук "api.request.duration"
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Симуляція затримки (наприклад, виклик до БД або API)
            await Task.Delay(Random.Shared.Next(10, 100));

            // Валідація: перевірка чи дата не в минулому
            if (date < DateOnly.FromDateTime(DateTime.Now))
            {
                // Логування попередження
                // Перевірити: Aspire Dashboard → Logs → Level: Warning → 
                // фільтр "Requested date"
                _logger.LogWarning("Requested date {Date} is in the past", date);

                // ========== ТРЕЙСИНГ: Позначення помилки ==========
                // Встановлює статус Error - span буде червоним в Traces
                // Перевірити: Aspire Dashboard → Traces → спани з Status: Error (червоні)
                activity?.SetStatus(ActivityStatusCode.Error, "Past date requested");

                return BadRequest("Cannot get forecast for past dates");
            }

            // Генерація прогнозу
            var forecast = new WeatherForecast
            {
                Date = date,
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            };

            // Логування успішного результату з деталями
            // Перевірити: Aspire Dashboard → Logs → Level: Information → 
            // пошук "Retrieved forecast" → бачиш всі параметри
            _logger.LogInformation("Retrieved forecast for {Date}: {Temperature}°C, {Summary}",
                date, forecast.TemperatureC, forecast.Summary);

            // ========== ТРЕЙСИНГ: Додавання тегів результату ==========
            // Додає теги з результатами для аналізу
            // Перевірити: Aspire Dashboard → Traces → відкрий span → бачиш всі теги:
            // - forecast.date
            // - forecast.temperature
            // - forecast.summary
            activity?.SetTag("forecast.temperature", forecast.TemperatureC);
            activity?.SetTag("forecast.summary", forecast.Summary);

            // ========== ТРЕЙСИНГ: Позначення успіху ==========
            // Встановлює статус Ok - span буде зеленим
            // Перевірити: Aspire Dashboard → Traces → спани з Status: Ok (зелені)
            activity?.SetStatus(ActivityStatusCode.Ok);

            return Ok(forecast);
        }
        finally
        {
            // ========== МЕТРИКИ: Запис тривалості ==========
            // Finally блок виконується ЗАВЖДИ (успіх або помилка)
            stopwatch.Stop();

            // Записує histogram метрику з часом виконання
            // Перевірити: Aspire Dashboard → Metrics → пошук "api.request.duration" →
            // бачиш графік розподілу часу відгуку (P50, P95, P99)
            // Можна порівняти швидкість різних endpoints
            _telemetry.RequestDuration.Record(
                stopwatch.ElapsedMilliseconds,
                new KeyValuePair<string, object?>("endpoint", "get_by_date"));
        }
    }

    /// <summary>
    /// Triggers an error for testing error handling and observability
    /// </summary>
    /// <returns>Always throws an exception</returns>
    /// <response code="500">Always returns internal server error</response>
    [HttpGet("error")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult SimulateError()
    {
        // ========== ТРЕЙСИНГ: Початок Activity ==========
        // Створює span "SimulateError" для відстеження помилкового запиту
        using var activity = _telemetry.ActivitySource.StartActivity("SimulateError");

        // ========== ЛОГУВАННЯ: Попередження ==========
        // Записує Warning що викликали тестовий endpoint з помилкою
        _logger.LogWarning("Simulating error endpoint called");

        // ========== МЕТРИКИ: Лічильник запитів ==========
        // Збільшує загальний counter запитів з мітками endpoint="error", error="true"
        // Можна відфільтрувати скільки саме помилкових запитів було
        _telemetry.RequestCounter.Add(1,
            new KeyValuePair<string, object?>("endpoint", "error"),
            new KeyValuePair<string, object?>("error", "true"));

        // ========== МЕТРИКИ: Лічильник помилок ==========
        // Окремий counter ТІЛЬКИ для помилок з міткою endpoint="error"
        // Корисно для alerting: "Якщо ErrorCounter > 100 за хвилину - тривога!"
        _telemetry.ErrorCounter.Add(1,
            new KeyValuePair<string, object?>("endpoint", "error"));

        try
        {
            // Симулює помилку для тестування системи моніторингу
            throw new InvalidOperationException("This is a simulated error for testing observability");
        }
        catch (Exception ex)
        {
            // ========== ЛОГУВАННЯ: Помилка з exception ==========
            // LogError автоматично записує stack trace та деталі exception
            _logger.LogError(ex, "Simulated error occurred");

            // ========== ТРЕЙСИНГ: Додає Event до Activity ==========
            // ActivityEvent - це як "точка на часовій шкалі" всередині span
            // Записує що саме сталося, коли, тип exception та повідомлення
            // В Aspire Dashboard побачиш "exception" event всередині span з деталями
            activity?.AddEvent(new ActivityEvent(
                "exception",
                DateTimeOffset.UtcNow,
                new ActivityTagsCollection
                {
                    ["exception.message"] = ex.Message,
                    ["exception.type"] = ex.GetType().FullName
                }));

            // ========== ТРЕЙСИНГ: Позначає Activity як помилку ==========
            // Встановлює Status = Error з повідомленням exception
            // Span буде червоним в Traces з повідомленням помилки
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            // Перекидає exception далі - клієнт отримає HTTP 500
            throw;
        }
    }

    /// <summary>
    /// Creates an artificial delay for testing performance monitoring
    /// </summary>
    /// <param name="delayMs">Delay in milliseconds</param>
    /// <returns>Response after specified delay</returns>
    /// <response code="200">Returns success message after delay</response>
    [HttpGet("slow")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> SlowEndpoint([FromQuery] int delayMs = 2000)
    {
        // ========== ТРЕЙСИНГ: Батьківський Activity ==========
        // Створює головний span "SlowEndpoint" для всього запиту
        using var activity = _telemetry.ActivitySource.StartActivity("SlowEndpoint");

        // Додає тег з параметром затримки для аналізу
        activity?.SetTag("delay.ms", delayMs);

        // ========== ЛОГУВАННЯ: Початок обробки ==========
        // Записує що endpoint викликали і з якою затримкою
        _logger.LogInformation("Slow endpoint called with delay: {DelayMs}ms", delayMs);

        // ========== МЕТРИКИ: Лічильник запитів ==========
        // Збільшує counter для endpoint="slow"
        _telemetry.RequestCounter.Add(1,
            new KeyValuePair<string, object?>("endpoint", "slow"));

        // ========== ТРЕЙСИНГ: Вкладений (дочірній) Activity ==========
        // Створює CHILD span всередині батьківського
        // В Aspire Dashboard побачиш ієрархію:
        // SlowEndpoint (2000ms)
        //   └─ ProcessingDelay (2000ms) ← вкладений span
        using (var childActivity = _telemetry.ActivitySource.StartActivity("ProcessingDelay"))
        {
            // Додає тег до дочірнього span
            // Можна бачити що саме виконувалося всередині
            childActivity?.SetTag("processing.type", "artificial_delay");

            // Симуляція повільної операції (наприклад, виклик до зовнішнього API)
            await Task.Delay(delayMs);
        }
        // Дочірній span автоматично закривається тут (через using)

        // ========== ЛОГУВАННЯ: Завершення обробки ==========
        // Записує що операція завершилась
        _logger.LogInformation("Slow endpoint completed after {DelayMs}ms", delayMs);

        // Повертає відповідь клієнту
        return Ok(new
        {
            message = $"Response after {delayMs}ms delay",
            timestamp = DateTime.UtcNow
        });
    }
    // Батьківський span автоматично закривається тут (через using)
}
