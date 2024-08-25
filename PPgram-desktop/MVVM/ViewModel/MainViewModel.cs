using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.IO;
using System.Windows.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using PPgram_desktop.Net;
using PPgram_desktop.Core;
using PPgram_desktop.MVVM.Model;
using PPgram_desktop.MVVM.View;
using System.Windows.Media.Imaging;
using PPgram_desktop.MVVM.Model.DTO;

namespace PPgram_desktop.MVVM.ViewModel;

internal class MainViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    #region bindings
    private Page _currentPage;
    public Page CurrentPage
    {
        get { return _currentPage; }
        set { _currentPage = value; OnPropertyChanged(); }
    }
    private bool _isError;
    public bool IsError
    {
        get { return _isError; }
        set { _isError = value; OnPropertyChanged(); }
    }
    private string _error;
    public string Error
    {
        get { return _error; }
        set { _error = value; OnPropertyChanged(); }
    }
    private bool _disconected;
    public bool Disconnected
    {
        get { return _disconected; }
        set { _disconected = value; OnPropertyChanged(); }
    }
    #endregion

    #region pages
    private readonly LoginPage login_p = new();
    private readonly LoginViewModel login_vm = new();
    private readonly RegPage reg_p = new();
    private readonly RegViewModel reg_vm = new();
    private readonly SettingsPage settings_p = new();
    private readonly SettingsViewModel settings_vm = new();
    private readonly ChatPage chat_p = new();
    private readonly ChatViewModel chat_vm = new();
    #endregion

    #region commands
    public ICommand ConnectCommand { get; set; }
    #endregion

    private readonly string sessionFilePath = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%\\PPgram-desktop\\session.sesf");
    private readonly string connectFilePath = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%\\PPgram-desktop\\connection.txt");
    private readonly string cachePath = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%\\PPgram-desktop\\cache\\");
    private readonly Client client = new();
    private readonly ProfileState profileState = ProfileState.Instance;

    public MainViewModel()
    {
        // commands
        ConnectCommand = new RelayCommand(o => TryConnect());
        // pages
        login_p.DataContext = login_vm;
        reg_p.DataContext = reg_vm;
        settings_p.DataContext = settings_vm;
        chat_p.DataContext = chat_vm;
        // pages events
        login_vm.ToReg += Login_vm_ToReg;
        login_vm.SendLogin += Login_vm_SendLogin;
        reg_vm.ToLogin += Reg_vm_ToLogin;
        reg_vm.SendRegister += Reg_vm_SendRegister;
        reg_vm.SendUsernameCheck += Reg_vm_SendUsernameCheck;
        chat_vm.MessageSent += Chat_vm_MessageSent;
        chat_vm.FetchMessages += Chat_vm_FetchMessages;
        chat_vm.NewChat += Chat_vm_NewChat;
        // client events
        client.Authorized += Client_Authorized;
        client.LoggedIn += Client_LoggedIn;
        client.Registered += Client_Registered;
        client.UsernameChecked += Client_UsernameChecked;
        client.SelfFetched += Client_SelfFetched;
        client.ChatsFetched += Client_ChatsFetched;
        client.Disconnected += Client_Disconnected;
        client.MessagesFetched += Client_MessagesFetched;
        client.NewMessage += Client_NewMessage;
        client.GotNewChat += Client_GotNewChat;
        // initial connection
        TryConnect();
    }

    #region client handlers
    private void Client_GotNewChat(object? sender, GotChatEventArgs e)
    {
        if (e.ok)
        {
            // DEBUG
            BitmapImage ava = new(new Uri("pack://application:,,,/Asset/default_avatar.png", UriKind.Absolute));
            ava.Freeze();
            profileState.Avatar = ava;
            // -----
            ChatModel chat = new()
            {
                Name = e.chat?.Name ?? "",
                Username = e.chat?.Username ?? "",
                Id = e.chat?.Id ?? 0,
                Avatar = ava,
            };
            chat_vm.AddChat(chat);
        }
    }
    private void Client_NewMessage(object? sender, NewMessageEventArgs e)
    {
        if (e.ok)
        {
            MessageModel message = new()
            {
                Id = e.message?.Id ?? 0,
                Chat = e.message?.ChatId ?? 0,
                From = e.message?.From ?? 0,
                // Name
                // Avatar
                Text = e.message?.Text ?? "",
            };
            if (e.message?.From == profileState.Id)
                message.Own = true;
            message.Date = DateTimeOffset.FromUnixTimeSeconds(e.message?.Date ?? 0).DateTime.ToLocalTime().ToString("H:mm");
            chat_vm.AddMessage(message);
        }
    }
    private void Client_MessagesFetched(object? sender, ResponseFetchMessagesEventArgs e)
    {
        if (e.ok)
        {
            // DEBUG
            BitmapImage ava = new(new Uri("pack://application:,,,/Asset/default_avatar.png", UriKind.Absolute));
            ava.Freeze();
            // -----
            ObservableCollection<MessageModel> messages = [];
            foreach (MessageDTO message_dto in e.messages)
            {
                MessageModel message = new()
                {
                    Id = message_dto.Id ?? 0,
                    Chat = message_dto.ChatId ?? 0,
                    From = message_dto.From ?? 0,
                    // Name
                    // Avatar
                    Text = message_dto.Text ?? "",

                };
                if (message_dto.From == profileState.Id)
                    message.Own = true;
                message.Date = DateTimeOffset.FromUnixTimeSeconds(message_dto.Date ?? 0).DateTime.ToLocalTime().ToString("H:mm");
                messages.Add(message);
            }
            chat_vm.UpdateMessages(messages);
        }
        else if (!e.ok)
        {
            ShowError("Unable to fetch messages");
        }
    }
    private void Client_SelfFetched(object? sender, ResponseFetchUserEventArgs e)
    {
        if (e.ok && e.user != null)
        {
            profileState.Name = e.user.Name ?? "";
            profileState.Username = e.user.Username ?? "";
            profileState.Id = e.user.Id ?? 0;
            // DEBUG
            BitmapImage ava = new(new Uri("pack://application:,,,/Asset/default_avatar.png", UriKind.Absolute));
            ava.Freeze();
            profileState.Avatar = ava;
            // -----
            chat_vm.UpdateProfile();
            client.FetchChats();
        }
        else if (!e.ok)
        {
            ShowError("Unable to fetch profile");
        }
    }
    private void Client_ChatsFetched(object? sender, ResponseFetchChatsEventArgs e)
    {
        if (e.ok)
        {
            ObservableCollection<ChatModel> chats = [];
            foreach (ChatDTO chat_dto in e.chats)
            {
                // DEBUG
                BitmapImage ava = new(new Uri("pack://application:,,,/Asset/default_avatar.png", UriKind.Absolute));
                ava.Freeze();
                profileState.Avatar = ava;
                // -----
                ChatModel chat = new()
                {
                    Name = chat_dto.Name ?? "",
                    Username = chat_dto.Username ?? "",
                    Id = chat_dto.Id ?? 0,
                    Avatar = ava,
                    LastMessage = ""
                };
                chats.Add(chat);
            }
            chat_vm.UpdateChats(chats);
        }
        else if (!e.ok)
        {
            ShowError("Unable to fetch chats");
        }
    }
    private void Client_UsernameChecked(object? sender, ResponseUsernameCheckEventArgs e)
    {
        if(e.available)
        {
            reg_vm.ShowUsernameStatus("This username is available", true);
        }
        else if (!e.available)
        {
            reg_vm.ShowUsernameStatus("This username is already taken");
        }
    }
    private void Client_Authorized(object? sender, ResponseSessionEventArgs e)
    {
        if (e.ok)
        {
            LoadChat();
        }
        else if (!e.ok)
        {
            File.Delete(sessionFilePath);
            CurrentPage = login_p;
        }
    }
    private void Client_Registered(object? sender, ResponseSessionEventArgs e)
    {
        if (e.ok)
        {
            CreateAuthFile(e.sessionId ?? "", e.userId ?? 0);
            LoadChat();
        }
        else if (!e.ok)
        {
            ShowError("Something went wrong");
        }
    }
    private void Client_LoggedIn(object? sender, ResponseSessionEventArgs e)
    {
        if (e.ok)
        {
            CreateAuthFile(e.sessionId ?? "", e.userId ?? 0);
            LoadChat();
        }
        else if (!e.ok)
        {
            ShowError("Wrong login or password");
        }
    }
    private void Client_Disconnected(object? sender, EventArgs e)
    {
        Disconnected = true;
    }
    #endregion

    private void CreateAuthFile(string sessionId, int userId)
    {
        string sesdata = sessionId + Environment.NewLine + userId;
        CreateFile(sessionFilePath, sesdata);
    }
    private static void CreateFile(string path, string data)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
        using var writer = new StreamWriter(File.OpenWrite(path));
        writer.Write(data);
    }
    private void LoadChat()
    {
        CurrentPage = chat_p;
        client.FetchSelf();
    }
    private void ShowError(string error)
    {
        Error = error;
        IsError = error != "";
    }
    private void TryConnect()
    {
        if (File.Exists(connectFilePath))
        {
            try
            {
                string[] lines = File.ReadAllLines(connectFilePath);
                string host = lines[0];
                int port = int.Parse(lines[1]);

                // connection
                ShowError("");
                Disconnected = false;
                client.Connect(host, port);

                if (File.Exists(sessionFilePath))
                {
                    try
                    {
                        lines = File.ReadAllLines(sessionFilePath);
                        string session_id = lines[0];
                        int user_id = Int32.Parse(lines[1]);
                        client.AuthorizeWithSessionId(session_id, user_id);
                    }
                    catch
                    {
                        File.Delete(sessionFilePath);
                        ShowError("Unable to load session file");
                    }
                }
                else
                {
                    CurrentPage = login_p;
                }
            }
            catch
            {
                ShowError("Unable to load connection file");
            }
        }
        else
        {
            // DEBUG
            MessageBox.Show("edit connection file appdata/local/PPgram Desktop/");
            // -----
            CreateFile(connectFilePath, "host" + Environment.NewLine + "port");
        }
    }

    #region chat page handlers
    private void Chat_vm_NewChat(object? sender, NewChatEventArgs e)
    {
        client.FetchUser(e.username);
    }
    private void Chat_vm_MessageSent(object? sender, SendMessageEventArgs e)
    {
        client.SendMessage(e.message);
    }
    private void Chat_vm_FetchMessages(object? sender, FetchMessagesEventArgs e)
    {
        client.FetchMessages(e.userId);
    }
    #endregion

    #region Login page handlers
    private void Login_vm_SendLogin(object? sender, LoginEventArgs e)
    {
        client.AuthorizeWithLogin(e.login, e.password);
    }
    private void Login_vm_ToReg(object? sender, EventArgs e)
    {
        CurrentPage = reg_p;
    }
    #endregion

    #region Register page handlers
    private void Reg_vm_SendRegister(object? sender, RegisterEventArgs e)
    {
        client.RegisterNewUser(e.username, e.name, e.password);
    }
    private void Reg_vm_SendUsernameCheck(object? sender, RegisterEventArgs e)
    {
        client.ChekUsernameAvailable(e.username);
    }
    private void Reg_vm_ToLogin(object? sender, EventArgs e)
    {
        CurrentPage = login_p;
    }
    #endregion

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}