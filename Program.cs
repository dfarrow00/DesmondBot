using DesmondBot.Features.Economy.Application;
using DesmondBot.Features.Roulette.Application;
using DesmondBot.Features.Roulette.Discord;
using DesmondBot.Infrastructure.Persistence.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord;
using NetCord.Hosting.Gateway;
using Serilog;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;
using NetCord.Gateway;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
	.ReadFrom.Configuration(builder.Configuration)
	.ReadFrom.Services(services)
	.Enrich.FromLogContext());

var token = builder.Configuration["Discord:Token"] ?? throw new InvalidOperationException("Discord bot token not found in configuration");
var databasePath = Path.Combine(builder.Environment.ContentRootPath,"Data","desmond.db");

builder.Services.AddDiscordGateway(options =>
{
	options.Token = token;
	options.Intents = GatewayIntents.Guilds | GatewayIntents.GuildUsers | GatewayIntents.GuildPresences;
})
.AddApplicationCommands()
.AddComponentInteractions<ButtonInteraction, ButtonInteractionContext>()
.AddComponentInteractions<ModalInteraction, ModalInteractionContext>()
.AddGatewayHandlers(typeof(Program).Assembly);

builder.Services.AddSingleton(new SqliteConnectionFactory(databasePath));
builder.Services.AddSingleton<SqliteDatabaseInitializer>();

builder.Services.AddSingleton<RouletteGameService>();
builder.Services.AddSingleton<RouletteMessageService>();
builder.Services.AddSingleton<IEconomyRepository, SqliteEconomyRepository>();
builder.Services.AddHostedService<DailyBalanceResetService>();

var host = builder.Build();

var databaseInitializer = host.Services.GetRequiredService<SqliteDatabaseInitializer>();
await databaseInitializer.InitializeAsync();

host.AddModules(typeof(Program).Assembly);

await host.RunAsync();