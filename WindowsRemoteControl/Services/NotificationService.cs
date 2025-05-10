using Microsoft.Extensions.Logging;
using System.Management;
using WindowsRemoteControl.Configuration;
using Telegram.Bot;
using System.Runtime.InteropServices;

namespace WindowsRemoteControl.Services;

public class NotificationService : IDisposable
{
    private readonly ITelegramBotClient _botClient;
    private readonly BotConfiguration _config;
    private readonly ILogger<NotificationService> _logger;
    private readonly ManagementEventWatcher? _processWatcher;
    private readonly ManagementEventWatcher? _loginWatcher;
    private readonly object _lockObject = new();
    private bool _isMonitoring;

    public NotificationService(
        ITelegramBotClient botClient,
        BotConfiguration config,
        ILogger<NotificationService> logger)
    {
        _botClient = botClient;
        _config = config;
        _logger = logger;
        
        try
        {
            // Monitor process creation
            _processWatcher = new ManagementEventWatcher(
                new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
            _processWatcher.EventArrived += OnProcessStarted;

            // Monitor login sessions
            _loginWatcher = new ManagementEventWatcher(
                new WqlEventQuery("SELECT * FROM SystemConfig_V2_SessionActivationNotification"));
            _loginWatcher.EventArrived += OnSessionActivity;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error initializing notification service: {ex.Message}");
        }
    }

    public void StartMonitoring()
    {
        if (_isMonitoring) return;

        lock (_lockObject)
        {
            try
            {
                _processWatcher?.Start();
                _loginWatcher?.Start();
                _isMonitoring = true;
                _logger.LogInformation("Notification monitoring started");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error starting monitoring: {ex.Message}");
            }
        }
    }

    public void StopMonitoring()
    {
        if (!_isMonitoring) return;

        lock (_lockObject)
        {
            try
            {
                _processWatcher?.Stop();
                _loginWatcher?.Stop();
                _isMonitoring = false;
                _logger.LogInformation("Notification monitoring stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error stopping monitoring: {ex.Message}");
            }
        }
    }

    private async void OnProcessStarted(object sender, EventArrivedEventArgs e)
    {
        try
        {
            string processName = e.NewEvent["ProcessName"]?.ToString() ?? "unknown";
            uint processId = Convert.ToUInt32(e.NewEvent["ProcessID"]);
            
            // Notify only for important system processes or specific patterns
            if (processName.ToLower().Contains("cmd") || 
                processName.ToLower().Contains("powershell") ||
                processName.ToLower().Contains("msi") ||
                processName.ToLower().Contains("install"))
            {
                string message = $"🚨 Phát hiện tiến trình: {processName} (PID: {processId})";
                await NotifyAll(message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing process event: {ex.Message}");
        }
    }

    private async void OnSessionActivity(object sender, EventArrivedEventArgs e)
    {
        try
        {
            string sessionInfo = e.NewEvent.ToString();
            string message = $"🔔 Hoạt động đăng nhập: {sessionInfo}";
            await NotifyAll(message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing session event: {ex.Message}");
        }
    }

    public async Task NotifyAll(string message)
    {
        foreach (var userId in _config.AuthorizedUsers)
        {
            try
            {
                await _botClient.SendTextMessageAsync(
                    chatId: userId,
                    text: message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending notification to user {userId}: {ex.Message}");
            }
        }
    }

    public void Dispose()
    {
        StopMonitoring();
        _processWatcher?.Dispose();
        _loginWatcher?.Dispose();
    }
}
