using System.Collections.ObjectModel;

namespace PPgram_desktop.MVVM.Model;

internal class UserModel
{
    public int ID { get; set; }
    public string Username { get; set; } = "";
    public string Name { get; set; } = "";
    public string AvatarSource { get; set; } = "";
    public string LastMessage { get; set; } = "";

    public ObservableCollection<MessageModel> Messages { get; set; } = [];
}
