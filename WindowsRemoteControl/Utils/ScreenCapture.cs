using System.Drawing;
using System.Drawing.Imaging;

namespace WindowsRemoteControl.Utils;

public class ScreenCapture
{
    public static byte[] CaptureScreen()
    {
        using var bitmap = new Bitmap(
            System.Windows.Forms.SystemInformation.VirtualScreen.Width,
            System.Windows.Forms.SystemInformation.VirtualScreen.Height,
            PixelFormat.Format32bppArgb);

        using var graphics = Graphics.FromImage(bitmap);
        
        graphics.CopyFromScreen(
            System.Windows.Forms.SystemInformation.VirtualScreen.X,
            System.Windows.Forms.SystemInformation.VirtualScreen.Y,
            0, 0,
            System.Windows.Forms.SystemInformation.VirtualScreen.Size,
            CopyPixelOperation.SourceCopy);

        using var ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }
}
