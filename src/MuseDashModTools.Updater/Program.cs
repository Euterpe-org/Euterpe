using Cocona;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = CoconaApp.CreateBuilder();
builder.Services.AddSingleton<ILocalService, LocalService>();

#if DEBUG
builder.Logging.SetMinimumLevel(LogLevel.Debug);
#else
builder.Logging.SetMinimumLevel(LogLevel.Information);
#endif

builder.Logging.AddConsole();

var app = builder.Build();

app.AddCommands<Commands>();

await app.RunAsync();