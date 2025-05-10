using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.Diagnostics;
using WindowsRemoteControl.Configuration;

namespace WindowsRemoteControl.Services;

public class ApplicationManager
{
    private readonly ILogger<ApplicationManager> _logger;
    private readonly BotConfiguration _config;
    private readonly Dictionary<string, string> _installedApps;

    public ApplicationManager(ILogger<ApplicationManager> logger, BotConfiguration config)
    {
        _logger = logger;
        _config = config;
        _installedApps = SearchInstalledApps();
    }

    public Dictionary<string, string> GetAllApplications()
    {
        var result = new Dictionary<string, string>(_installedApps);
        
        // Add common apps, don't overwrite existing keys (prefer registry paths)
        foreach (var app in _config.CommonApps)
        {
            if (!result.ContainsKey(app.Key))
            {
                result[app.Key] = app.Value;
            }
        }
        
        return result;
    }

    public string GetApplicationList()
    {
        var allApps = GetAllApplications().Keys.OrderBy(app => app);
        return "Ứng dụng có sẵn:\n" + string.Join("\n", allApps);
    }

    public (bool success, string message) OpenApplication(string appName)
    {
        var allApps = GetAllApplications();

        if (allApps.TryGetValue(appName.ToLower(), out var path))
        {
            bool success = false;
            string message = string.Empty;
            
            try
            {
                // Replace username placeholder if present
                if (path.Contains("Quang Vinh"))
                {
                    path = path.Replace("Quang Vinh", Environment.UserName);
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });

                success = true;
                message = $"Đang mở {appName}...";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error opening {appName}: {ex.Message}");
                success = false;
                message = $"Lỗi khi mở {appName}: {ex.Message}";
            }
            finally
            {
                // check if the process is running
                var processName = Path.GetFileNameWithoutExtension(path);
                var processes = Process.GetProcessesByName(processName);
                if (processes.Length > 0)
                {
                    success = true;
                    message = $"Đã mở {appName} thành công.";
                }
                else if (success) // Only override if we think it succeeded but process not found
                {
                    success = false;
                    message = $"Không thể mở {appName}. Vui lòng kiểm tra lại.";
                }
            }
            
            return (success, message);
        }

        return (false, $"Không tìm thấy ứng dụng '{appName}'. Sử dụng /list để xem danh sách ứng dụng.");
    }

    private Dictionary<string, string> SearchInstalledApps()
    {
        var installedApps = new Dictionary<string, string>();

        try
        {
            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths"))
            {
                if (key != null)
                {
                    foreach (var appName in key.GetSubKeyNames())
                    {
                        using (var appKey = key.OpenSubKey(appName))
                        {
                            if (appKey != null)
                            {
                                var path = appKey.GetValue("") as string;
                                if (!string.IsNullOrEmpty(path))
                                {
                                    var baseName = Path.GetFileNameWithoutExtension(appName).ToLower();
                                    installedApps[baseName] = path;
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error searching registry: {ex.Message}");
        }

        return installedApps;
    }
}
