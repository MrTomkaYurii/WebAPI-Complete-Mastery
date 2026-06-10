using Microsoft.AspNetCore.Mvc;
using OpenTelemetryDemo.Api.Services;
using System.Diagnostics;

namespace OpenTelemetryDemo.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class MetricsController : ControllerBase
{
    private readonly ILogger<MetricsController> _logger;
    private readonly TelemetryService _telemetry;

    public MetricsController(ILogger<MetricsController> logger, TelemetryService telemetry)
    {
        _logger = logger;
        _telemetry = telemetry;
    }

    /// <summary>
    /// Processes a request with configurable complexity for metrics testing
    /// </summary>
    /// <param name="complexity">Processing complexity level (1-10)</param>
    /// <returns>Processing result with timing information</returns>
    /// <response code="200">Returns processing result</response>
    /// <response code="500">If processing fails</response>
    [HttpGet("process")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProcessRequest([FromQuery] int complexity = 1)
    {
        // ========== МЕТРИКИ: Збільшення лічильника активних запитів ==========
        // ActiveRequests +1 (UpDownCounter)
        // Показує скільки запитів обробляється ЗАРАЗ
        _telemetry.IncrementActiveRequests();
                // ========== ТРЕЙСИНГ: Батьківський span ==========
        using var activity = _telemetry.ActivitySource.StartActivity("ProcessRequest");
        activity?.SetTag("complexity", complexity);
                // ========== ЛОГУВАННЯ ==========
        _logger.LogInformation("Processing request with complexity: {Complexity}", complexity);
                // ========== МЕТРИКИ: Лічильник запитів з мітками ==========
        // Рахує скільки запитів з різним рівнем складності
        _telemetry.RequestCounter.Add(1,
            new KeyValuePair<string, object?>("operation", "process"),
            new KeyValuePair<string, object?>("complexity", complexity.ToString()));
                // Таймер для вимірювання часу виконання
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // ========== ТРЕЙСИНГ: Вкладений span для обчислень ==========
            // Створює child span "ComputeResult" всередині "ProcessRequest"
            // В Aspire побачиш ієрархію spans
            using (var span = _telemetry.ActivitySource.StartActivity("ComputeResult"))
            {
                span?.SetTag("computation.type", "fibonacci");
                                // Виконує обчислення Fibonacci в окремому потоці
                // Math.Min обмежує складність до 40 (запобігає переповненню)
                var result = await Task.Run(() =>
                    ComputeFibonacci(Math.Min(complexity * 10, 40)));
                span?.SetTag("computation.result", result);
                                // LogDebug - виводиться тільки в Development
                _logger.LogDebug("Computed Fibonacci({N}) = {Result}",
                    complexity * 10, result);
            }
            // Child span закривається тут

            // Додаткова затримка залежно від складності
            await Task.Delay(complexity * 100);

            stopwatch.Stop();

            // ========== МЕТРИКИ: Histogram часу обробки ==========
            // Записує скільки мілісекунд тривала обробка
            // Дозволяє бачити P50, P95, P99 для різних рівнів складності
            _telemetry.ProcessingTime.Record(stopwatch.ElapsedMilliseconds,
                new KeyValuePair<string, object?>("complexity", complexity.ToString()));

            return Ok(new
            {
                complexity,
                processingTimeMs = stopwatch.ElapsedMilliseconds,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            // ========== ЛОГУВАННЯ: Помилка ==========
            _logger.LogError(ex, "Error processing request with complexity {Complexity}",
                complexity);

            // ========== ТРЕЙСИНГ: Event з деталями exception ==========
            // Додає event "exception" до timeline span'у
            activity?.AddEvent(new ActivityEvent("exception", DateTimeOffset.UtcNow,
                new ActivityTagsCollection
                {
                    ["exception.message"] = ex.Message,
                    ["exception.type"] = ex.GetType().FullName
                }));

            // ========== ТРЕЙСИНГ: Позначає span як помилку ==========
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            return StatusCode(500, "Processing error");
        }
        finally
        {
            // ========== МЕТРИКИ: Зменшення лічильника активних запитів ==========
            // ActiveRequests -1 (UpDownCounter)
            // Finally виконується ЗАВЖДИ - і при успіху, і при помилці
            // Гарантує що лічильник не "застрягне" при exception
            _telemetry.DecrementActiveRequests();
        }
        // В Aspire Dashboard → Traces побачиш:
        // ProcessRequest (3000ms)
        //   └─ ComputeResult (2800ms) ← вкладений span з обчисленнями
    }


    /// <summary>
    /// Processes multiple items in batch for testing concurrent operations
    /// </summary>
    /// <param name="count">Number of items to process</param>
    /// <returns>Batch processing results</returns>
    /// <response code="200">Returns batch results</response>
    [HttpGet("batch")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> BatchProcess([FromQuery] int count = 5)
    {
        // ========== ТРЕЙСИНГ: Батьківський span ==========
        // Створює головний span для всієї batch операції
        using var activity = _telemetry.ActivitySource.StartActivity("BatchProcess");

        // Тег з розміром batch для аналізу продуктивності
        activity?.SetTag("batch.size", count);

        // ========== ЛОГУВАННЯ: Початок обробки ==========
        _logger.LogInformation("Starting batch process with {Count} items", count);

        // ========== МЕТРИКИ: Лічильник batch операцій ==========
        _telemetry.RequestCounter.Add(1,
            new KeyValuePair<string, object?>("operation", "batch"));

        // Створює список задач для паралельної обробки
        var tasks = new List<Task<int>>();

        // Запускає N задач паралельно (не чекає завершення кожної)
        for (int i = 0; i < count; i++)
        {
            var index = i;
            // Кожен ProcessItem() створить свій вкладений span
            tasks.Add(ProcessItem(index));
        }

        // Чекає поки ВСІ задачі завершаться паралельно
        // Task.WhenAll ефективніше ніж await в циклі
        var results = await Task.WhenAll(tasks);

        // ========== ЛОГУВАННЯ: Завершення ==========
        _logger.LogInformation("Batch process completed. Processed {Count} items", results.Length);

        // ========== ТРЕЙСИНГ: Результат обробки ==========
        // Додає тег з кількістю оброблених items
        activity?.SetTag("batch.completed", results.Length);

        // Повертає результати: кількість, сума всіх значень, масив результатів
        return Ok(new
        {
            processed = results.Length,
            totalSum = results.Sum(),
            results
        });
        // В Aspire Dashboard → Traces:
        // BatchProcess (2000ms)
        //   ├─ ProcessItem[0] (500ms)  ← паралельно
        //   ├─ ProcessItem[1] (400ms)  ← паралельно
        //   ├─ ProcessItem[2] (600ms)  ← паралельно
        //   └─ ProcessItem[3] (300ms)  ← паралельно
    }


    private async Task<int> ProcessItem(int index)
    {
        using var activity = _telemetry.ActivitySource.StartActivity("ProcessItem");
        activity?.SetTag("item.index", index);

        await Task.Delay(Random.Shared.Next(50, 200));

        var result = Random.Shared.Next(1, 100);
        _logger.LogDebug("Processed item {Index} with result {Result}", index, result);

        return result;
    }

    private long ComputeFibonacci(int n)
    {
        if (n <= 1) return n;
        return ComputeFibonacci(n - 1) + ComputeFibonacci(n - 2);
    }

    /// <summary>
    /// Gets the current status of the metrics service
    /// </summary>
    /// <returns>Service status information</returns>
    /// <response code="200">Returns service status</response>
    [HttpGet("status")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetStatus()
    {
        // ========== ЛОГУВАННЯ ==========
        // Записує що викликали status endpoint (health check)
        // Зазвичай такі endpoints викликають дуже часто (кожні 5-10 сек)
        // тому логування на рівні Information може створити багато логів
        _logger.LogInformation("Status endpoint called");

        // Повертає JSON з інформацією про стан сервісу
        return Ok(new
        {
            // Статус здоров'я сервісу
            status = "healthy",

            // ========== МЕТРИКИ: Кількість активних запитів ==========
            // Викликає кастомний метод телеметрії який повертає 
            // скільки зараз обробляється запитів одночасно
            // Корисно для моніторингу навантаження в реальному часі
            activeRequests = _telemetry.GetActiveRequestCount(),

            // Час на сервері (UTC)
            timestamp = DateTime.UtcNow,

            // Поточне оточення (Development, Production, Staging)
            // Корисно для перевірки в яке середовище потрапив запит
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        });
    }

}
