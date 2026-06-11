using Microsoft.AspNetCore.Mvc;
using OpenTelemetryDemo.Api.Services;
using System.Diagnostics;

namespace OpenTelemetryDemo.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class TelemetryDemoController : ControllerBase
{
    private readonly ILogger<TelemetryDemoController> _logger;
    private readonly TelemetryService _telemetry;

    public TelemetryDemoController(ILogger<TelemetryDemoController> logger, TelemetryService telemetry)
    {
        _logger = logger;
        _telemetry = telemetry;
    }


    /// <summary>
    /// Demonstrates baggage propagation across spans
    /// </summary>
    /// <param name="userId">User ID to propagate as baggage</param>
    /// <param name="tenantId">Tenant ID to propagate as baggage</param>
    /// <returns>Baggage propagation example result</returns>
    [HttpGet("baggage")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> DemonstrateBaggage(
    [FromQuery] string userId = "user123",
    [FromQuery] string tenantId = "tenant456")
    {
        // ========== BAGGAGE: Встановлення контекстних даних ==========
        // SetBaggage додає дані в поточний Activity context
        // Ці дані АВТОМАТИЧНО передадуться у всі вкладені spans та між сервісами
        Activity.Current?.SetBaggage("user.id", userId);
        Activity.Current?.SetBaggage("tenant.id", tenantId);

        // ========== ТРЕЙСИНГ: Головний span demo ==========
        using var activity = _telemetry.ActivitySource.StartActivity("BaggageDemo");
        activity?.SetTag("demo.type", "baggage");

        // ========== ЛОГУВАННЯ ==========
        _logger.LogInformation(
            "Starting baggage propagation demo for user {UserId} in tenant {TenantId}",
            userId, tenantId);

        // ========== ВИКЛИК ВКЛАДЕНИХ СЕРВІСІВ ==========
        // ProcessWithBaggage зчитає userId та tenantId через GetBaggageItem
        // Не треба передавати параметрами - baggage автоматично доступний!
        var result1 = await ProcessWithBaggage("Service1");
        var result2 = await ProcessWithBaggage("Service2");

        // ========== ЧИТАННЯ ВСЬОГО BAGGAGE ==========
        // Activity.Current?.Baggage повертає всі встановлені key-value пари
        // Конвертує IEnumerable<KeyValuePair> у Dictionary для зручності
        var currentBaggage = Activity.Current?.Baggage?
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            ?? new Dictionary<string, string?>();

        // Повертає результати з усім контекстом
        return Ok(new
        {
            message = "Baggage propagation demo completed",
            baggage = currentBaggage,        // Всі baggage items
            service1Result = result1,         // Результат з userId, tenantId
            service2Result = result2          // Результат з userId, tenantId
        });
    }

    private async Task<object> ProcessWithBaggage(string serviceName)
    {
        // ========== ТРЕЙСИНГ: Створення span з динамічною назвою ==========
        // serviceName може бути "ServiceA", "ServiceB" → span "ProcessServiceA"
        using var activity = _telemetry.ActivitySource.StartActivity($"Process{serviceName}");

        // ========== BAGGAGE: Отримання контекстних даних ==========
        // Baggage - це key-value пари що АВТОМАТИЧНО передаються між сервісами
        // Не треба передавати вручну через параметри або headers!
        // GetBaggageItem читає дані які були встановлені в попередньому сервісі
        var userId = Activity.Current?.GetBaggageItem("user.id");
        var tenantId = Activity.Current?.GetBaggageItem("tenant.id");

        // ========== ТРЕЙСИНГ: Теги для аналізу ==========
        activity?.SetTag("service.name", serviceName);
        // Чи є userId в baggage (true/false)
        activity?.SetTag("has.user.id", !string.IsNullOrEmpty(userId));
        // Чи є tenantId в baggage (true/false)
        activity?.SetTag("has.tenant.id", !string.IsNullOrEmpty(tenantId));

        // ========== ЛОГУВАННЯ: З контекстом користувача ==========
        // userId та tenantId автоматично є в логах для correlation
        _logger.LogInformation("{ServiceName} processing with user {UserId} and tenant {TenantId}",
            serviceName, userId, tenantId);

        // Симуляція роботи сервісу
        await Task.Delay(Random.Shared.Next(50, 150));

        // Повертає результат з контекстними даними
        return new
        {
            service = serviceName,
            processedAt = DateTime.UtcNow,
            userId,      // Передається далі
            tenantId     // Передається далі
        };
    }


    /// <summary>
    /// Demonstrates span links for batch processing
    /// </summary>
    /// <param name="itemCount">Number of items to process in batch</param>
    /// <returns>Batch processing result with linked spans</returns>
    [HttpGet("span-links")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> DemonstrateSpanLinks([FromQuery] int itemCount = 5)
    {
        // ========== МЕТРИКИ: Активні запити ==========
        _telemetry.IncrementActiveRequests();

        try
        {
            // ========== ТРЕЙСИНГ: Головний span для batch операції ==========
            using var batchActivity = _telemetry.ActivitySource.StartActivity("BatchProcessingWithLinks");
            batchActivity?.SetTag("batch.size", itemCount);

            // Зберігаємо контексти всіх item spans для створення links
            var itemSpans = new List<ActivityContext>();
            var results = new List<object>();

            // ========== ОБРОБКА КОЖНОГО ITEM ==========
            // Кожен item створює ОКРЕМИЙ span (НЕ child!)
            for (int i = 0; i < itemCount; i++)
            {
                using var itemActivity = _telemetry.ActivitySource.StartActivity($"ProcessItem_{i}");

                if (itemActivity != null)
                {
                    itemActivity.SetTag("item.id", i);

                    // ========== ЗБЕРІГАЄМО CONTEXT ==========
                    // Зберігаємо ActivityContext кожного item для майбутніх links
                    itemSpans.Add(itemActivity.Context);

                    await Task.Delay(Random.Shared.Next(10, 50));
                    results.Add(new { itemId = i, processedAt = DateTime.UtcNow });
                }
            }

            // ========== SPAN LINKS: Створення зв'язків ==========
            // ActivityLink - це НЕ parent-child зв'язок, а "related to" зв'язок
            // Links показують: "цей span пов'язаний з тими spans"
            var links = itemSpans.Select(ctx => new ActivityLink(ctx)).ToArray();

            // ========== SUMMARY SPAN З LINKS ==========
            // StartActivity з параметрами:
            // - "BatchSummary" - назва
            // - ActivityKind.Internal - тип
            // - parentContext: default - БЕЗ батька (не child span!)
            // - links: links - ЗВ'ЯЗОК з попередніми spans через links
            using var summaryActivity = _telemetry.ActivitySource.StartActivity(
                "BatchSummary",
                ActivityKind.Internal,
                parentContext: default,
                links: links);

            // Тег з кількістю зв'язаних spans
            summaryActivity?.SetTag("linked.spans.count", itemSpans.Count);

            // Event з деталями завершення batch
            summaryActivity?.AddEvent(new ActivityEvent("BatchCompleted",
                DateTimeOffset.UtcNow,
                new ActivityTagsCollection { ["item.count"] = itemCount }));

            // ========== МЕТРИКИ ==========
            _telemetry.RequestCounter.Add(1,
                new KeyValuePair<string, object?>("operation", "batch_with_links"));

            return Ok(new
            {
                message = "Span links demonstration completed",
                itemsProcessed = itemCount,
                linkedSpans = itemSpans.Count,
                results
            });
        }
        finally
        {
            // ========== МЕТРИКИ: Завжди зменшуємо активні запити ==========
            _telemetry.DecrementActiveRequests();
        }
    }


    /// <summary>
    /// Demonstrates various metric types including UpDownCounter
    /// </summary>
    /// <returns>Current metric values</returns>
    [HttpGet("metrics-demo")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> DemonstrateMetrics()
    {
        // ========== ТРЕЙСИНГ ==========
        using var activity = _telemetry.ActivitySource.StartActivity("MetricsDemo");

        // ========== UP-DOWN COUNTER: Збільшення черги ==========
        // QueueDepth.Add(+5) - додали 1-5 задач в чергу
        // Симулює: нові задачі надійшли в обробку
        _telemetry.UpdateQueueDepth(Random.Shared.Next(1, 5));

        await Task.Delay(100);

        // ========== UP-DOWN COUNTER: Зменшення черги ==========
        // QueueDepth.Add(-3) - обробили 1-3 задачі з черги
        // Симулює: задачі завершилися
        _telemetry.UpdateQueueDepth(-Random.Shared.Next(1, 3));

        // ========== HISTOGRAM: Запис тривалості запиту ==========
        // Записує випадкове значення 50-500ms для демонстрації розподілу
        // В реальності це час виконання запиту
        _telemetry.RequestDuration.Record(Random.Shared.Next(50, 500));

        // ========== HISTOGRAM: Запис часу обробки ==========
        // Записує випадкове значення 10-100ms
        // В реальності це час конкретної операції (DB query, API call, etc)
        _telemetry.ProcessingTime.Record(Random.Shared.Next(10, 100));

        // Повертає інформацію про всі типи метрик
        return Ok(new
        {
            message = "Metrics demonstration completed",

            // ========== ПОТОЧНІ ЗНАЧЕННЯ ==========
            // GetActiveRequestCount() - скільки запитів обробляється зараз
            activeRequests = _telemetry.GetActiveRequestCount(),

            // GetQueueDepth() - скільки задач в черзі зараз
            queueDepth = _telemetry.GetQueueDepth(),

            // ========== ДОВІДКА ПРО ТИПИ МЕТРИК ==========
            metricTypes = new
            {
                // Counter - тільки збільшується (+1)
                counters = new[] { "RequestCounter", "ErrorCounter" },

                // Histogram - розподіл значень (P50, P95, P99)
                histograms = new[] { "RequestDuration", "ProcessingTime" },

                // UpDownCounter - може +1 та -1
                upDownCounters = new[] { "ActiveRequests", "QueueDepth" },

                // ObservableGauge - поточне значення (автоопитування)
                observableGauges = new[] { "CpuUsage" },

                // ObservableUpDownCounter - поточне значення лічильника
                observableUpDownCounters = new[] { "ThreadPoolSize" }
            }
        });
    }

    /// <summary>
    /// Demonstrates custom span attributes and events
    /// </summary>
    /// <param name="complexity">Processing complexity (1-10)</param>
    /// <returns>Processing result with custom telemetry</returns>
    [HttpGet("custom-telemetry")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> DemonstrateCustomTelemetry([FromQuery] int complexity = 5)
    {
        // ========== ТРЕЙСИНГ: Server span з детальними тегами ==========
        // ActivityKind.Server - означає що це entry point (вхідний запит)
        using var activity = _telemetry.ActivitySource.StartActivity("CustomTelemetryDemo", ActivityKind.Server);

        // Теги для категоризації та фільтрації
        activity?.SetTag("app.feature", "telemetry-demo");
        activity?.SetTag("app.version", TelemetryService.ServiceVersion);
        activity?.SetTag("processing.complexity", complexity);
        activity?.SetTag("processing.algorithm", "recursive");

        // ========== EVENTS: Timeline події всередині span ==========
        // AddEvent додає точку на часовій шкалі span з додатковими даними
        // Event "ProcessingStarted" з очікуваною тривалістю
        activity?.AddEvent(new ActivityEvent("ProcessingStarted",
            DateTimeOffset.UtcNow,
            new ActivityTagsCollection
            {
                ["complexity"] = complexity,
                ["expected.duration.ms"] = complexity * 100
            }));

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // ========== ОБРОБКА ПО СТАДІЯХ З EVENTS ==========
            // Кожна стадія додає 2 events: Started + Completed
            for (int stage = 1; stage <= complexity; stage++)
            {
                // Event початку стадії (без додаткових даних)
                activity?.AddEvent(new ActivityEvent($"Stage{stage}Started"));

                // Симуляція роботи стадії
                await Task.Delay(Random.Shared.Next(50, 150));

                // Event завершення стадії з часом виконання
                activity?.AddEvent(new ActivityEvent($"Stage{stage}Completed",
                    DateTimeOffset.UtcNow,
                    new ActivityTagsCollection { ["duration.ms"] = stopwatch.ElapsedMilliseconds }));
            }

            stopwatch.Stop();

            // ========== МЕТРИКИ: Counter з мітками успіху ==========
            _telemetry.RequestCounter.Add(1,
                new KeyValuePair<string, object?>("status", "success"),
                new KeyValuePair<string, object?>("complexity", complexity));

            // ========== МЕТРИКИ: Histogram часу обробки ==========
            _telemetry.ProcessingTime.Record(stopwatch.ElapsedMilliseconds,
                new KeyValuePair<string, object?>("complexity", complexity));

            // ========== ТРЕЙСИНГ: Статус успіху ==========
            activity?.SetStatus(ActivityStatusCode.Ok, "Processing completed successfully");

            return Ok(new
            {
                message = "Custom telemetry demonstration completed",
                complexity,
                stages = complexity,
                totalDurationMs = stopwatch.ElapsedMilliseconds,
                averageStageMs = stopwatch.ElapsedMilliseconds / complexity
            });
        }
        catch (Exception ex)
        {
            // ========== МЕТРИКИ: Лічильник помилок ==========
            _telemetry.ErrorCounter.Add(1,
                new KeyValuePair<string, object?>("error.type", ex.GetType().Name));

            // ========== ТРЕЙСИНГ: Статус помилки ==========
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            // ========== EVENTS: Exception event з деталями ==========
            // Додає event "Exception" на timeline з типом та повідомленням помилки
            activity?.AddEvent(new ActivityEvent("Exception",
                DateTimeOffset.UtcNow,
                new ActivityTagsCollection
                {
                    ["exception.type"] = ex.GetType().FullName,
                    ["exception.message"] = ex.Message
                }));

            throw;
        }
    }

    /// <summary>
    /// Demonstrates manual context propagation
    /// </summary>
    /// <returns>Context propagation information</returns>
    [HttpGet("context-propagation")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> DemonstrateContextPropagation()
    {
        // ========== ТРЕЙСИНГ: Батьківський span ==========
        using var parentActivity = _telemetry.ActivitySource.StartActivity("ParentOperation");

        // ========== КОНТЕКСТ: Зберігаємо context батьківського span ==========
        // Activity.Context містить TraceId, SpanId, TraceFlags
        // Потрібно зберегти ДО Task.Run, бо всередині Activity.Current може бути null
        var parentContext = Activity.Current?.Context ?? default;

        // ========== Task.Run: Новий потік ==========
        // Task.Run виконується в іншому потоці ThreadPool
        // Activity.Current в новому потоці = null (контекст не передається автоматично!)
        var result = await Task.Run(async () =>
        {
            // ========== ТРЕЙСИНГ: Дочірній span з РУЧНИМ прокиданням контексту ==========
            // StartActivity з 3 параметрами:
            // 1. "ChildOperation" - назва span
            // 2. ActivityKind.Internal - тип activity (Internal/Server/Client/Producer/Consumer)
            // 3. parentContext - ВРУЧНУ передаємо батьківський контекст
            // Без parentContext - child span був би в ІНШОМУ trace!
            using var childActivity = _telemetry.ActivitySource.StartActivity(
                "ChildOperation",
                ActivityKind.Internal,
                parentContext);

            // Тег що контекст передано вручну
            childActivity?.SetTag("manually.propagated", true);

            await Task.Delay(100);

            // Повертаємо дані для демонстрації зв'язку spans
            return new
            {
                parentTraceId = parentContext.TraceId.ToString(),
                parentSpanId = parentContext.SpanId.ToString(),
                childTraceId = childActivity?.TraceId.ToString(),
                childSpanId = childActivity?.SpanId.ToString(),
                // TraceId однаковий = spans в одному trace ✅
                isSameTrace = childActivity?.TraceId == parentContext.TraceId
            };
        });

        return Ok(new
        {
            message = "Context propagation demonstration completed",
            result
        });
    }
}

