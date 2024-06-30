using MessengerModels;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

#pragma warning disable SYSLIB0011

namespace MessengerClient
{
    static class CurrentUser
    {
        public static event EventHandler? UserChanged;
        private static User? user;
        public static User? User
        {
            get => user;
            set
            {
                if (user != value)
                {
                    user = value;
                    UserChanged?.Invoke(null, null!);
                }
            }
        }
        public static async Task<Response> SendMessageAsync(Request request)
        {
            try
            {
                using TcpClient acceptor = new("127.0.0.1", 9001);
                await using NetworkStream ns = acceptor.GetStream();

                BinaryFormatter bf = new();

                bf.Serialize(ns, request);

                Response response = (Response)bf.Deserialize(ns);

                if (response.Message is null)
                {
                    return response;
                }
                else
                {
                    throw new Exception(response.Message);
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
