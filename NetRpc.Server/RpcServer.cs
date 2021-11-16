using System.Net;
using System.Net.Sockets;
using NetRpc.Common;
using System.IO;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetRpc.Server
{
  public class RpcServer
  {
    private TcpListener tcpListener;
    private IFrameHandler _handler;
    private ManualResetEvent _acceptConnection = new ManualResetEvent(false);

    public Action<IOException> ErrorHandler { get; set; }
    public uint MaxMessageLength { get; set; } = 1_000_000;
    public RpcServer(IPAddress address, int port, IFrameHandler handler)
    {
      tcpListener = new TcpListener(address, port);
      this._handler = handler;
    }

    public Task Start()
    {
      tcpListener.Start();
      return listenForConnection();
    }

    public void Stop()
    {
      tcpListener.Stop();
    }

    private async Task listenForConnection()
    {
      while (true)
      {
        var client = await tcpListener.AcceptTcpClientAsync();
        Task.Run(() => handleConnection(client));
      }
    }
    private async Task handleConnection(TcpClient client)
    {
      using (client)
      {
        try
        {
          await readMessage(client);
        }
        catch (IOException exception)
        {
          if (ErrorHandler != null)
            ErrorHandler(exception);
        }
      }
    }

    private void sendMessage(TcpClient client, IMessage message)
      => SendUtility.SendMessage(client.GetStream(), message);

    private async Task readMessage(TcpClient client)
    {
      while (true)
      {
        var message = await SendUtility.ReadFrame(client.GetStream(), MaxMessageLength);
        this._handler.Receive(new DefaultContext()
        {
          client = client,
          respond = msg => sendMessage(client, msg)
        }, message.type, message.buff);
      }
    }
  }
}