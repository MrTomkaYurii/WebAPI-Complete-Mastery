using GenericHostBoilerplate.App.HostedServices;
using GenericHostBoilerplate.App.Services.Contracts;
using GenericHostBoilerplate.App.Services.Impl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

CreateHostBuilder(args).Build().Run();

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostBuilderContext, services) =>
        {
            services.AddTransient<IExampleService, ExampleService>();
            services.AddHostedService<HostedService>();
        })
        .ConfigureAppConfiguration((hostBuilderContext, configBuilder) =>
        {
            var env = hostBuilderContext.HostingEnvironment;

            configBuilder.AddEnvironmentVariables();
            configBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            configBuilder.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: false, reloadOnChange: true);
        })
        .ConfigureLogging((hostBuilderContext, loggingBuilder) =>
        {
            // Додавання консольного логіювання
            loggingBuilder.AddConsole(); 
            // Відключення логів запуску хоста
            loggingBuilder.AddFilter<ConsoleLoggerProvider>("Microsoft.Hosting.Lifetime", LogLevel.None);
            // Відключення інших логів
            loggingBuilder.AddFilter<ConsoleLoggerProvider>("Microsoft.Extensions.Hosting.Internal.Host", LogLevel.None);
            // Налаштування мінімального рівня логів для всіх провайдерів
            loggingBuilder.SetMinimumLevel(LogLevel.Debug); 
        });