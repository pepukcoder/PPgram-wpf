using PPgram_desktop.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PPgram_desktop.MVVM.ViewModel;

class LoginViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler ToReg;
    public event EventHandler<LoginEventArgs> SendLogin;

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
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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
}
class LoginEventArgs : EventArgs
{
    public string login = "";
    public string password = "";
}

