using System.Net;
using System.Net.Sockets;
using NetRpc.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace NetRpc.Client
{
  public class RpcClient
  {
    private TcpClient _client;
    private Dictionary<int, Func<IMessage>> _messageFactories = new Dictionary<int, Func<IMessage>>();
    private Dictionary<Guid, TaskCompletionSource<IMessage>> _pendingTasks = new Dictionary<Guid, TaskCompletionSource<IMessage>>();

    public RpcClient(IPAddress address, int port)
    {
      _client = new TcpClient();
      _client.Connect(address, port);
      ThreadPool.QueueUserWorkItem(o => Receive());
    }

    public void RegisterMessageFactory(int type, Func<IMessage> factory)
    {
      _messageFactories[type] = factory;
    }

    public Task<IMessage> SendMessage(IMessage message)
    {
      TaskCompletionSource<IMessage> task = new TaskCompletionSource<IMessage>();
      _pendingTasks[message.GetGuid()] = task;
      SendUtility.SendMessage(_client.GetStream(), message);
      return (Task<IMessage>)task.Task;
    }
    private async void Receive()
    {
      while (true)
      {
        var rawMsg = await SendUtility.ReadFrame(_client.GetStream(), 1_000_000);
        if (!_messageFactories.ContainsKey(rawMsg.type))
          continue;
        var msg = _messageFactories[rawMsg.type]();
        msg.Decode(rawMsg.buff);
        if (!_pendingTasks.ContainsKey(msg.GetGuid()))
          continue;
        var stage = _pendingTasks[msg.GetGuid()];
        _pendingTasks.Remove(msg.GetGuid());
        stage.TrySetResult(msg);
      }
    }
  }
}