using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

class JoinUser
{
    private string userID;
    private TcpClient client;

    public JoinUser(TcpClient client)
    {
        userID = "";
        this.client = client;
    }

    public void SetUserID(string userID)
    {
        this.userID = userID;
    }

    public TcpClient GetClient()
    {
        return client;
    }

    public string GetUserID()
    {
        return userID;
    }
}

class Server
    {
        private static TcpListener server;
        private static List<JoinUser> clientList = new List<JoinUser>();

        static void Main(string[] args)
        {
            server = new TcpListener(IPAddress.Any, 12340);
            server.Start();

            Console.WriteLine("server start\n");
            Console.WriteLine("waiting for a client\n");

            Thread serverMessage = new Thread(new ParameterizedThreadStart(ServerMessage));
            serverMessage.Start();

            while(true)
            {
                TcpClient client = server.AcceptTcpClient();
                JoinUser joinUser = new JoinUser(client);
                clientList.Add(joinUser);
                Console.WriteLine("handler\n");
                Thread t_handler = new Thread(new ParameterizedThreadStart(ClientListener));
                t_handler.Start(joinUser);
                Console.WriteLine(clientList.Count + "명");
            }
        }

    static void ServerMessage(object sender)
    {
        while(true)
        {
            string serverMsg = "|서버" + "|" + Console.ReadLine();
            BroadCast(serverMsg);
        }
    }

    static void ClientListener(object sender)
    {
        JoinUser client = null;
        StreamReader sr = null;
        Console.WriteLine("Listener\n");
        try
        {
            client = sender as JoinUser;
            sr = new StreamReader(client.GetClient().GetStream());

            Console.WriteLine("New Client connected\n");

            while (true)
            {
                string message = sr.ReadLine();
                if (!string.IsNullOrEmpty(message))
                {
                    if(client.GetUserID() == "")
                    {
                        client.SetUserID(message.Split('/')[1].ToString());
                        Console.WriteLine(client.GetUserID());
                    }
                    BroadCast(message);
                    Console.WriteLine("received data : {0}\n", message);
                }
            }
        }
        catch (SocketException se)
        {
            Console.WriteLine("SocketException : {0}\n", se.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception : {0}\n", ex.Message);
        }
        finally
        {
            Console.WriteLine("client disconnected\n");
            Console.WriteLine("{0} 님이 퇴장하셨습니다.", client.GetUserID());
            clientList.Remove(client);

            sr.Close();
            client.GetClient().Close();

            Thread.CurrentThread.Abort();
        }
    }

    static void BroadCast(string message)
    {
        foreach (JoinUser client in clientList)
        {
              StreamWriter sw = new StreamWriter(client.GetClient().GetStream());
              sw.WriteLine(message);
              sw.Flush();
        }
    }

}
