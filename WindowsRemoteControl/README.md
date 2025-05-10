# Windows Remote Control

Ứng dụng điều khiển máy tính Windows từ xa qua Telegram Bot.

## Tính năng

- Mở ứng dụng Windows từ xa qua Telegram
- Chụp màn hình từ xa
- Thông báo hệ thống real-time
- Quản lý tiến trình
- Xem thông tin hệ thống
- Bảo mật dữ liệu cấu hình
- Đăng ký chạy cùng Windows
- Phân quyền người dùng bảo mật

## Cách sử dụng

### Chạy ứng dụng

```bash
# Chạy thông thường
dotnet run

# Đăng ký chạy cùng Windows (cần quyền admin)
dotnet run -- --register

# Hủy đăng ký khởi động cùng Windows
dotnet run -- --unregister
```

### Lệnh Telegram

#### Quản lý cơ bản
- `/start` - Khởi động bot
- `/help` - Hiển thị trợ giúp
- `/status` - Kiểm tra trạng thái
- `/shutdown` - Tắt bot

#### Quản lý ứng dụng
- `/open <app>` - Mở ứng dụng (vd: `/open notepad`)
- `/list` - Hiển thị danh sách ứng dụng

#### Giám sát hệ thống
- `/screenshot` - Chụp ảnh màn hình
- `/sysinfo` - Hiển thị thông tin hệ thống
- `/processes` - Hiển thị danh sách tiến trình
- `/killprocess <pid>` - Kết thúc tiến trình

#### Thông báo
- `/notifications on/off` - Bật/tắt thông báo hệ thống

## Cấu hình

1. Thay đổi Telegram Bot Token trong file `Configuration/BotConfiguration.cs`:
   ```csharp
   public string Token { get; } = "YOUR_BOT_TOKEN";
   ```

2. Thêm ID Telegram của bạn vào danh sách người dùng cho phép:
   ```csharp
   public long[] AuthorizedUsers { get; } = new[] { YOUR_TELEGRAM_USER_ID };
   ```

## Lưu trữ dữ liệu

- **Cấu hình bảo mật**: `%AppData%\WindowsRemoteControl\Config\secure.config` (mã hóa)

## Tối ưu hiệu năng

- Ứng dụng hoạt động ổn định không gây lag
- Screenshot tối ưu hiệu suất
- Giám sát tiến trình hiệu quả

## Build

```bash
# Build debug
dotnet build

# Build release
dotnet build -c Release

# Publish standalone executable
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

## Lưu ý bảo mật

Ứng dụng này có khả năng chụp màn hình và giám sát hệ thống, vì vậy chỉ nên sử dụng cho mục đích hợp pháp và trên máy tính của chính bạn. Hãy đảm bảo:

1. Token bot được bảo mật
2. Chỉ thêm ID Telegram đáng tin vào danh sách cho phép
3. Sử dụng ở nơi an toàn, tránh vi phạm quyền riêng tư của người khác
4. Cấu hình được mã hóa AES để bảo vệ thông tin nhạy cảm

## Thông báo hệ thống

Bot có thể tự động thông báo về:
- Tiến trình hệ thống quan trọng (cmd, powershell, msi, installer)
- Hoạt động đăng nhập
- Các sự kiện hệ thống quan trọng khác
