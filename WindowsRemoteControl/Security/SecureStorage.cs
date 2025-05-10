using System.Text.Json;
using WindowsRemoteControl.Configuration;

namespace WindowsRemoteControl.Security;

public class SecureStorage
{
    private readonly string _configPath;
    private readonly string _encryptionKey;

    public SecureStorage()
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string configDir = Path.Combine(appDataPath, "WindowsRemoteControl", "Config");
        Directory.CreateDirectory(configDir);
        
        _configPath = Path.Combine(configDir, "secure.config");
        
        // Use machine-specific key for encryption
        _encryptionKey = Environment.MachineName + Environment.UserName;
    }

    public void SaveConfig(BotConfiguration config)
    {
        var configData = new
        {
            Token = config.Token,
            AuthorizedUsers = config.AuthorizedUsers
        };
        
        string json = JsonSerializer.Serialize(configData, new JsonSerializerOptions { WriteIndented = true });
        string encrypted = EncryptionHelper.Encrypt(json, _encryptionKey);
        
        File.WriteAllText(_configPath, encrypted);
    }

    public (string Token, long[] AuthorizedUsers)? LoadConfig()
    {
        if (!File.Exists(_configPath))
            return null;

        try
        {
            string encrypted = File.ReadAllText(_configPath);
            string json = EncryptionHelper.Decrypt(encrypted, _encryptionKey);
            
            var config = JsonSerializer.Deserialize<ConfigData>(json);
            if (config == null)
                return null;

            return (config.Token, config.AuthorizedUsers);
        }
        catch
        {
            return null;
        }
    }

    private class ConfigData
    {
        public string Token { get; set; } = string.Empty;
        public long[] AuthorizedUsers { get; set; } = Array.Empty<long>();
    }
}
