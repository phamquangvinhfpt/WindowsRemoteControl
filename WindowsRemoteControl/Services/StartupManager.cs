using Microsoft.Win32;
using System.Security.Principal;

namespace WindowsRemoteControl.Services;

public class StartupManager
{
    private const string APP_NAME = "WindowsRemoteControl";
    private const string REGISTRY_KEY_PATH = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    public void RegisterForStartup()
    {
        try
        {
            // if (!IsAdministrator())
            // {
            //     Console.WriteLine("Cần quyền quản trị để đăng ký khởi động cùng Windows.");
            //     Console.WriteLine("Vui lòng chạy ứng dụng này với quyền quản trị.");
            //     Console.WriteLine("Nhấn Enter để thoát...");
            //     Console.ReadLine();
            //     return;
            // }

            string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string appDirectory = Path.GetDirectoryName(appPath) ?? string.Empty;

            // Get the .exe path for non-single-file publish
            string exePath = appPath.EndsWith(".dll")
                ? Path.Combine(appDirectory, $"{APP_NAME}.exe")
                : appPath;

            if (!File.Exists(exePath))
            {
                Console.WriteLine($"Không tìm thấy file exe tại: {exePath}");
                return;
            }

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY_PATH, true)
                                    ?? Registry.CurrentUser.CreateSubKey(REGISTRY_KEY_PATH))
            {
                if (key != null)
                {
                    key.SetValue(APP_NAME, $"\"{exePath}\"");
                    Console.WriteLine("Đã đăng ký thành công. Ứng dụng sẽ khởi động cùng Windows.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi khi đăng ký khởi động: {ex.Message}");
        }
    }

    public void UnregisterFromStartup()
    {
        try
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY_PATH, true))
            {
                if (key != null)
                {
                    if (key.GetValue(APP_NAME) != null)
                    {
                        key.DeleteValue(APP_NAME, false);
                        Console.WriteLine("Đã hủy đăng ký thành công.");
                    }
                    else
                    {
                        Console.WriteLine("Chưa có đăng ký nào để hủy.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi khi hủy đăng ký: {ex.Message}");
        }
    }

    private bool IsAdministrator()
    {
        return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
            .IsInRole(WindowsBuiltInRole.Administrator);
    }
}
