using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using PPgram_desktop.Core;
using PPgram_desktop.MVVM.Model;
using PPgram_desktop.MVVM.Model.DTO;

namespace PPgram_desktop.MVVM.ViewModel;

class ChatViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<SendMessageEventArgs> MessageSent;
    public event EventHandler<FetchMessagesEventArgs> FetchMessages;
    public event EventHandler ScrollToLast;

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
    private BitmapImage _avatar;
    public BitmapImage Avatar
    {
        get { return _avatar; }
        set { _avatar = value; OnPropertyChanged(); }
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
            LoadMessages();
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
    private BitmapImage _chatAvatar;
    public BitmapImage ChatAvatar
    {
        get { return _chatAvatar; }
        set { _chatAvatar = value; OnPropertyChanged(); }
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
                Chat = SelectedChat.Id,
                Text = MessageInput,
                Date = DateTime.Now.ToString("H:mm"),

                First = false,
                Last = false,
                Own = true
            };
            ChatMessages.Add(message);
            MessageInput = "";
            MessageSent?.Invoke(this, new SendMessageEventArgs
            {
                message = new MessageDTO
                {
                    Id = message.Id,
                    ChatId = message.Chat,
                    Text = message.Text,
                    ReplyTo = 0
                }
            });
            ScrollToLast?.Invoke(this, EventArgs.Empty);
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
    private void LoadMessages()
    {
        ChatName = SelectedChat.Name;
        ChatUsername = SelectedChat.Username;
        ChatAvatar = SelectedChat.Avatar;
        if (SelectedChat.Messages.Count != 0)
        {
            ChatMessages = SelectedChat.Messages;
        }  
        else
        {
            FetchMessages?.Invoke(this, new FetchMessagesEventArgs
            {
                userId = SelectedChat.Id
            });
        }    
    }

    public void UpdateProfile()
    {
        Name = profile.Name;
        Username = profile.Username;
        Avatar = profile.Avatar;
    }
    public void UpdateChats(ObservableCollection<ChatModel> chats)
    {
        Chats = chats;
    }
    public void UpdateMessages(ObservableCollection<MessageModel> messages)
    {
        ChatMessages = SelectedChat.Messages = messages;
    }
    public void AddMessage(MessageModel message)
    {
        foreach (ChatModel chat in Chats)
        {
            if (chat.Id == message.From)
            {
                Application.Current.Dispatcher.Invoke(() => {
                    chat.Messages.Add(message);
                });
                LoadMessages();
                ScrollToLast?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
class SendMessageEventArgs : EventArgs
{
    public MessageDTO message;
}
class FetchMessagesEventArgs : EventArgs
{
    public int userId;
}