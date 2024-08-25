using System.Text;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Buffers.Text;
using System.Windows;

using PPgram_desktop.MVVM.Model;
using PPgram_desktop.Net.IO;
using System.Collections.ObjectModel;
using PPgram_desktop.MVVM.Model.DTO;
using System.Xml.Linq;
using System.Diagnostics.Eventing.Reader;

namespace PPgram_desktop.Net;

class Client
{
    public event EventHandler<ResponseSessionEventArgs> Authorized;
    public event EventHandler<ResponseSessionEventArgs> LoggedIn;
    public event EventHandler<ResponseSessionEventArgs> Registered;
    public event EventHandler<ResponseUsernameCheckEventArgs> UsernameChecked;
    public event EventHandler<ResponseFetchUserEventArgs> SelfFetched;
    public event EventHandler<ResponseFetchChatsEventArgs> ChatsFetched;
    public event EventHandler<ResponseFetchMessagesEventArgs> MessagesFetched;
    public event EventHandler<NewMessageEventArgs> NewMessage;
    public event EventHandler<GotChatEventArgs> GotNewChat;

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
            Thread listenThread = new(new ThreadStart(Listen))
            { IsBackground = true };
            listenThread.Start();
        }
        catch
        {
            Disconnected?.Invoke(this, new());
        }
    }
    private void Listen()
    {
        List<byte> response_chunks = [];
        int expected_size = 0;
        bool isFirst = true;
        while (true)
        {
            try
            {
                int read_count;

                // get server response length if chunk is first
                if (isFirst)
                {
                    byte[] length_bytes = new byte[4];
                    read_count = stream.Read(length_bytes, 0, 4);
                    if (read_count == 0) break;
                    if (BitConverter.IsLittleEndian) Array.Reverse(length_bytes);
                    int length = BitConverter.ToInt32(length_bytes);

                    expected_size = length;
                    isFirst = false;
                }
                
                // get server response chunk
                byte[] responseBytes = new byte[expected_size];
                read_count = stream.Read(responseBytes, 0, expected_size);

                // cut chunk by actual read count and to list 
                ArraySegment<byte> segment = new(responseBytes, 0, read_count);
                responseBytes = [.. segment];
                response_chunks.AddRange(responseBytes);

                if (response_chunks.Count >= expected_size)
                {
                    // get whole response if size equals expected
                    string response = Encoding.UTF8.GetString(response_chunks.ToArray());

                    // reset chunks
                    response_chunks.Clear();
                    expected_size = 0;
                    isFirst = true;

                    // handle response
                    HandleResponse(response);
                } 
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
            stream?.Write(RequestBuilder.GetBytes(request));
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
        // parse common fields
        string? r_method = rootNode?["method"]?.GetValue<string>();
        string? r_event = rootNode?["event"]?.GetValue<string>();
        string? r_error = rootNode?["error"]?.GetValue<string>();
        bool? ok = rootNode?["ok"]?.GetValue<bool>();
        // parse specific fields
        switch (r_method)
        {
            case "login":
                if (ok == true)
                {
                    LoggedIn?.Invoke(this, new ResponseSessionEventArgs
                    {
                        ok = true,
                        sessionId = rootNode?["session_id"]?.GetValue<string>(),
                        userId = rootNode?["user_id"]?.GetValue<int>()
                    });
                }
                else if (ok == false && r_error != null)
                {
                    LoggedIn?.Invoke(this, new ResponseSessionEventArgs
                    {
                        ok = false,
                        error = r_error
                    });
                }
                break;
            case "register":
                if (ok == true)
                {
                    Registered?.Invoke(this, new ResponseSessionEventArgs
                    {
                        ok = true,
                        sessionId = rootNode?["session_id"]?.GetValue<string>(),
                        userId = rootNode?["user_id"]?.GetValue<int>()
                    });
                }
                else if (ok == false && r_error != null)
                {
                    Registered?.Invoke(this, new ResponseSessionEventArgs
                    {
                        ok = false,
                        error = r_error
                    });
                }
                break;
            case "auth":
                if (ok == true)
                {
                    Authorized?.Invoke(this, new ResponseSessionEventArgs
                    {
                        ok = true
                    });
                }
                else if(ok == false && r_error != null)
                {
                    Authorized?.Invoke(this, new ResponseSessionEventArgs
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
                if (ok == true)
                {
                    JsonNode? userNode = rootNode?["data"];
                    SelfFetched?.Invoke(this, new ResponseFetchUserEventArgs
                    {
                        ok = true,
                        user = new ProfileDTO
                        {
                            Name = userNode?["name"]?.GetValue<string>(),
                            Username = userNode?["username"]?.GetValue<string>(),
                            Id = userNode?["user_id"]?.GetValue<int>(),
                            Photo = userNode?["photo"]?.GetValue<string>()
                        }
                    });
                }
                else if (ok == false && r_error != null)
                {
                    SelfFetched?.Invoke(this, new ResponseFetchUserEventArgs
                    {
                        ok = false,
                        error = r_error
                    });
                }
                break;
            case "fetch_chats":
                if (ok == true)
                {
                    // SHITCODE CLEANING NEEDED
                    JsonArray? chatsJson = rootNode?["data"]?.AsArray();
                    ObservableCollection<ChatDTO> chatlist = [];
                    if (chatsJson != null)
                    {
                        foreach (JsonNode? chatNode in chatsJson)
                        {
                            ChatDTO? chat = chatNode?.Deserialize<ChatDTO>();
                            if (chat != null)
                            {
                                chatlist.Add(chat);
                            } 
                        }
                        ChatsFetched?.Invoke(this, new ResponseFetchChatsEventArgs
                        {
                            ok = true,
                            chats = chatlist
                        });
                    }
                }
                else if (ok == false && r_error != null)
                {
                    ChatsFetched?.Invoke(this, new ResponseFetchChatsEventArgs
                    {
                        ok = true,
                        error = r_error
                    });
                }
                break;
            case "fetch_messages":
                if (ok == true)
                {        
                    JsonArray? messagesJson = rootNode?["data"]?.AsArray();
                    ObservableCollection<MessageDTO> messagelist = [];
                    if (messagesJson != null)
                    {
                        foreach (JsonNode? chatNode in messagesJson)
                        {
                            MessageDTO? message = chatNode?.Deserialize<MessageDTO>();
                            if (message != null)
                                messagelist.Add(message);
                        }
                    }
                    MessagesFetched?.Invoke(this, new ResponseFetchMessagesEventArgs
                    {
                        ok = true,
                        messages = messagelist
                    });
                }
                else if (ok == false && r_error != null)
                {
                    MessagesFetched?.Invoke(this, new ResponseFetchMessagesEventArgs
                    {
                        ok = false,
                        error = r_error
                    });
                }
                break;
            case "fetch_user":
                if (ok == true)
                {
                    JsonNode? userNode = rootNode?["data"];
                    GotNewChat?.Invoke(this, new GotChatEventArgs
                    {
                        ok = true,
                        chat = new ChatDTO
                        {
                            Name = userNode?["name"]?.GetValue<string>(),
                            Username = userNode?["username"]?.GetValue<string>(),
                            Id = userNode?["user_id"]?.GetValue<int>(),
                            Photo = userNode?["photo"]?.GetValue<string>()
                        }
                    });
                }
                else if (ok == false && r_error != null)
                {
                    GotNewChat?.Invoke(this, new GotChatEventArgs
                    {
                        ok = false,
                        error = r_error
                    });
                }
                break;
        }
        switch (r_event)
        {
            case "new_message":
                if (ok == true)
                {
                    JsonNode? messageJson = rootNode?["data"];
                    if (messageJson != null)
                    {
                        MessageDTO? message_dto = messageJson.Deserialize<MessageDTO>();
                        NewMessage?.Invoke(this, new NewMessageEventArgs
                        {
                            ok = true,
                            message = message_dto
                        });
                    }
                }
                else if (ok == false && r_error != null)
                {
                    NewMessage?.Invoke(this, new NewMessageEventArgs
                    {
                        ok = false,
                        error = r_error
                    });
                }
                break;
            case "new_chat":
                if (ok == true)
                {
                    JsonNode? chatNode = rootNode?["data"];
                    GotNewChat?.Invoke(this, new GotChatEventArgs
                    {
                        ok = true,
                        chat = chatNode?.Deserialize<ChatDTO>()
                    });
                }
                else if (ok == false && r_error != null)
                {
                    GotNewChat?.Invoke(this, new GotChatEventArgs
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
    public void FetchChats()
    {
        var data = new
        {
            method = "fetch",
            what = "chats"
        };
        Send(data);
    }  
    public void FetchUser(string username)
    {
        var data = new
        {
            method = "fetch",
            what = "user",
            username = username
        };
        Send(data);
    }    
    public void FetchMessages(long id)
    {
        var data = new
        {
            method = "fetch",
            what = "messages",
            chat_id = id,
            range = new[]{ -1, -19999}
        };
        Send(data);
    }

    public void SendMessage(MessageDTO message)
    {
        var data = new
        {
            method = "send_message",
            to = message.ChatId,
            has_reply = message.ReplyTo != null,
            reply_to = message.ReplyTo,
            content = new
            {
                text = message.Text
            }
        };
        Send(data);
    }
    #endregion
}

class ResponseSessionEventArgs : EventArgs
{
    public int? userId;
    public string? sessionId;

    public bool ok;
    public string error;
}
class ResponseUsernameCheckEventArgs : EventArgs
{
    public bool available;
}
class ResponseFetchUserEventArgs : EventArgs
{
    public ProfileDTO? user;

    public bool ok;
    public string error;
}
class ResponseFetchChatsEventArgs : EventArgs
{
    public ObservableCollection<ChatDTO> chats;

    public bool ok;
    public string error;
}
class ResponseFetchMessagesEventArgs : EventArgs
{
    public ObservableCollection<MessageDTO> messages;

    public bool ok;
    public string error;
}
class NewMessageEventArgs : EventArgs
{
    public MessageDTO? message;

    public bool ok;
    public string error;
}
class GotChatEventArgs : EventArgs
{
    public ChatDTO? chat;

    public bool ok;
    public string error;
}