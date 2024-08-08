using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using PPgram_desktop.Net.IO;
using System.Windows;
using System.Text.Json.Serialization;
using PPgram_desktop.MVVM.Model;
using System.Text.Json;

namespace PPgram_desktop.Net;

class Client
{
    TcpClient client;
    NetworkStream stream;

    public event EventHandler<AuthEventArgs> AuthorizedEvent;
    public event EventHandler<MessageEventArgs> MessageRecievedEvent;
    public event EventHandler<MessageEventArgs> DisconnectedEvent;

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
            catch (Exception ex)
            {

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

                //Get server response length
                byte[] length_bytes = new byte[4];
                read_count = stream.Read(length_bytes, 0, 4);
                if (read_count == 0) break;
                if (BitConverter.IsLittleEndian) Array.Reverse(length_bytes);

                int length = BitConverter.ToInt32(length_bytes);

                //Get server response itself
                byte[] responseBytes = new byte[length];
                read_count = stream.Read(responseBytes, 0, length);
                if (read_count == 0) break;

                string response = Encoding.UTF8.GetString(responseBytes);
                MessageBox.Show(response);
                //TODO
                //Parse response id and type 
                //Invoke corresponding events in ViewModels

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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