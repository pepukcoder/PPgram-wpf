using PPgram_desktop.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PPgram_desktop.MVVM.ViewModel;

class RegViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler ToLogin;



    #region bindings
    public string Name { get; set; }
    public string Username { get; set; }
    private string _password = "";
    public string Password
    {
        get { return _password; }
        set { _password = value; ValidatePassword(); ValidatePasswordConfirm(); }
    }
    private string _confirmPassword = "";
    public string ConfirmPassword
    {
        get { return _confirmPassword; }
        set { _confirmPassword = value; ValidatePasswordConfirm(); }
    }

    private bool _passOk;
    public bool PassOk
    {
        get { return _passOk; }
        set { _passOk = value; OnPropertyChanged(); }
    }
    private bool _passError;
    public bool PassError
    {
        get { return _passError; }
        set { _passError = value; OnPropertyChanged(); }
    }
    private bool _passConfOk;
    public bool PassConfOk
    {
        get { return _passConfOk; }
        set { _passConfOk = value; OnPropertyChanged(); }
    }
    private bool _passConfError;
    public bool PassConfError
    {
        get { return _passConfError; }
        set { _passConfError = value; OnPropertyChanged(); }
    }
    #endregion

    #region commands
    public ICommand GoToLoginCommand { get; set; }
    public ICommand RegCommand { get; set; }
    #endregion
    public RegViewModel() 
    {
        GoToLoginCommand = new RelayCommand(o => GoToLogin());
    }
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    private void ValidatePassword()
    {
        if (Password != null)
        {
            if (Password.Length >= 8 && Password.Length <= 25)
            {
                PassOk = true;
                PassError = false;
            }
            else
            {
                PassOk = false;
                PassError = true;
            }
        }
    }
    private void ValidatePasswordConfirm()
    {
        if (Password.Length >= 8 && Password == ConfirmPassword)
        {
            PassConfOk = true;
            PassConfError = false;
        }
        else
        {
            PassConfError = true;
            PassConfOk = false;
        }
    }
    private void GoToLogin()
    {
        ToLogin?.Invoke(this, new EventArgs());
    }
}
