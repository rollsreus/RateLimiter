using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 4;
        options.Window = TimeSpan.FromSeconds(60);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    }));

builder.Services.AddRateLimiter(_ => _
    .AddSlidingWindowLimiter(policyName: "sliding", options =>
    {
        options.PermitLimit = 3;
        options.Window = TimeSpan.FromSeconds(60);
        options.SegmentsPerWindow = 3;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    }));

builder.Services.AddRateLimiter(_ => _
    .AddTokenBucketLimiter(policyName: "tokenPolicy", options =>
    {
        options.TokenLimit =  4;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
        options.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
        options.TokensPerPeriod = 3;
        options.AutoReplenishment = true;
    }));

builder.Services.AddRateLimiter(_ => _.AddConcurrencyLimiter(policyName: "Concurrency", options =>
    {
        options.PermitLimit = 4;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    }));


var app = builder.Build();

app.UseRateLimiter();

static string GetTicks() => (DateTime.Now.Ticks & 0x11111).ToString("00000");

app.MapGet("/", () => Results.Ok($"Hello fixed {GetTicks()}"))
                           .RequireRateLimiting("fixed");

app.MapGet("/sliding", () => Results.Ok($"Hello sliding {GetTicks()}"))
                           .RequireRateLimiting("sliding");

app.MapGet("/Token", () => Results.Ok($"Hello Token {GetTicks()}"))
                           .RequireRateLimiting("tokenPolicy");

app.MapGet("/Concurrent", () => Results.Ok($"Hello concurrency {GetTicks()}"))
                           .RequireRateLimiting("Concurrency");



app.Run();