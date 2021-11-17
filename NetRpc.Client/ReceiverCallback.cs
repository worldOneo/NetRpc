using NetRpc.Common;
using System;

namespace NetRpc.Client
{
  public class ReceiverCallback : IReceiver
  {
    private Action<IReceiveContext, IMessage> _callback;
    public ReceiverCallback(Action<IReceiveContext, IMessage> callback)
    {
      _callback = callback;
    }

    public void Receive(IReceiveContext context, IMessage message) => _callback(context, message);
  }
}