using System.Net;
using System.Net.Sockets;
using NetRpc.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace NetRpc.Client
{
  public class RpcClient<T>
  {
    public uint MaxMessageLength { get; set; } = 1_000_000;
    private TcpClient _client;
    private IFrameHandler _handler;
    private ISender<T> _sender;
    public RpcClient(IPAddress address, int port, IFrameHandler handler, ISender<T> sender)
    {
      _client = new TcpClient();
      _client.Connect(address, port);
      _handler = handler;
      _sender = sender;
      ThreadPool.QueueUserWorkItem(o => Receive());
    }

    public TcpClient Client() => _client;

    public T SendMessage(IMessage message)
    {
      return _sender.Send(_client, message);
    }
    private async void Receive()
    {
      while (true)
      {
        var rawMsg = await SendUtility.ReadFrame(_client.GetStream(), MaxMessageLength);
        _handler.Receive(new DefaultContext()
        {
          client = _client,
          respond = msg => SendMessage(msg)
        }, rawMsg.type, rawMsg.buff);
      }
    }
  }
}