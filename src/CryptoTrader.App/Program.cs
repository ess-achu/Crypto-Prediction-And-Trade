using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using CryptoTrader.App.Models;
using CryptoTrader.App.Services;
using CryptoTrader.App.ML;

var builder = WebApplication.CreateBuilder(args);

// Register all services as Singletons so they share memory with the BackgroundService
builder.Services.AddSingleton<BotState>();
builder.Services.AddSingleton<BinanceClient>();
builder.Services.AddSingleton<PaperTradingEngine>();
builder.Services.AddSingleton<TradeLearner>();

// Attach the trading engine thread
builder.Services.AddHostedService<BotRunnerService>();

var app = builder.Build();

// Enable serving html files from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

// Super fast minimal API to feed data to the JS frontend
app.MapGet("/api/status", (BotState state) => Results.Ok(state));

app.Run();
