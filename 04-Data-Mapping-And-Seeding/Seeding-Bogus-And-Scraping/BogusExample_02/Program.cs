using BogusExample_02;
using BogusExample_02.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<Worker>();

        services.AddDbContext<DatabaseContext>(x =>
        {
            x.UseSqlServer(hostContext.Configuration.GetConnectionString("Database"));
        });

    })
    .Build();

await host.RunAsync();