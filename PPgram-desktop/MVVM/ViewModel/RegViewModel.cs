using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using PPgram_desktop.Core;

namespace PPgram_desktop.MVVM.ViewModel;

class RegViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<RegisterEventArgs> SendUsernameCheck;
    public event EventHandler<RegisterEventArgs> SendRegister;
    public event EventHandler ToLogin;

    #region bindings
    public string Name { get; set; }
    private string _username;
    public string Username
    {
        get { return _username; }
        set { _username = value; ValidateUsername(); }
    }
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

    private bool _usernameInfo;
    public bool UsernameInfo
    { 
        get { return _usernameInfo; }
        set { _usernameInfo = value; OnPropertyChanged(); }
    }
    private bool _usernameOk;
    public bool UsernameOk
    {
        get { return _usernameOk; }
        set { _usernameOk = value; OnPropertyChanged(); }
    }
    private string _usernameStatus;
    public string UsernameStatus
    {
        get { return _usernameStatus; }
        set { _usernameStatus = value; OnPropertyChanged(); }
    }
    #endregion

    #region commands
    public ICommand GoToLoginCommand { get; set; }
    public ICommand RegCommand { get; set; }
    #endregion

    private readonly DispatcherTimer _timer;

    public RegViewModel() 
    {
        UsernameOk = false;
        GoToLoginCommand = new RelayCommand(o => GoToLogin());
        RegCommand = new RelayCommand(o => TryRegister());
        // username check request delay timer
        _timer = new() { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += CheckUsername;
    }
    private void ValidateUsername()
    {
        // stop timer if user is still editing username
        UsernameOk = false;
        _timer.Stop();
        if (!String.IsNullOrEmpty(Username))
        { 
            // check username length
            if (Username.Length < 3)
            {
                ShowUsernameStatus("Username is too short");
                return;
            }
            // check if username is valid characters
            foreach (char c in Username)
            {
                if (!Char.IsAsciiLetterOrDigit(c))
                {
                    ShowUsernameStatus("Invalid username");
                    return;
                }
            }
            // restart delay if username is valid
            _timer.Start();
        }
        ShowUsernameStatus("");
    }
    private bool ValidatePassword()
    {
        // check if password length is valid
        if (!String.IsNullOrEmpty(Password))
        {
            if (Password.Length >= 8 && Password.Length <= 28)
            {
                PassOk = true;
                PassError = false;
                return true;
            }
            PassOk = false;
            PassError = true;
            return false;
        }
        PassOk = false;
        PassError = false;
        return false;
    }
    private bool ValidatePasswordConfirm()
    {
        // check if password confirmation matches password
        if (!String.IsNullOrEmpty(ConfirmPassword))
        {
            if (Password == ConfirmPassword)
            {
                PassConfOk = true;
                PassConfError = false;
                return true;
            }
            PassConfError = true;
            PassConfOk = false;
            return false;
        }
        PassConfError = false;
        PassConfOk = false;
        return false;
    }
    private void CheckUsername(object? sender, EventArgs e)
    {
        // stop timer to prevent request spam
        _timer.Stop();
        SendUsernameCheck?.Invoke(this, new RegisterEventArgs
        {
            username = $"@{Username}"
        });
    }
    private void TryRegister()
    {
        // check if validation passed successfully
        if (String.IsNullOrWhiteSpace(Name) || !ValidatePassword() || !ValidatePasswordConfirm() || !UsernameOk)
            return;
        SendRegister?.Invoke(this, new RegisterEventArgs
        {
            name = Name,
            username = $"@{Username}",
            password = Password,
        });
    }
    private void GoToLogin()
    {
        ToLogin?.Invoke(this, new EventArgs());
    }

    public void ShowUsernameStatus(string status, bool ok = false)
    {
        UsernameStatus = status;
        UsernameInfo = status != "";
        UsernameOk = ok;
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

class RegisterEventArgs : EventArgs
{
    public string name;
    public string username;
    public string password;
}
