using System.Text;

namespace WindowsRemoteControl.Utils;

public static class KeyCodeTranslator
{
    public static string Translate(int vkCode)
    {
        // Handle special keys
        switch (vkCode)
        {
            case 8: return "[BACKSPACE]";
            case 9: return "[TAB]";
            case 13: return "[ENTER]";
            case 16: return "[SHIFT]";
            case 17: return "[CTRL]";
            case 18: return "[ALT]";
            case 19: return "[PAUSE]";
            case 20: return "[CAPS LOCK]";
            case 27: return "[ESC]";
            case 32: return " ";
            case 33: return "[PAGE UP]";
            case 34: return "[PAGE DOWN]";
            case 35: return "[END]";
            case 36: return "[HOME]";
            case 37: return "[←]";
            case 38: return "[↑]";
            case 39: return "[→]";
            case 40: return "[↓]";
            case 44: return "[PRINT SCREEN]";
            case 45: return "[INSERT]";
            case 46: return "[DELETE]";
            case 91: return "[WIN]";
            case 92: return "[WIN]";
            case 93: return "[MENU]";
            case 144: return "[NUM LOCK]";
            case 145: return "[SCROLL LOCK]";
            case 186: return ";";
            case 187: return "=";
            case 188: return ",";
            case 189: return "-";
            case 190: return ".";
            case 191: return "/";
            case 192: return "`";
            case 219: return "[";
            case 220: return "\\";
            case 221: return "]";
            case 222: return "'";
            
            // Function keys
            case 112: return "[F1]";
            case 113: return "[F2]";
            case 114: return "[F3]";
            case 115: return "[F4]";
            case 116: return "[F5]";
            case 117: return "[F6]";
            case 118: return "[F7]";
            case 119: return "[F8]";
            case 120: return "[F9]";
            case 121: return "[F10]";
            case 122: return "[F11]";
            case 123: return "[F12]";
            
            // Numpad
            case 96: return "0";
            case 97: return "1";
            case 98: return "2";
            case 99: return "3";
            case 100: return "4";
            case 101: return "5";
            case 102: return "6";
            case 103: return "7";
            case 104: return "8";
            case 105: return "9";
            case 106: return "*";
            case 107: return "+";
            case 109: return "-";
            case 110: return ".";
            case 111: return "/";
        }

        // Try to get the character using ToUnicode
        StringBuilder result = new StringBuilder(2);
        byte[] keyboardState = new byte[256];
        GetKeyboardState(keyboardState);
        
        uint scanCode = MapVirtualKey((uint)vkCode, 0);
        int chars = ToUnicode((uint)vkCode, scanCode, keyboardState, result, result.Capacity, 0);
        
        if (chars > 0)
        {
            return result.ToString();
        }
        
        // Fallback to simple character mapping
        if ((vkCode >= 48 && vkCode <= 57) || (vkCode >= 65 && vkCode <= 90))
        {
            return ((char)vkCode).ToString();
        }
        
        return $"[{vkCode}]";
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    static extern int ToUnicode(uint virtualKeyCode, uint scanCode,
        byte[] keyboardState, [System.Runtime.InteropServices.Out, System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr, SizeConst = 64)]
        StringBuilder result, int resultLength, uint flags);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    static extern bool GetKeyboardState(byte[] keyState);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    static extern uint MapVirtualKey(uint uCode, uint uMapType);
}
