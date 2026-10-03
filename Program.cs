using DesmondBot.Features.Roulette.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;

var builder = Host.CreateApplicationBuilder(args);
var token = builder.Configuration["Discord:Token"] ?? throw new InvalidOperationException("Discord bot token not found in configuration");

builder.Services.AddDiscordGateway(options =>
{
	options.Token = token;
})
.AddApplicationCommands()
.AddComponentInteractions<ButtonInteraction, ButtonInteractionContext>()
.AddComponentInteractions<ModalInteraction, ModalInteractionContext>();

builder.Services.AddSingleton<RouletteGameService>();

var host = builder.Build();

host.AddModules(typeof(Program).Assembly);

await host.RunAsync();