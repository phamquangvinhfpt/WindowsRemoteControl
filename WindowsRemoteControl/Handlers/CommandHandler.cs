using Microsoft.Extensions.Logging;
using WindowsRemoteControl.Configuration;
using WindowsRemoteControl.Services;
using WindowsRemoteControl.Utils;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace WindowsRemoteControl.Handlers;

public class CommandHandler
{
    private readonly BotConfiguration _config;
    private readonly ILogger _logger;
    private readonly ApplicationManager _applicationManager;
    private readonly ITelegramBotClient _botClient;
    private readonly NotificationService _notificationService;

    public CommandHandler(
        BotConfiguration config,
        ILogger logger,
        ApplicationManager applicationManager,
        ITelegramBotClient botClient,
        NotificationService notificationService)
    {
        _config = config;
        _logger = logger;
        _applicationManager = applicationManager;
        _botClient = botClient;
        _notificationService = notificationService;
    }

    public async Task<string> HandleCommandAsync(string messageText, long chatId, CancellationToken cancellationToken)
    {
        var parts = messageText.Split(' ');
        var action = parts[0].ToLower();

        return action switch
        {
            "/start" => GetStartMessage(),
            "/help" => GetHelpMessage(),
            "/list" => _applicationManager.GetApplicationList(),
            "/status" => "Bot đang hoạt động và sẵn sàng nhận lệnh.",
            "/shutdown" => await HandleShutdown(chatId, cancellationToken),
            "/open" => HandleOpenApp(messageText),
            "/screenshot" => await HandleScreenshot(chatId, cancellationToken),
            "/notifications" or "/notify" => HandleNotifications(parts),
            "/sysinfo" => GetSystemInfo(),
            "/processes" => GetRunningProcesses(),
            "/killprocess" => KillProcess(parts),
            _ => "Lệnh không hợp lệ. Dùng /help để xem danh sách lệnh."
        };
    }

    private string GetStartMessage()
    {
        return @"Remote Windows Control Bot đang hoạt động!
Sử dụng /open <tên_ứng_dụng> để mở ứng dụng
Sử dụng /list để xem danh sách ứng dụng
Sử dụng /screenshot để chụp màn hình
Sử dụng /notifications để bật/tắt thông báo
Sử dụng /help để xem thêm thông tin";
    }

    private string GetHelpMessage()
    {
        return @"Lệnh có sẵn:
/open <tên_ứng_dụng> - Mở ứng dụng được chỉ định
/list - Hiển thị danh sách ứng dụng có sẵn
/status - Kiểm tra trạng thái bot
/shutdown - Tắt bot (không tắt máy tính)
/screenshot - Chụp ảnh màn hình hiện tại
/notifications on/off - Bật/tắt thông báo về hệ thống
/sysinfo - Hiển thị thông tin hệ thống
/processes - Hiển thị danh sách tiến trình
/killprocess <pid> - Kết thúc tiến trình";
    }

    private async Task<string> HandleShutdown(long chatId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Bot shutdown requested");
        
        // Stop notifications before shutdown
        _notificationService.StopMonitoring();
        
        // Schedule shutdown after sending the message
        _ = Task.Delay(1000, cancellationToken)
            .ContinueWith(_ => Environment.Exit(0), cancellationToken);
        
        return "Đang tắt bot...";
    }

    private string HandleOpenApp(string messageText)
    {
        var parts = messageText.Split(' ');
        if (parts.Length < 2)
        {
            return "Vui lòng chỉ định ứng dụng cần mở. Ví dụ: /open notepad";
        }

        var appName = parts[1].ToLower();
        var (success, message) = _applicationManager.OpenApplication(appName);
        return message;
    }

    private async Task<string> HandleScreenshot(long chatId, CancellationToken cancellationToken)
    {
        try
        {
            var screenshot = ScreenCapture.CaptureScreen();
            
            using var ms = new MemoryStream(screenshot);
            var inputFile = InputFile.FromStream(ms, "screenshot.png");
            
            await _botClient.SendPhotoAsync(
                chatId: chatId,
                photo: inputFile,
                caption: $"Screenshot - {DateTime.Now}",
                cancellationToken: cancellationToken);
            
            return "Ảnh chụp màn hình đã được gửi.";
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error taking screenshot: {ex.Message}");
            return $"Lỗi khi chụp màn hình: {ex.Message}";
        }
    }

    private string HandleNotifications(string[] parts)
    {
        if (parts.Length < 2)
        {
            return "Sử dụng: /notifications on hoặc /notifications off";
        }

        string action = parts[1].ToLower();
        switch (action)
        {
            case "on":
                _notificationService.StartMonitoring();
                return "Đã bật thông báo hệ thống.";
            case "off":
                _notificationService.StopMonitoring();
                return "Đã tắt thông báo hệ thống.";
            default:
                return "Sử dụng: /notifications on hoặc /notifications off";
        }
    }

    private string GetSystemInfo()
    {
        try
        {
            var info = $@"💻 Thông tin hệ thống:
OS: {Environment.OSVersion}
User: {Environment.UserName}
Machine: {Environment.MachineName}
Processors: {Environment.ProcessorCount}
System Directory: {Environment.SystemDirectory}
Working Set: {Environment.WorkingSet / 1024 / 1024} MB";
            
            return info;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting system info: {ex.Message}");
            return $"Lỗi khi lấy thông tin hệ thống: {ex.Message}";
        }
    }

    private string GetRunningProcesses()
    {
        try
        {
            var processes = System.Diagnostics.Process.GetProcesses()
                .OrderByDescending(p => p.WorkingSet64)
                .Take(10)
                .Select(p => 
                {
                    try
                    {
                        return $"{p.Id}: {p.ProcessName} - {p.WorkingSet64 / 1024 / 1024} MB";
                    }
                    catch
                    {
                        return $"{p.Id}: {p.ProcessName} - N/A";
                    }
                })
                .ToList();

            return "🔄 Top 10 tiến trình (theo RAM):\n" + string.Join("\n", processes);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting processes: {ex.Message}");
            return $"Lỗi khi lấy danh sách tiến trình: {ex.Message}";
        }
    }

    private string KillProcess(string[] parts)
    {
        if (parts.Length < 2)
        {
            return "Sử dụng: /killprocess <pid>";
        }

        if (!int.TryParse(parts[1], out int pid))
        {
            return "PID không hợp lệ.";
        }

        try
        {
            var process = System.Diagnostics.Process.GetProcessById(pid);
            string processName = process.ProcessName;
            process.Kill();
            process.WaitForExit(5000);
            
            return $"Đã kết thúc tiến trình {processName} (PID: {pid})";
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error killing process: {ex.Message}");
            return $"Lỗi khi kết thúc tiến trình: {ex.Message}";
        }
    }
}
