using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using PPgram_desktop.Core;

namespace PPgram_desktop.MVVM.ViewModel;

class RegViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler ToLogin;
    public EventHandler<RegisterEventArgs> SendRegister;

    #region bindings
    public string Name { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }

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
    #endregion

    #region commands
    public ICommand GoToLoginCommand { get; set; }
    public ICommand RegCommand { get; set; }
    #endregion
    public RegViewModel() 
    {
        GoToLoginCommand = new RelayCommand(o => GoToLogin());
        RegCommand = new RelayCommand(o => TryRegister());
    }
    private bool ValidateReg()
    {
        // check if all fields filled
        if (String.IsNullOrWhiteSpace(Password)
            || String.IsNullOrWhiteSpace(ConfirmPassword)
            || String.IsNullOrWhiteSpace(Username)
            || String.IsNullOrWhiteSpace(Name))
        {
            ShowError("Missing data");
            return false;
        }
        // check password length
        if (Password.Length < 8 || Password.Length > 32)
        {
            ShowError("Invalid password");
            return false;
        }
        // check password confirmation
        if (Password != ConfirmPassword)
        {
            ShowError("Passwords do not match");
            return false;
        }
        // check username length
        if (Username.Length <= 3)
        {
            ShowError("Invalid username");
            return false;
        }
        // check if username is valid characters
        foreach (char c in Username)
        {
            if (!Char.IsAsciiLetterOrDigit(c))
            {
                ShowError("Invalid username");
                return false;
            }
        }
        Error = "";
        IsError = false;
        return true;
    }
    private void TryRegister()
    {
        if (!ValidateReg())
        {
            return;
        }
        SendRegister?.Invoke(this, new RegisterEventArgs
        {
            name = Name,
            username = Username,
            password = Password,
        });
    }
    private void GoToLogin()
    {
        ToLogin?.Invoke(this, new EventArgs());
    }
    private void ShowError(string error)
    {
        Error = error;
        IsError = true;
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