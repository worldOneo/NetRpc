using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using NetRpc.Common;
using System.IO;
using System;

namespace NetRpc.Server
{
  public class RpcServer
  {
    private TcpListener tcpListener;
    private FrameHandler handler;
    private bool Running;

    public Action<IOException> ErrorHandler { get; set; }
    public int MaxMessageLength { get; set; } = 1_000_000;
    public RpcServer(IPAddress address, int port, FrameHandler handler)
    {
      tcpListener = new TcpListener(address, port);
      this.handler = handler;
    }

    public void Start()
    {
      tcpListener.Start();
      Running = true;
      listenForConnection();
    }

    public void Stop()
    {
      Running = false;
      tcpListener.Stop();
    }

    public void listenForConnection()
    {
      tcpListener.BeginAcceptTcpClient(handleConnection, null);
    }
    void handleConnection(IAsyncResult result)
    {
      listenForConnection();
      using (TcpClient client = tcpListener.EndAcceptTcpClient(result))
      {
        try
        {
          readMessage(client);
        }
        catch (IOException exception)
        {
          if (ErrorHandler != null)
            ErrorHandler(exception);
        }
      }
    }

    private void sendMessage(TcpClient client, Message message)
      => SendUtility.SendMessage(client.GetStream(), message);

    private void readMessage(TcpClient client)
    {
      while (true)
      {
        int type;
        var buff = SendUtility.ReadFrame(client.GetStream(), out type, MaxMessageLength);
        this.handler.Receive(new DefaultContext()
        {
          client = client,
          respond = msg => sendMessage(client, msg)
        }, type, buff);
      }
    }
  }
}