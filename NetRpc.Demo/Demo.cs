using System;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace NetRpc.Demo
{
  class Program
  {
    static void Main(string[] args)
    {
      var rsa = new RSACryptoServiceProvider(4096);
      var pub = new RSACryptoServiceProvider();
      var priv = new RSACryptoServiceProvider();
      pub.ImportParameters(rsa.ExportParameters(false));
      priv.ImportParameters(rsa.ExportParameters(true));

      ThreadPool.QueueUserWorkItem(b =>
      {
        new RemoteServer(priv);
        Console.WriteLine("Server thread goes bye bye");
      });
      runDemo(pub).Wait();
    }

    async static Task runDemo(RSACryptoServiceProvider pub)
    {
      var client = new RemoteClient(pub);

      LoginResponse success;
      do
      {
        Console.Write("Password: ");
        var pw = await Task<string>.Run(() => Console.ReadLine());
        success = await client.Login("user", pw);
        Console.WriteLine("[Client] " + (success.Successful ? "Logged in" : "Login failed"));
      } while (!success.Successful);

      await Task<string>.Run(() => Console.ReadLine());
    }
  }
}
