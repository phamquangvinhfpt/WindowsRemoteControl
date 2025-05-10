using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using WindowsRemoteControl.Services;
using WindowsRemoteControl.Configuration;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<BotConfiguration>();
        services.AddSingleton<ApplicationManager>();
        services.AddSingleton<StartupManager>();
        services.AddSingleton<ITelegramBotClient>(sp => 
            new TelegramBotClient(sp.GetRequiredService<BotConfiguration>().Token));
        services.AddSingleton<NotificationService>();
        services.AddHostedService<TelegramBotService>();
    })
    .Build();

// Parse command line arguments
var startupManager = host.Services.GetRequiredService<StartupManager>();

// Register for startup if --register argument is provided
// if (args.Contains("--register"))
// {
//     startupManager.RegisterForStartup();
//     Environment.Exit(0);
// }
    startupManager.RegisterForStartup();

// Unregister from startup if --unregister argument is provided
if (args.Contains("--unregister"))
{
    startupManager.UnregisterFromStartup();
    Environment.Exit(0);
}

try
{
    await host.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    return 1;
}

return 0;
