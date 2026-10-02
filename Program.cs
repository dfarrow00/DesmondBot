using DesmondBot.Features.Roulette.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;

var builder = Host.CreateApplicationBuilder(args);

var configuration = new ConfigurationBuilder()
	.SetBasePath(AppContext.BaseDirectory)
	.AddJsonFile("appsettings.json", optional: false)
	.Build();

string token = configuration["Discord:Token"] ?? throw new Exception("Discord bot token not found in configuration");

builder.Services.AddDiscordGateway(options =>
{
	options.Token = token;
})
.AddApplicationCommands()
.AddComponentInteractions<ButtonInteraction, ButtonInteractionContext>();

builder.Services.AddSingleton<RouletteGameService>();

var host = builder.Build();

host.AddModules(typeof(Program).Assembly);

await host.RunAsync();