using System.IO;
using System.Windows.Media.Imaging;

namespace PPgram_desktop.Core;

internal class Base64ToBitmapConverter
{
    public static BitmapImage ConvertBase64(string base64String)
    {
        byte[] imageBytes = Convert.FromBase64String(base64String);
        BitmapImage bitmapImage = new();
        using (MemoryStream ms = new(imageBytes))
        {
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = ms;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
        }
        return bitmapImage;
    }
}


