using System.Text;
using System.Net.Sockets;
using System.Windows;
using System.Text.Json.Serialization;

using PPgram_desktop.MVVM.Model;
using PPgram_desktop.Net.IO;

namespace PPgram_desktop.Net;

class Client
{
    public event EventHandler<AuthEventArgs> AuthorizedEvent;
    public event EventHandler<MessageEventArgs> MessageRecievedEvent;
    public event EventHandler<MessageEventArgs> DisconnectedEvent;

    private readonly TcpClient client;
    private NetworkStream stream;

    public Client()
    {
        client = new();
    }
    public void Connect(string host, int port)
    {

        if (!client.Connected)
        {
            try
            {
                client.Connect(host, port);
                stream = client.GetStream();

                Thread listenThread = new(new ThreadStart(Listen));
                listenThread.Start();
            }
            catch (Exception e)
            {
                // MessageBox.Show(e.Message, "Error",MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }
    }
    private void Listen()
    {
        while (true)
        {
            try
            {
                int read_count;

                // get server response length
                byte[] length_bytes = new byte[4];
                read_count = stream.Read(length_bytes, 0, 4);
                if (read_count == 0) break;
                if (BitConverter.IsLittleEndian) Array.Reverse(length_bytes);

                int length = BitConverter.ToInt32(length_bytes);

                // get server response itself
                byte[] responseBytes = new byte[length];
                read_count = stream.Read(responseBytes, 0, length);
                if (read_count == 0) break;

                string response = Encoding.UTF8.GetString(responseBytes);
                MessageBox.Show(response);

                // TODO
                // parse response method 
                // invoke events in ViewModels
            }
            catch (Exception e)
            {
                // MessageBox.Show(e.Message, "Error",MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }
    }
    public void AuthorizeWithSessionId(string session_id)
    {

    }
    public void AuthorizeWithLogin(string login, string password)
    {
        
    }
    public void RegisterNewUser(string username, string name, string password_hash)
    {

    }
    public void FetchUsers()
    {

    }
    public void FetchUser(string Id)
    {

    }
    public void FetchUserMessages(string Id)
    {

    }
}

class MessageEventArgs : EventArgs
{
    public int? sender_id;
    public int? receiver_id;
    public ChatMessageModel? message;
}
class AuthEventArgs : EventArgs
{
    public int? userId;
    public int? sessionId;
}