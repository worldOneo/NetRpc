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
    private TcpClient client;
    private Dictionary<int, Func<Message>> messageFactories = new Dictionary<int, Func<Message>>();
    private Dictionary<Guid, TaskCompletionSource<Message>> pendingTasks = new Dictionary<Guid, TaskCompletionSource<Message>>();

    public RpcClient(IPAddress address, int port)
    {
      client = new TcpClient();
      client.Connect(address, port);
      ThreadPool.QueueUserWorkItem(o => Receive());
    }

    public void RegisterMessageFactory(int type, Func<Message> factory)
    {
      messageFactories[type] = factory;
    }

    public Task<Message> SendMessage(Message message)
    {
      TaskCompletionSource<Message> task = new TaskCompletionSource<Message>();
      pendingTasks[message.GetGuid()] = task;
      SendUtility.SendMessage(client.GetStream(), message);
      return (Task<Message>)task.Task;
    }
    private async void Receive()
    {
      while (true)
      {
        var rawMsg = await SendUtility.ReadFrame(client.GetStream(), 1_000_000);
        if (!messageFactories.ContainsKey(rawMsg.type))
          continue;
        var msg = messageFactories[rawMsg.type]();
        msg.Decode(rawMsg.buff);
        if (!pendingTasks.ContainsKey(msg.GetGuid()))
          continue;
        var stage = pendingTasks[msg.GetGuid()];
        pendingTasks.Remove(msg.GetGuid());
        stage.TrySetResult(msg);
      }
    }
  }
}