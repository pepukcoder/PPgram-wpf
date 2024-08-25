using PPgram_desktop.MVVM.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PPgram_desktop.MVVM.View;

public partial class ChatPage : Page
{
    public ChatPage()
    {
        InitializeComponent();
        this.DataContextChanged += (s, e) =>
        {
            ((ChatViewModel)this.DataContext).ScrollToLast += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (Listview_Messages.Items.Count > 0)
                        Listview_Messages.ScrollIntoView(Listview_Messages.Items[^1]);
                }, System.Windows.Threading.DispatcherPriority.Background);
            };
        };
    }

    private void MessageInputBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                if (sender is TextBox textBox)
                {
                    textBox.Text += "\r\n";
                    textBox.CaretIndex = textBox.Text.Length; // Move caret to end of text
                    e.Handled = true;
                }
            }
            else
            {
                if (this.DataContext != null)
                {
                    ((ChatViewModel)this.DataContext).SendMessageCommand.Execute(null);
                }
                e.Handled = true;
            }
        }
    }
}
