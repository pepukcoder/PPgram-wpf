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

namespace PPgram_desktop.MVVM.ViewModel;

internal class MainViewModel : INotifyPropertyChanged
{
    private readonly string sessionFilePath = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%\\PPgram-desktop\\session.sesf");
    private readonly string cachePath = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%\\PPgram-desktop\\cache\\");

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
    private bool _retryConnect;
    public bool RetryConnect;
    #endregion

    #region pages
    private LoginPage login_p = new();
    private LoginViewModel login_vm = new();
    private RegPage reg_p = new();
    private RegViewModel reg_vm = new();
    private SettingsPage settings_p = new();
    private SettingsViewModel settings_vm = new();
    private ChatPage chat_p = new();
    private ChatViewModel chat_vm = new();
    #endregion

    #region commands
    public ICommand ConnectCommand { get; set; }
    #endregion

    private readonly Client client = new();
    private ProfileState profile = ProfileState.Instance;
    private ChatState chat = ChatState.Instance;

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

        // client events
        client.Authorized += Client_Authorized;
        client.LoggedIn += Client_LoggedIn;
        client.Registered += Client_Registered;
        client.UsernameChecked += Client_UsernameChecked;
        client.SelfFetched += Client_SelfFetched;
        client.Disconnected += Client_Disconnected;

        CurrentPage = login_p;
        TryConnect();
    }

    #region client handlers
    private void Client_SelfFetched(object? sender, ResponseFetchUserEventArgs e)
    {
        if (e.ok)
        {
            if (String.IsNullOrEmpty(e.avatarData))
            {
                profile.AvatarSource = "Asset/default_avatar.png";
            }
            else
            {
                string avatarPath = cachePath + e.username + e.avatarFormat;
                CreateFile(avatarPath, e.avatarData);
                profile.AvatarSource = avatarPath;
            }
            profile.Name = e.name ?? "";
            profile.Username = e.username ?? "";
            profile.Id = e.userId ?? 0;
            chat_vm.UpdateProfile();
        }
        else if (!e.ok)
        {
            ShowError("Unable to fetch profile");
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
    private void Client_Authorized(object? sender, ResponseAuthEventArgs e)
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
    private void Client_Registered(object? sender, ResponseRegisterEventArgs e)
    {
        if (e.ok)
        {
            CreateAuthFile(e.sessionId ?? "", e.userId ?? 0);
            LoadChat();
        }
        else if (!e.ok)
        {
            reg_vm.ShowError("Something went wrong");
        }
    }
    private void Client_LoggedIn(object? sender, ResponseLoginEventArgs e)
    {
        if (e.ok)
        {
            CreateAuthFile(e.sessionId ?? "", e.userId ?? 0);
            LoadChat();
        }
        else if (!e.ok)
        {
            login_vm.ShowError("Wrong login or password");
        }
    }
    private void Client_Disconnected(object? sender, EventArgs e)
    {
        ShowError("Unable to connect to the server");
        RetryConnect = true;
    }
    #endregion

    private void CreateAuthFile(string sessionId, int userId)
    {
        string sesdata = sessionId + Environment.NewLine + userId;
        CreateFile(sessionFilePath, sesdata);
    }
    private void CreateFile(string path, string data)
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
        // connection
        ShowError("");
        RetryConnect = false;
        client.Connect("127.0.0.1", 8080);
        // authorization
        if (File.Exists(sessionFilePath))
        {
            try
            {
                string[] lines = File.ReadAllLines(sessionFilePath);
                string session_id = lines[0];
                int user_id = Int32.Parse(lines[1]);
                client.AuthorizeWithSessionId(session_id, user_id);
            }
            catch
            {
                ShowError("Unable to load session file");
            }
        }
        else
        {
            CurrentPage = login_p;
        }
    }

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
    private void Reg_vm_SendUsernameCheck(object? sender, CheckUsernameEventArgs e)
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