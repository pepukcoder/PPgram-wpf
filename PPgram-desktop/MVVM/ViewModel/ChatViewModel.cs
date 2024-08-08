using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PPgram_desktop.Core;
using PPgram_desktop.MVVM.Model;

namespace PPgram_desktop.MVVM.ViewModel;

class ChatViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    #region bindings
    public ObservableCollection<ChatMessageModel> ChatMessages { get; set; }
    public ChatMessageModel SelectedMessage { get; set; }
    private string _messageInput;
    public string MessageInput
    {
        get { return _messageInput; }
        set { _messageInput = value; OnPropertyChanged(); }
    }
    #endregion

    #region commands
    public ICommand SendMessageCommand { get; set; }
    public ICommand DownloadFileCommand { get; set; }
    #endregion

    public ChatViewModel()
    {
        SendMessageCommand = new RelayCommand(o => SendMessage());
        DownloadFileCommand = new RelayCommand(o => DownloadFile());
        ChatMessages = [];
        SelectedMessage = new();
        _messageInput = "";

        ChatMessages.Add(new ChatMessageModel
        {
            Name = "Pavlo",
            Text = "ок сообщение тест",
            Date = DateTime.Now.ToString("H:mm"),

            IsFirst = true,
            IsLast = false,
        });
        ChatMessages.Add(new ChatMessageModel
        {
            Name = "Pavlo",
            Text = "тест ответа",
            Reply = true,
            ReplyText = "ок сообщение тест",

            Date = DateTime.Now.ToString("H:mm"),

            IsFirst = false,
            IsLast = true,
        });
    }
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private void SendMessage()
    {
        if (MessageInput.Trim() != "")
        {
            ChatMessages.Add(new ChatMessageModel
            {
                Name = "Pepuk",
                Text = MessageInput,
                Date = DateTime.Now.ToString("H:mm"),

                IsFirst = false,
                IsLast = false,
                IsOwn = true
            });
            MessageInput = "";
        }
    }
    private void DownloadFile()
    {
        MessageBox.Show("requestdownloadfile");
    }
}

