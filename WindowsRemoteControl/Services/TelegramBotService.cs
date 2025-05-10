using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WindowsRemoteControl.Configuration;
using WindowsRemoteControl.Handlers;

namespace WindowsRemoteControl.Services;

public class TelegramBotService : BackgroundService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<TelegramBotService> _logger;
    private readonly BotConfiguration _config;
    private readonly CommandHandler _commandHandler;

    public TelegramBotService(
        BotConfiguration config,
        ILogger<TelegramBotService> logger,
        ApplicationManager applicationManager,
        NotificationService notificationService)
    {
        _config = config;
        _logger = logger;
        _botClient = new TelegramBotClient(_config.Token);
        _commandHandler = new CommandHandler(_config, _logger, applicationManager, _botClient, notificationService);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken
        );

        var me = await _botClient.GetMeAsync(stoppingToken);
        _logger.LogInformation($"Bot started: @{me.Username}");
    }

    async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message)
            return;

        if (message.Text is not { } messageText)
            return;

        var chatId = message.Chat.Id;
        var userId = message.From?.Id ?? 0;

        if (!_config.AuthorizedUsers.Contains(userId))
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Unauthorized access. This incident will be reported.",
                cancellationToken: cancellationToken);
            _logger.LogWarning($"Unauthorized access attempt by user ID: {userId}");
            return;
        }

        var action = messageText.Split(' ')[0];
        _logger.LogInformation($"Received '{action}' command from user {userId}");

        var response = await _commandHandler.HandleCommandAsync(messageText, chatId, cancellationToken);

        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: response,
            cancellationToken: cancellationToken);
    }

    Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogError(ErrorMessage);
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
    }
}
