using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using PPgram_desktop.Core;
using PPgram_desktop.MVVM.Model;

namespace PPgram_desktop.MVVM.ViewModel;

class ChatViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    #region bindings
    public ObservableCollection<MessageModel> ChatMessages { get; set; }
    public MessageModel SelectedMessage { get; set; }
    private string _messageInput;
    public string MessageInput
    {
        get { return _messageInput; }
        set { _messageInput = value; OnPropertyChanged(); }
    }
    #endregion

    #region commands
    public ICommand SendMessageCommand { get; set; }
    //public ICommand DownloadFileCommand { get; set; }
    #endregion

    public ChatViewModel()
    {
        SendMessageCommand = new RelayCommand(o => SendMessage());
        //DownloadFileCommand = new RelayCommand(o => DownloadFile());

        ChatMessages = [];
        SelectedMessage = new();
        _messageInput = "";
    }
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private void SendMessage()
    {
        if (MessageInput.Trim() != "")
        {
            /* TODO
            ChatMessages.Add(new ChatMessageModel
            {
                Name = PROFILE NAME,
                Text = MessageInput,
                Date = DateTime.Now.ToString("H:mm"),

                IsFirst = CHECK,
                IsLast = CHECK,
                IsOwn = true
            });
            MessageInput = "";
            client.SendMessage(message);
            */
        }
    }
    private void DownloadFile()
    {

    }
}