using DesmondBot.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;

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
.AddComponentInteractions();

var host = builder.Build();

host.AddModules(typeof(Program).Assembly);

await host.RunAsync();