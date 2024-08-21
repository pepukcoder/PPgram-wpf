using System.Text;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Buffers.Text;
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
    public event EventHandler<ResponseFetchUserEventArgs> SelfFetched;

    //public event EventHandler<ResponseFetchUserEventArgs> UserFetched;
    //public event EventHandler<IncomeMessageEventArgs> MessageRecieved;
    public event EventHandler Disconnected;

    private readonly TcpClient client;
    private NetworkStream stream;

    public Client()
    {
        client = new();
    }
    public void Connect(string host, int port)
    {
        try
        {
            client.Connect(host, port);
            stream = client.GetStream();
            Thread listenThread = new(new ThreadStart(Listen));
            listenThread.IsBackground = true;
            listenThread.Start();
        }
        catch
        {
            Disconnected?.Invoke(this, new());
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

                // handle response
                HandleResponse(response);
            }
            catch
            {
                Disconnected?.Invoke(this, new());
            }
        }
    }
    private void Send(object data)
    {
        try
        {
            string request = JsonSerializer.Serialize(data);
            stream.Write(RequestBuilder.GetBytes(request));
        }
        catch
        {
            Disconnected?.Invoke(this, new());
        }
        
    }
    private void Stop()
    {
        Disconnected?.Invoke(this, new EventArgs());
        stream?.Dispose();
        client.Close();
    }
    private void HandleResponse(string response)
    {
        JsonNode? rootNode = JsonNode.Parse(response);

        string? method = rootNode?["method"]?.GetValue<string>();
        bool? ok = rootNode?["ok"]?.GetValue<bool>();
        string? r_error = rootNode?["error"]?.GetValue<string>();
        switch (method)
        {
            case "login":
                if (ok == true)
                {
                    LoggedIn?.Invoke(this, new ResponseLoginEventArgs
                    {
                        ok = true,
                        sessionId = rootNode?["session_id"]?.GetValue<string>(),
                        userId = rootNode?["user_id"]?.GetValue<int>()
                    });
                }
                else if (ok == false && r_error != null)
                {
                    LoggedIn?.Invoke(this, new ResponseLoginEventArgs
                    {
                        ok = false,
                        error = r_error
                    });
                }
                break;
            case "register":
                MessageBox.Show(response);
                if (ok == true)
                {
                    Registered?.Invoke(this, new ResponseRegisterEventArgs
                    {
                        ok = true,
                        sessionId = rootNode?["session_id"]?.GetValue<string>(),
                        userId = rootNode?["user_id"]?.GetValue<int>()
                    });
                }
                else if (ok == false && r_error != null)
                {
                    Registered?.Invoke(this, new ResponseRegisterEventArgs
                    {
                        ok = false,
                        error = r_error
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
                else if(ok == false && r_error != null)
                {
                    Authorized?.Invoke(this, new ResponseAuthEventArgs
                    {
                        ok = false,
                        error = r_error
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
            case "fetch_self":
                MessageBox.Show(response);
                if (true) // Pavlo forgot to add { "ok": true } in json so this is for DEBUG
                {
                    JsonNode? userNode = rootNode?["response"];
                    SelfFetched?.Invoke(this, new ResponseFetchUserEventArgs
                    {
                        ok = true,
                        name = userNode?["name"]?.GetValue<string>(),
                        username = userNode?["username"]?.GetValue<string>(),
                        userId = userNode?["user_id"]?.GetValue<int>(),
                        avatarFormat = userNode?["avatar_format"]?.GetValue<string>(),
                        avatarData = userNode?["avatar_data"]?.GetValue<string>()
                    });
                }
                if (ok == false && r_error != null)
                {
                    SelfFetched?.Invoke(this, new ResponseFetchUserEventArgs
                    {
                        ok = false,
                        error = r_error
                    });
                }
                break;
        }
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
        Send(data);
    }
    public void AuthorizeWithLogin(string login, string password)
    {
        var data = new
        {
            method = "login",
            username = login,
            password_hash = password
        };
        Send(data);
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
        Send(data);
    }
    public void ChekUsernameAvailable(string username)
    {
        var data = new
        {
            method = "check",
            what = "username",
            data = username
        };
        Send(data);
    }
    public void FetchSelf()
    {
        var data = new
        {
            method = "fetch",
            what = "self"
        };
        Send(data);
    }
    public void FetchUser(string id)
    {

    }

    public void FetchChats()
    {

    }    
    public void FetchMessages(string id)
    {

    }
    public void SendMessage(MessageModel message)
    {
        var data = new
        {
            method = "send_message",

        };
        Send(data);
    }
    #endregion
}

class ResponseLoginEventArgs : EventArgs
{
    public int? userId;
    public string? sessionId;

    public bool ok;
    public string error;
}
class ResponseRegisterEventArgs : EventArgs
{
    public int? userId;
    public string? sessionId;

    public bool ok;
    public string error;
}
class ResponseAuthEventArgs : EventArgs
{
    public bool ok;
    public string error;
}
class ResponseUsernameCheckEventArgs : EventArgs
{
    public bool available;
}
class ResponseFetchUserEventArgs : EventArgs
{
    public string? name;
    public string? username;
    public int? userId;
    public string? avatarFormat;
    public string? avatarData;

    public bool ok;
    public string error;
}

class IncomeMessageEventArgs : EventArgs
{
    public int? sender_id;
    public int? receiver_id;
    public MessageModel? message;
}