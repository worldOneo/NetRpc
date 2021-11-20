using System.Threading.Tasks;
using System.Net.Sockets;
using System;

namespace NetRpc.Common
{
  public abstract class Interceptor
  {
    public Interceptor<Task> Create(IFrameHandler next, Func<IContext, int, byte[], bool> handlerInt)
      => new Interceptor<Task>(next, handlerInt, new Sender(), (cl, msg) => null);
    public Interceptor<Task> Create(IFrameHandler next, Func<IContext, int, byte[], bool> handlerInt, Func<TcpClient, IMessage, Task> senderInt)
      => new Interceptor<Task>(next, handlerInt, new Sender(), senderInt);
    public Interceptor<Task> Create(IFrameHandler next, Func<TcpClient, IMessage, Task> senderInt)
      => new Interceptor<Task>(next, (a, b, c) => true, new Sender(), senderInt);
  }
  public class Interceptor<T> : IFrameHandler, ISender<T>
  {
    protected IFrameHandler _nextHandler;
    protected Func<IContext, int, byte[], bool> _handlerInt;
    protected ISender<T> _nextSender;
    protected Func<TcpClient, IMessage, T> _senderInt;
    public Interceptor(IFrameHandler next, Func<IContext, int, byte[], bool> handlerInt, ISender<T> sender, Func<TcpClient, IMessage, T> senderInt)
    {
      _nextHandler = next;
      _nextSender = sender;
      _handlerInt = handlerInt;
      _senderInt = senderInt;
    }

    public void Receive(IContext ctx, int type, byte[] bytes)
    {
      bool con = _handlerInt(ctx, type, bytes);
      if (con)
        _nextHandler.Receive(ctx, type, bytes);
    }

    public T Send(TcpClient client, IMessage msg)
    {
      T next = _senderInt(client, msg);
      if (next == null)
        return _nextSender.Send(client, msg);
      return next;
    }
  }
}