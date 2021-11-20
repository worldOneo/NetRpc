namespace NetRpc.Common
{
  public interface IExtendable
  {
    object Extra(string key);
    void Extra(string key, object extra);
  }
}