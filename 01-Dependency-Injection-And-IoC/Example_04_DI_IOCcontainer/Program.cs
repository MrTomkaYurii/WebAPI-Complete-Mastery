using Example_04_DI_IOCcontainer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

//конфігруруємо хост
IHostBuilder CreateHostBuilder() =>
    Host.CreateDefaultBuilder()
        .ConfigureServices((hostcontext, services) =>
            services.AddTransient<ITransientGetCreatedTime, GetCreatedTimeImplementation>()
                    .AddScoped<IScopedGetCreatedTime, GetCreatedTimeImplementation>()
                    .AddSingleton<ISingletonGetCreatedTime, GetCreatedTimeImplementation>()
                    .AddTransient<GetCreatedTimeInvoker>());
//білдимо хост
using IHost host = CreateHostBuilder().Build();

//Створюємо скоупи
ExecuteScope(host.Services, "sample-scope-1");
ExecuteScope(host.Services, "sample-scope-2");

static void ExecuteScope(IServiceProvider services, string scope)
{
    using IServiceScope serviceScope = services.CreateScope();
    IServiceProvider provider = serviceScope.ServiceProvider;

    Console.WriteLine("===================================================");
    Console.WriteLine($"Scope Name: {scope}");
    Console.WriteLine("===================================================");
    Console.WriteLine($"First Call -->>");
    GetCreatedTimeInvoker invoker1 = provider.GetRequiredService<GetCreatedTimeInvoker>();
    invoker1.Invoke();

    Console.WriteLine("... ...");
    Console.WriteLine("... ...");

    Console.WriteLine($"Second Call -->>");
    GetCreatedTimeInvoker invoker2 = provider.GetRequiredService<GetCreatedTimeInvoker>();
    invoker2.Invoke();

    Console.WriteLine("= END =");
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine();
}