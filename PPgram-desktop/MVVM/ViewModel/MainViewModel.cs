using System.Collections.ObjectModel;
using System.Windows.Input;
using System.IO;
using System.Windows.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using PPgram_desktop.Net;
using PPgram_desktop.Core;
using PPgram_desktop.MVVM.Model;
using PPgram_desktop.MVVM.View;
using System.Windows;

namespace PPgram_desktop.MVVM.ViewModel;

internal class MainViewModel : INotifyPropertyChanged
{
    private readonly string sessionFilePath = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%\\PPgram-desktop\\session.sesf");

    public event PropertyChangedEventHandler? PropertyChanged;

    #region bindings
    public ObservableCollection<UserModel> Chats { get; set; }
    public UserModel SelectedChat { get; set; }
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
    private Page _currentPage;
    public Page CurrentPage
    {
        get { return _currentPage; }
        set { _currentPage = value; OnPropertyChanged(); }
    }
    #endregion

    #region pages
    private LoginPage login_p = new();
    private LoginViewModel login_vm = new();
    private RegPage reg_p = new();
    private RegViewModel reg_vm = new();
    private SettingsPage settings_p = new();
    private SettingsViewModel settings_vm = new();
    private ChatPage chat_p = new();
    #endregion

    #region commands
    public ICommand SettingsButtonCommand { get; set; }
    public ICommand NewChatCommand { get; set; }
    #endregion

    private readonly Client client;

    public MainViewModel()
    {
        // commands
        SettingsButtonCommand = new RelayCommand(o => SettingsButton_Click());
        NewChatCommand = new RelayCommand(o => NewChat_Enter());

        // sidebar
        _newChatName = "";
        SelectedChat = new();
        Chats = [];

        // pages
        login_p.DataContext = login_vm;
        reg_p.DataContext = reg_vm;
        settings_p.DataContext = settings_vm;
        chat_p.DataContext = this;

        // pages events
        login_vm.ToReg += Login_vm_ToReg;
        login_vm.SendLogin += Login_vm_SendLogin;
        reg_vm.ToLogin += Reg_vm_ToLogin;
        reg_vm.SendRegister += Reg_vm_SendRegister;

        // connection
        client = new();
        bool connected = false;
        while (!connected)
        {
            try
            {
                client.Connect("127.0.0.1", 8080);
                connected = true;
            }
            catch { }
        }

        // client events
        client.Authorized += Client_Authorized;
        client.Registered += Client_Registered;
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
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error",MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }
        else
        {
            SidebarVisible = false;
            CurrentPage = login_p;
        }
    }

    #region client handlers
    private void Client_Authorized(object? sender, ResponseAuthEventArgs e)
    {
        if (e.ok)
        {
            LoadChat();
        }
        else if (!e.ok)
        {
            File.Delete(sessionFilePath);
            SidebarVisible = false;
            CurrentPage = login_p;
        }
    }
    private void Client_MessageRecieved(object? sender, IncomeMessageEventArgs e)
    {
        foreach (UserModel chat in Chats)
        {
            if (chat.ID == e.sender_id && e.message != null)
            {
                chat.Messages.Add(e.message);
            }
        }
    }
    private void Client_Registered(object? sender, ResponseRegisterEventArgs e)
    {
        if (e.ok)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(sessionFilePath) ?? string.Empty);
            using var writer = new StreamWriter(File.OpenWrite(sessionFilePath));
            writer.WriteLine(e.sessionId);
            writer.WriteLine(e.userId);

            LoadChat();
        }
        else if (!e.ok)
        {
            reg_vm.ShowError(e.error);
        }
    }
    #endregion
    private void SettingsButton_Click()
    {
        
    }
    private void NewChat_Enter()
    {
        NewChatName = "";
    }
    private void LoadChat()
    {
        SidebarVisible = true;
        CurrentPage = chat_p;
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
    void Reg_vm_ToLogin(object? sender, EventArgs e)
    {
        CurrentPage = login_p;
    }
    #endregion

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}