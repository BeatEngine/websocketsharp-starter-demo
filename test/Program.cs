using MQTTnet.Server;
using MQTTnet;
using System.Text;
using static System.Console;
using System.Net.Sockets;
using System.Net.WebSockets;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Timers;






public class TestService : WebSocketBehavior
{
    static bool connected = false;
    static List<TestService> refs = new List<TestService>();
    protected override void OnMessage(MessageEventArgs e)
    {
        Console.WriteLine("Received from client: " + e.Data);
        if(!refs.Contains(this))
        {
            refs.Add(this);
        }
    }

    protected override void OnClose(CloseEventArgs e)
    {
        refs.Remove(this);
        base.OnClose(e);
    }

    public static void SendMsg(String msg)
    {
        for (int i = 0; i < refs.Count(); i++)
        {
            refs[i].Send(msg);
        }
    }

    public static bool Active()
    {
        return refs.Count() > 0;
    }

    protected override void OnError(WebSocketSharp.ErrorEventArgs e)
    {
        refs.Remove(this);
        // do nothing
    }
}


public class Start
{

    static async Task httpServer()
{
    Console.WriteLine("Webinterface under localhost:8080");
    string htmlHead = File.ReadAllText("app.html");
    string httpHeader = "HTTP/1.1 200 OK\nContent-Length: "+htmlHead.Length+"\nContent-Type: text/html; charset=utf-8\nServer: Apache\n\n";
    string smsg = httpHeader + htmlHead;
    TcpListener server = new TcpListener(System.Net.IPAddress.Any, 8080);  
        // we set our IP address as server's address, and we also set the port: 9999

        server.Start();  // this will start the server

        while (true)   //we wait for a connection
        {
            TcpClient client = server.AcceptTcpClient();  //if a connection exists, the server will accept it

            NetworkStream ns = client.GetStream(); //networkstream is used to send/receive messages

            byte[] hello = new byte[smsg.Length];   //any message must be serialized (converted to byte array)
            hello = Encoding.Default.GetBytes(smsg);  //conversion string => byte array

            ns.Write(hello, 0, hello.Length);     //sending the message

            if (client.Connected)  //while the client is connected, we look for incoming messages
            {
                StreamReader r = new StreamReader(ns);
                String ?ln = r.ReadLine();
                while(ln != null && ln != "")
                {
                    ln = r.ReadLine();
                }
                //Console.WriteLine(encoder.GetString(msg).Trim('')); //now , we write the message as string
            }
        }
}

    static async Task webSocketServer()
    {
        // Initialize WebSocket server
        WebSocketServer ws = new WebSocketServer("ws://localhost:8083");
        ws.AddWebSocketService<TestService>("/test");
        ws.Start();
        Console.WriteLine("WebSocket server started at ws://localhost:8083/test");
        
        while(true)
        {
            if(TestService.Active())
            {
                Thread.Sleep(100);
                TestService.SendMsg(DateTime.Now.ToString()); 
            }
        }

    }

    static void Main()
    {
        Task.Run(httpServer);
        Task.Run(webSocketServer);

        // Keep application running until user press a key
        ReadLine();
    }
}







