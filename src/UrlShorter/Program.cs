using FASTER.core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using UrlShorter;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
using var settings = new FasterKVSettings<long, string>(Path.Combine(AppContext.BaseDirectory, "data")) { TryRecoverLatest = true };
using var store = new FasterKV<long, string>(settings);
builder.Services.AddSingleton(store);
builder.Services.AddHostedService<TimedCheckpointService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
// Configure the HTTP request pipeline.
app.MapControllers();

app.Run();
