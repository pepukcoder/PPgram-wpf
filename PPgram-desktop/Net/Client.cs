using System.Text;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;

using PPgram_desktop.MVVM.Model;
using PPgram_desktop.Net.IO;

namespace PPgram_desktop.Net;

class Client
{
    public event EventHandler<ResponseAuthEventArgs> Authorized;
    public event EventHandler<ResponseLoginEventArgs> LoggedIn;
    public event EventHandler<ResponseRegisterEventArgs> Registered;
    public event EventHandler<ResponseUsernameCheckEventArgs> UsernameChecked;

    public event EventHandler<IncomeMessageEventArgs> MessageRecieved;
    public event EventHandler Disconnected;

    private readonly TcpClient client;
    private NetworkStream stream;

    public Client()
    {
        client = new();
    }
    public bool Connect(string host, int port)
    {
        try
        {
            client.Connect(host, port);
            stream = client.GetStream();
            Thread listenThread = new(new ThreadStart(Listen));
            listenThread.IsBackground = true;
            listenThread.Start();
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
    private void Listen()
    {
        while (client.Connected)
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

                // handle response
                HandleResponse(response);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error",MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }
    }
    private void HandleResponse(string response)
    {

        // REFACTOR & CLEANING NEEDED

        string? method = JsonNode.Parse(response)?["method"]?.GetValue<string>();
        bool? ok = JsonNode.Parse(response)?["ok"]?.GetValue<bool>();
        string? error = JsonNode.Parse(response)?["error"]?.GetValue<string>();
        switch (method)
        {
            case "login":
                MessageBox.Show(response);
                if (ok == true)
                {
                    LoggedIn?.Invoke(this, new ResponseLoginEventArgs
                    {
                        ok = true,
                        sessionId = JsonNode.Parse(response)?["session_id"]?.GetValue<string>(),
                        userId = JsonNode.Parse(response)?["user_id"]?.GetValue<int>()
                    });
                }
                else if (ok == false && error != null)
                {
                    LoggedIn?.Invoke(this, new ResponseLoginEventArgs
                    {
                        ok = false,
                        error = error
                    });
                }
                break;
            case "register":
                if (ok == true)
                {
                    Registered?.Invoke(this, new ResponseRegisterEventArgs
                    {
                        ok = true,
                        sessionId = JsonNode.Parse(response)?["session_id"]?.GetValue<string>(),
                        userId = JsonNode.Parse(response)?["user_id"]?.GetValue<int>()
                    });
                }
                else if (ok == false && error != null)
                {
                    Registered?.Invoke(this, new ResponseRegisterEventArgs
                    {
                        ok = false,
                        error = error
                    });
                }
                break;
            case "auth":
                if (ok == true)
                {
                    Authorized?.Invoke(this, new ResponseAuthEventArgs
                    {
                        ok = true
                    });
                }
                else if(ok == false && error != null)
                {
                    Authorized?.Invoke(this, new ResponseAuthEventArgs
                    {
                        ok = false,
                        error = error
                    });
                }
                break;
            case "check_username":
                if (ok == true)
                {
                    UsernameChecked?.Invoke(this, new ResponseUsernameCheckEventArgs
                    {
                        available = false
                    });
                }
                else if (ok == false)
                {
                    UsernameChecked?.Invoke(this, new ResponseUsernameCheckEventArgs
                    {
                        available = true
                    });
                }
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
        if (!client.Connected)
            return;
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
        if (!client.Connected)
            return;
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
        if (!client.Connected)
            return;
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
        if (!client.Connected)
            return;
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
        if (!client.Connected)
            return;
    }
    public void FetchUser(string id)
    {
        if (!client.Connected)
            return;
    }
    public void FetchMessages(string id)
    {
        if (!client.Connected)
            return;
    }
    public void ChekUsernameAvailable(string username)
    {
        if (!client.Connected)
            return;
        var data = new
        {
            method = "check",
            what = "username",
            data = username
        };
        string request = JsonSerializer.Serialize(data);
        stream.Write(RequestBuilder.GetBytes(request));
    }
    #endregion
}

class ResponseLoginEventArgs : EventArgs
{
    public int? userId;
    public string? sessionId = "";

    public bool ok = false;
    public string error = "";
}
class ResponseRegisterEventArgs : EventArgs
{
    public int? userId;
    public string? sessionId = "";

    public bool ok = false;
    public string error = "";
}
class ResponseAuthEventArgs : EventArgs
{
    public bool ok = false;
    public string error = "";
}

class ResponseUsernameCheckEventArgs : EventArgs
{
    public bool available;
}
class IncomeMessageEventArgs : EventArgs
{
    public int? sender_id;
    public int? receiver_id;
    public MessageModel? message;
}