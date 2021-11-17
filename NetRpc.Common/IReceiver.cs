namespace NetRpc.Common
{
  public interface IReceiver
  {
    void Receive(IReceiveContext ctx, IMessage Message);
  }
}