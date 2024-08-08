using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PPgram_desktop.Net;
using PPgram_desktop.Core;
using PPgram_desktop.MVVM.Model;
using PPgram_desktop.MVVM.View;
using System.Configuration;

namespace PPgram_desktop.MVVM.ViewModel;

internal class MainViewModel : INotifyPropertyChanged
{
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
    #endregion

    #region commands
    public ICommand SettingsButtonCommand { get; set; }
    public ICommand NewChatCommand { get; set; }
    #endregion

    private readonly Client client;


    public MainViewModel()
    {
        SettingsButtonCommand = new RelayCommand(o => SettingsButton_Click());
        NewChatCommand = new RelayCommand(o => NewChat_Enter());

        _newChatName = "";
        SelectedChat = new();
        Chats = [];

        login_p.DataContext = login_vm;
        reg_p.DataContext = reg_vm;

        login_vm.ToReg += Login_vm_ToReg;
        login_vm.SendLogin += Login_vm_SendLogin;
        reg_vm.ToLogin += Reg_vm_ToLogin;

        _sidebarVisible = true;
        _currentPage = login_p;

        client = new();
        client.Connect("127.0.0.1", 8080);
        Chats.Add(new UserModel
        {
            Name = "Pavlo",
            AvatarSource = "\\Asset\\default_avatar.png",
            LastMessage = "last message",
        });
        Chats.Add(new UserModel
        {
            Name = "Artem Pidor",
            AvatarSource = "\\Asset\\default_avatar.png",
            LastMessage = "last message",
        });
    }

    private void Login_vm_SendLogin(object? sender, LoginEventArgs e)
    {
        client.AuthorizeWithLogin(e.login, e.password);
    }

    private void Client_MessageRecieved(object? sender, MessageEventArgs e)
    {
        foreach (UserModel chat in Chats)
        {
            if (chat.ID == e.sender_id && e.message != null)
            {
                chat.Messages.Add(e.message);
            }
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private void SettingsButton_Click()
    {
        
    }

    private void NewChat_Enter()
    {
        NewChatName = "";
    }
    private void Login_vm_ToReg(object? sender, EventArgs e)
    {
        CurrentPage = reg_p;
    }
    private void Reg_vm_ToLogin(object? sender, EventArgs e)
    {
        CurrentPage = login_p;
    }
}