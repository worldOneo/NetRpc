using System.Net;
using System.Net.Sockets;
using NetRpc.Common;
using System.IO;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetRpc.Server
{
  public abstract class RpcServer
  {
    public static RpcServer<Task> create(IPAddress address, int port, IFrameHandler handler)
      => new RpcServer<Task>(address, port, handler, new Sender());
  }

  public class RpcServer<T> : RpcServer
  {
    private TcpListener tcpListener;
    private IFrameHandler _handler;
    private ISender<T> _sender;
    private ManualResetEvent _acceptConnection = new ManualResetEvent(false);

    public Action<IOException> ErrorHandler { get; set; }
    public uint MaxMessageLength { get; set; } = 1_000_000;

    public RpcServer(IPAddress address, int port, IFrameHandler handler, ISender<T> sender)
    {
      tcpListener = new TcpListener(address, port);
      _handler = handler;
      _sender = sender;
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

    private T sendMessage(TcpClient client, IMessage message)
      => _sender.Send(client, message);

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