using System.Diagnostics;
using System.Diagnostics.Metrics;
using static System.Net.WebRequestMethods;

namespace OpenTelemetryDemo.Api.Services;

public class TelemetryService
{
            public const string ServiceName = "OpenTelemetryDemo.Api";
            public const string ServiceVersion = "1.0.0";

            // ========== ТРЕЙСИНГ ==========
            // ActivitySource - джерело для створення spans (activities) для distributed tracing
            // Використання: ActivitySource.StartActivity("MethodName")
            public ActivitySource ActivitySource { get; }

            // ========== БАТЬКІВСЬКИЙ ОБ'ЄКТ МЕТРИК ==========
            // Meter - контейнер для всіх метрик, групує метрики одного сервісу
            public Meter Meter { get; }

            // ========== COUNTER: Лічильники (тільки зростають) ==========
            // RequestCounter - загальна кількість запитів (завжди +1)
            // Використання: RequestCounter.Add(1, new KeyValuePair("endpoint", "..."))
            public Counter<long> RequestCounter { get; }

            // ErrorCounter - загальна кількість помилок (завжди +1)
            // Корисно для alerting: "якщо > 100 помилок за хвилину - alarm!"
            public Counter<long> ErrorCounter { get; }

            // ========== HISTOGRAM: Розподіл значень ==========
            // RequestDuration - розподіл часу виконання запитів (P50, P95, P99)
            // Використання: RequestDuration.Record(stopwatch.ElapsedMilliseconds)
            // Показує мін, макс, середнє, percentiles для знаходження повільних запитів
            public Histogram<double> RequestDuration { get; }

            // ProcessingTime - розподіл часу обробки конкретних операцій
            // Використання: ProcessingTime.Record(operationMs)
            public Histogram<long> ProcessingTime { get; }

            // ========== UP-DOWN COUNTER: +1 та -1 ==========
            // ActiveRequests - ПОТОЧНА кількість запитів що обробляються зараз
            // Використання: початок +1, кінець -1
            // Показує навантаження в реальному часі
            public UpDownCounter<int> ActiveRequests { get; }

            // QueueDepth - ПОТОЧНА глибина черги (скільки задач чекають)
            // Використання: додали в чергу +1, взяли з черги -1
            // Якщо постійно росте - bottleneck!
            public UpDownCounter<int> QueueDepth { get; }

            // ========== OBSERVABLE: Автоопитування ==========
            // CpuUsage - ПОТОЧНЕ використання CPU (%)
            // Система автоматично викликає callback кожні N секунд
            // Не викликаєш вручну - тільки задаєш функцію що повертає значення
            public ObservableGauge<double> CpuUsage { get; }

            // ThreadPoolSize - ПОТОЧНИЙ розмір thread pool
            // Автоматично опитується системою для моніторингу потоків
            public ObservableUpDownCounter<int> ThreadPoolSize { get; }


            private int _activeRequests = 0;
            private int _queueDepth = 0;

    public TelemetryService()
    {
        ActivitySource = new ActivitySource(ServiceName, ServiceVersion);
        Meter = new Meter(ServiceName, ServiceVersion);

        //створює лічильник(Counter) для підрахунку HTTP - запитів
        RequestCounter = Meter.CreateCounter<long>(
            "app.requests.total",   //унікальна назва метрики, яку буде видно у системі моніторингу
            unit: "requests",       //одиниця виміру (для кращої зрозумілості у dashboard).
            description: "Total number of requests received");

        //створює лічильник для помилок
        ErrorCounter = Meter.CreateCounter<long>(
            "app.errors.total",
            unit: "errors",
            description: "Total number of errors");

        //створює гістограму (Histogram) для вимірювання часу виконання запитів
        RequestDuration = Meter.CreateHistogram<double>(
            "app.request.duration",
            unit: "ms",
            description: "Request duration in milliseconds");

        //для вимірювання часу обробки певних операцій у вашому додатку
        ProcessingTime = Meter.CreateHistogram<long>(
            "app.processing.time",
            unit: "ms",
            description: "Processing time for operations");

        //відстеження активних запитів у вашому додатку
        ActiveRequests = Meter.CreateUpDownCounter<int>(
            "app.requests.active",
            unit: "requests",
            description: "Number of requests currently being processed");

        //для моніторингу глибини черги обробки у вашому додатку через OpenTelemetry Metrics
        QueueDepth = Meter.CreateUpDownCounter<int>(
            "app.queue.depth",
            unit: "items",
            description: "Current depth of the processing queue");

        //для моніторингу використання CPU
        CpuUsage = Meter.CreateObservableGauge(
            "app.cpu.usage",
            observeValue: () => GetCpuUsage(),
            unit: "percent",
            description: "Current CPU usage percentage");

        //для моніторингу розміру ThreadPool у вашому додатку
        ThreadPoolSize = Meter.CreateObservableUpDownCounter(
            "app.threadpool.size",
            observeValue: () => GetThreadPoolSize(),
            unit: "threads",
            description: "Current thread pool size");
    }

    public void IncrementActiveRequests()
    {
        Interlocked.Increment(ref _activeRequests);
        ActiveRequests.Add(1);
    }

    //для зменшення лічильника активних запитів у багатопотоковому середовищі.
    public void DecrementActiveRequests()
    {
        Interlocked.Decrement(ref _activeRequests);
        ActiveRequests.Add(-1);
    }

    public void UpdateQueueDepth(int delta)
    {
        Interlocked.Add(ref _queueDepth, delta);
        QueueDepth.Add(delta);
    }

    public int GetActiveRequestCount() => _activeRequests;
    public int GetQueueDepth() => _queueDepth;

    private double GetCpuUsage()
    {
        // Demo: Returns random CPU usage for demonstration purposes
        return Random.Shared.Next(10, 90);
    }

    private int GetThreadPoolSize()
    {
        ThreadPool.GetAvailableThreads(out _, out _);
        ThreadPool.GetMaxThreads(out int maxWorkerThreads, out _);
        ThreadPool.GetMinThreads(out int minWorkerThreads, out _);
        return maxWorkerThreads - minWorkerThreads;
    }
}

