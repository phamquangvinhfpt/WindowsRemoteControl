namespace WindowsRemoteControl.Configuration;

public class BotConfiguration
{
    public string Token { get; } = "YOUR_BOT_TOKEN";
    public long[] AuthorizedUsers { get; } = new[] { YOUR_USER_ID };
    
    public Dictionary<string, string> CommonApps { get; } = new()
    {
        ["chrome"] = @"C:\Program Files\Google\Chrome\Application\chrome.exe",
        ["firefox"] = @"C:\Program Files\Mozilla Firefox\firefox.exe",
        ["notepad"] = "notepad.exe",
        ["calc"] = "calc.exe",
        ["explorer"] = "explorer.exe",
        ["cmd"] = "cmd.exe",
        ["word"] = @"C:\Program Files\Microsoft Office\root\Office16\WINWORD.EXE",
        ["excel"] = @"C:\Program Files\Microsoft Office\root\Office16\EXCEL.EXE",
        ["powerpoint"] = @"C:\Program Files\Microsoft Office\root\Office16\POWERPNT.EXE",
        ["vscode"] = @"C:\Users\Quang Vinh\AppData\Local\Programs\Microsoft VS Code\Code.exe",
        ["parsec"] = @"C:\Program Files\Parsec\parsecd.exe"
    };
}
