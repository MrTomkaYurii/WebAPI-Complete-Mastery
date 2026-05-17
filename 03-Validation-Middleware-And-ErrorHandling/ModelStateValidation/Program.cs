
using AspNet_ModelStateValidation.Filters;
using Microsoft.AspNetCore.Mvc;

namespace AspNet_ModelStateValidation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();



            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;// Это отключает автоматическую обработку ошибок валидации модели, которая обычно возвращает HTTP 400 Bad Request
            });

            builder.Services.AddScoped<ValidationFilterAttribute>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

                        app.MapControllers();

            app.Run();
        }
    }
}
