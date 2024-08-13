using System.Text;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Nodes;

using PPgram_desktop.MVVM.Model;
using PPgram_desktop.Net.IO;
using System.Windows;

namespace PPgram_desktop.Net;

class Client
{
    public event EventHandler<ResponseAuthEventArgs> Authorized;
    public event EventHandler<ResponseLoginEventArgs> LoggedIn;
    public event EventHandler<ResponseRegisterEventArgs> Registered;
    public event EventHandler<ResponseMessageEventArgs> MessageRecieved;
    public event EventHandler Disconnected;

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
                listenThread.IsBackground = true;
                listenThread.Start();
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, "Error",MessageBoxButton.OK,MessageBoxImage.Error);
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
                // handle response
                HandleResponse(response);
            }
            catch (Exception e)
            {
                // MessageBox.Show(e.Message, "Error",MessageBoxButton.OK,MessageBoxImage.Error);
                Stop();
            }
        }
    }
    private void HandleResponse(string response)
    {
        string? method = JsonNode.Parse(response)?["method"]?.GetValue<string>();
        bool? ok = JsonNode.Parse(response)?["ok"]?.GetValue<bool>();
        string? error = JsonNode.Parse(response)?["error"]?.GetValue<string>();
        switch (method)
        {
            case "login":
                
                if (ok == true)
                {
                    MessageBox.Show(response);
                }
                MessageBox.Show(error);
                break;
            case "register":
                if (ok == true)
                {
                    MessageBox.Show(response);
                }
                MessageBox.Show(error);
                break;
            case "auth":
                
                break;
        }
    }
    private void Stop()
    {
        Disconnected?.Invoke(this, new EventArgs());
        stream.Close();
        client.Close();
    }
    #region request methods
    public void AuthorizeWithSessionId(string sessionId, int userId)
    {
        var data = new
        {
            method = "auth",
            user_id = userId,
            session_id = sessionId,
        };
        string request = JsonSerializer.Serialize(data);
        stream.Write(RequestBuilder.GetBytes(request));
    }
    public void AuthorizeWithLogin(string login, string password)
    {
        var data = new
        {
            method = "login",
            username = login,
            password_hash = password
        };
        string request = JsonSerializer.Serialize(data);
        stream.Write(RequestBuilder.GetBytes(request));
    }
    public void RegisterNewUser(string newUsername, string newName, string newPassword)
    {
        var data = new
        {
            method = "register",
            username = newUsername,
            name = newName,
            password_hash = newPassword
        };
        string request = JsonSerializer.Serialize(data);
        stream.Write(RequestBuilder.GetBytes(request));
    }
    public void SendMessage(MessageModel message)
    {
        /*
        var data = new
        {
            method = "",
        };
        string request = JsonSerializer.Serialize(data);
        stream.Write(RequestBuilder.GetBytes(request));
        */
    }
    public void FetchChats()
    {

    }
    public void FetchUser(string id)
    {

    }
    public void FetchMessages(string id)
    {

    }
    #endregion
}

class ResponseMessageEventArgs : EventArgs
{
    public int? sender_id;
    public int? receiver_id;
    public MessageModel? message;
}
class ResponseLoginEventArgs : EventArgs
{

}
class ResponseRegisterEventArgs : EventArgs
{
    public bool? ok;
    public string? error;
}
class ResponseAuthEventArgs : EventArgs
{
    public int? userId;
    public int? sessionId;
}