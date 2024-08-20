using System.Collections.ObjectModel;

namespace PPgram_desktop.MVVM.Model;

internal sealed class ChatState
{
    private static readonly Lazy<ChatState> lazy = new(() => new ChatState());
    public static ChatState Instance => lazy.Value;
    private ChatState() { }

    public string Name { get; set; } = "";
    public string Username { get; set; } = "";
    public string AvatarSource { get; set; } = "";
    public string Id { get; set; } = "";
    public ObservableCollection<UserModel> Chats { get; set; }
    public ObservableCollection<MessageModel> ChatMessages { get; set; }
}
