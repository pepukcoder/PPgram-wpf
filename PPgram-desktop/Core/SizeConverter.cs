namespace PPgram_desktop.Core;

internal class SizeConverter
{
    static readonly string[] SizeSuffixes = ["bytes", "KB", "MB", "GB"];

    public static string ConvertBytes(Int64 value)
    {
        if (value == 0) return "0 bytes";
        if (value < 0) return "-" + ConvertBytes(-value);
        int i = 0;
        decimal dValue = (decimal)value;
        while (Math.Round(dValue, 1) >= 1000)
        {
            dValue /= 1000;
            i++;
        }
        return string.Format("{0:n1} {1}", dValue, SizeSuffixes[i]);
    }
}
