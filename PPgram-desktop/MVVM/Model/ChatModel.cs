using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace PPgram_desktop.MVVM.Model;

internal class ChatModel
{
    [JsonPropertyName("id")]
    public int ID { get; set; }
    [JsonPropertyName("username")]
    public string Username { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    public string AvatarSource { get; set; }
    public string LastMessage { get; set; }

    public ObservableCollection<MessageModel> Messages { get; set; } = [];
}
