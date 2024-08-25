using System.Windows.Media.Imaging;

namespace PPgram_desktop.MVVM.Model;

internal sealed class ProfileState
{
    private static readonly Lazy<ProfileState> lazy = new(() => new ProfileState());
    public static ProfileState Instance => lazy.Value;
    private ProfileState() { }

    public string Name { get; set; }
    public string Username { get; set; }
    public BitmapImage Avatar { get; set; }
    public int Id { get; set; }

}
