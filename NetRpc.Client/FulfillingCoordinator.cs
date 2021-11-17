using NetRpc.Common;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NetRpc.Client
{
  public class FulfillingCoordinator : IFrameHandler, ISender<Task<IMessage>>
  {
    private Dictionary<int, Func<IMessage>> _messageFactories = new Dictionary<int, Func<IMessage>>();
    private Dictionary<Guid, TaskCompletionSource<IMessage>> _pendingTasks = new Dictionary<Guid, TaskCompletionSource<IMessage>>();
    private Dictionary<int, IReceiver> _typedHandlers = new Dictionary<int, IReceiver>();

    public void RegisterMessageFactory(int type, Func<IMessage> factory)
    {
      _messageFactories[type] = factory;
    }

    public void Receive(IContext context, int type, byte[] buff)
    {
      if (!_messageFactories.ContainsKey(type))
        return;
      var msg = _messageFactories[type]();
      msg.Decode(buff);
      if (!_pendingTasks.ContainsKey(msg.GetGuid()))
      {
        if (!_typedHandlers.ContainsKey(msg.Type()))
          return;
        _typedHandlers[msg.Type()].Receive(new DefaultContext()
        {
          client = context.Client()
        }, msg);
        return;
      }
      var stage = _pendingTasks[msg.GetGuid()];
      _pendingTasks.Remove(msg.GetGuid());
      stage.TrySetResult(msg);
    }

    public Task<IMessage> Send(TcpClient client, IMessage message)
    {
      TaskCompletionSource<IMessage> task = new TaskCompletionSource<IMessage>();
      _pendingTasks[message.GetGuid()] = task;
      SendUtility.SendMessage(client.GetStream(), message);
      return (Task<IMessage>)task.Task;
    }

    public void Register(int type, IReceiver handler)
    {
      _typedHandlers[type] = handler;
    }
  }
}