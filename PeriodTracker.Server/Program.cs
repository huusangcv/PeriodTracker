using PeriodTracker.Entities;
using PeriodTracker.Services.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;

var MyAllowSpecificOrigins = "_secure_origins";
services.AddCors(options =>
{
    options.AddPolicy(
        MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins(
#if DEBUG
                    "https://localhost:56391",
                    "https://localhost:56392",
                    "http://172.27.16.1:5248",
                    "http://192.168.1.3:8081"
#endif
                   )
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials()
                  ;
        });
});


services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
services.AddOpenApi();

// Register dependency
services.AddHttpContextAccessor();
services.AddDbContext<DataContext>();
DependencyRegister.Register(services);

var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
