using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

using PPgram_desktop.Core;
using PPgram_desktop.MVVM.Model;

namespace PPgram_desktop.MVVM.ViewModel;

class ChatViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<SendMessageEventArgs> MessageSent;

    #region bindings
    // User badge
    private string _name = "";
    public string Name
    {
        get { return _name; }
        set { _name = value; OnPropertyChanged(); }
    }
    private string _username = "";
    public string Username
    {
        get { return _username; }
        set { _username = value; OnPropertyChanged(); }
    }
    private string _avatarSource = "Asset\\default_avatar.png";
    public string AvatarSource
    {
        get { return _avatarSource; }
        set { _avatarSource = value; OnPropertyChanged(); }
    }
    // Sidebar
    private ObservableCollection<ChatModel> _chats;
    public ObservableCollection<ChatModel> Chats
    {
        get => _chats;
        set
        {
            _chats = value;
            OnPropertyChanged();
        }
    }

    private ChatModel _selectedChat;
    public ChatModel SelectedChat
    { 
        get => _selectedChat;
        set
        { 
            _selectedChat = value;
            OnPropertyChanged();
            LoadMessages(_selectedChat);
        }
    }
    private string _newChatName;
    public string NewChatName
    {
        get { return _newChatName; }
        set { _newChatName = value; OnPropertyChanged(); }
    }
    private bool _sidebarVisible;
    public bool SidebarVisible
    {
        get { return _sidebarVisible; }
        set { _sidebarVisible = value; OnPropertyChanged(); }
    }
    // Messages
    public ObservableCollection<MessageModel> _chatMessages;
    public ObservableCollection<MessageModel> ChatMessages
    { 
        get => _chatMessages; 
        set { _chatMessages = value; OnPropertyChanged(); }
    }
    public MessageModel SelectedMessage { get; set; }
    private string _messageInput;
    public string MessageInput
    {
        get { return _messageInput; }
        set { _messageInput = value; OnPropertyChanged(); }
    }
    // Chat info
    private string _chatName = "";
    public string ChatName
    {
        get { return _chatName; }
        set { _chatName = value; OnPropertyChanged(); }
    }
    private string _chatUsername = "";
    public string ChatUsername
    {
        get { return _chatUsername; }
        set { _chatUsername = value; OnPropertyChanged(); }
    }
    private string _chatAvatarSource = "Asset\\default_avatar.png";
    public string ChatAvatarSource
    {
        get { return _chatAvatarSource; }
        set { _chatAvatarSource = value; OnPropertyChanged(); }
    }

    #endregion

    #region commands
    public ICommand SendMessageCommand { get; set; }
    public ICommand ShowProfileCommand { get; set; }
    public ICommand SettingsButtonCommand { get; set; }
    public ICommand NewChatCommand { get; set; }
    //public ICommand DownloadFileCommand { get; set; }
    #endregion

    private ProfileState profile = ProfileState.Instance;
    private ChatState chatState = ChatState.Instance;
    private ObservableCollection<ChatModel> chats;
    private ObservableCollection<MessageModel> chatMessages;

    public ChatViewModel()
    {
        // commands
        SendMessageCommand = new RelayCommand(o => SendMessage());
        ShowProfileCommand = new RelayCommand(o => ShowProfile());
        SettingsButtonCommand = new RelayCommand(o => SettingsButton_Click());
        NewChatCommand = new RelayCommand(o => NewChat_Enter());
        //DownloadFileCommand = new RelayCommand(o => DownloadFile());

        // sidebar
        SidebarVisible = true;
        _newChatName = "";
        SelectedChat = new();
        Chats = [];

        // chat
        ChatMessages = [];
        SelectedMessage = new();
        _messageInput = "";
    }
    
    private void SendMessage()
    {
        if (MessageInput.Trim() != "")
        {
            MessageModel message = new MessageModel
            {
                Name = "Pepuk",
                From = profile.Id,
                To = 1045866742,
                Text = MessageInput,
                Date = DateTime.Now.ToString("H:mm"),

                IsFirst = false,
                IsLast = false,
                IsOwn = true
            };
            ChatMessages.Add(message);
            MessageInput = "";
            MessageSent?.Invoke(this, new SendMessageEventArgs
            {
                message = message
            });
        }
    }
    private void SettingsButton_Click()
    {

    }
    private void NewChat_Enter()
    {
        NewChatName = "";
    }
    private void ShowProfile()
    {

    }
    private void DownloadFile()
    {

    }
    private void LoadMessages(ChatModel selected_chat)
    {
        ChatName = selected_chat.Name;
        ChatUsername = selected_chat.Username;
        ChatAvatarSource = selected_chat.AvatarSource;
        ChatMessages = selected_chat.Messages;
    }

    public void UpdateProfile()
    {
        Name = profile.Name;
        Username = profile.Username;
        AvatarSource = profile.AvatarSource;
    }
    public void UpdateChat()
    {
        Chats = chatState.Chats;
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
class SendMessageEventArgs : EventArgs
{
    public MessageModel message { get; set; }
}