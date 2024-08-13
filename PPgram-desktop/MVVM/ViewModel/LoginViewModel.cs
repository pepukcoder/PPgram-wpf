using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using PPgram_desktop.Core;

namespace PPgram_desktop.MVVM.ViewModel;

class LoginViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler ToReg;
    public event EventHandler<LoginEventArgs> SendLogin;

    #region bindings
    public string Login { get; set; }
    public string Password { private get; set; }
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
    public ICommand ToRegCommand { get; set; }
    public ICommand LoginCommand { get; set; }
    #endregion

    public LoginViewModel() 
    {
        ToRegCommand = new RelayCommand(o => GoToReg());
        LoginCommand = new RelayCommand(o => TryLogin());
    }
    
    private void GoToReg()
    {
        ToReg?.Invoke(this, new EventArgs());
    }
    private void TryLogin()
    {
        SendLogin?.Invoke(this, new LoginEventArgs
        {
            login = Login,
            password = Password
        });
    }
    public void ShowError(string error)
    {
        Error = error;
        IsError = true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
class LoginEventArgs : EventArgs
{
    public string login = "";
    public string password = "";
}

