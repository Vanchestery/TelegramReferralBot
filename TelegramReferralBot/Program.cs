using ReferralBot.Extensions;
using ReferralBot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();

ContainerConfigurator.Configure(builder.Services, builder.Configuration);

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
