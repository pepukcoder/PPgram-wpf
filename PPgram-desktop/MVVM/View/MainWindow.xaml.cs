using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Input;
using System.Windows.Threading;

namespace PPgram_desktop;

public partial class MainWindow : Window
{
    private void MainFrame_Navigated(object sender, NavigationEventArgs e)
    {
        Frame? fr = sender as Frame;
        fr?.NavigationService.RemoveBackEntry();
    }
    private void Window_Closed(object sender, EventArgs e)
    {
        Environment.Exit(Environment.ExitCode);
    }
}