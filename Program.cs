using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using NetCord.Hosting.Gateway;

var builder = Host.CreateApplicationBuilder(args);

var configuration = new ConfigurationBuilder()
	.SetBasePath(AppContext.BaseDirectory)
	.AddJsonFile("appsettings.json", optional: false)
	.Build();

string token = configuration["Discord:Token"] ?? throw new Exception("Discord bot token not found in configuration");

builder.Services.AddDiscordGateway(options =>
{
	options.Token = token;
});

var host = builder.Build();

await host.RunAsync();