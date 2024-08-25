using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace PPgram_desktop.MVVM.Model;

internal class ChatModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Username { get; set; }
    public BitmapImage Avatar { get; set; }
    public string LastMessage { get; set; }
    public ObservableCollection<MessageModel> Messages { get; set; } = [];
}
