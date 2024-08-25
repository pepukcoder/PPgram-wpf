using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PPgram_desktop.Core;

namespace PPgram_desktop.MVVM.ViewModel;

class LoginViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<LoginEventArgs> SendLogin;
    public event EventHandler ToReg;

    #region bindings
    public string Login { get; set; }
    public string Password { private get; set; }
    #endregion

    #region commands
    public ICommand ToRegCommand { get; set; }
    public ICommand LoginCommand { get; set; }
    #endregion

    public LoginViewModel() 
    {
        ToRegCommand = new RelayCommand(o => GoToReg());
        LoginCommand = new RelayCommand(o => TryLogin());
    }
    private void TryLogin()
    {
        // check if fields not empty
        if (String.IsNullOrWhiteSpace(Login) || String.IsNullOrEmpty(Password))
            return;
        SendLogin?.Invoke(this, new LoginEventArgs
        {
            login = $"@{Login}",
            password = Password
        });
    }
    private void GoToReg()
    {
        ToReg?.Invoke(this, new EventArgs());
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

class LoginEventArgs : EventArgs
{
    public string login;
    public string password;
}