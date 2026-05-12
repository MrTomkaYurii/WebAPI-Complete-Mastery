using OptionsPatternDemo;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOptions<WeatherOptions>().BindConfiguration(nameof(WeatherOptions))
//01 - Startup Validation
    .ValidateDataAnnotations()
    .ValidateOnStart();
//02 - Own custom validation logic against the WeatherOptions class
//.Validate(options =>
// {
//     if (options.State != "Kerala") return false;
//     return true;
// });

//03 - How to Load Configurations in Program.cs?
var weatherOptions = builder.Configuration.GetSection(nameof(WeatherOptions)).Get<WeatherOptions>();
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
