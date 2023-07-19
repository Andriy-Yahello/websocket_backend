using System.Net.WebSockets;
using System.Collections.Concurrent;

namespace backend.Middleware
{
    public class ConnectionManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();

        public ConcurrentDictionary<string, WebSocket> GetAllSockets()
        {
            return _sockets;
        }

        public string AddSocket(WebSocket socket)
        {
            var id = Guid.NewGuid().ToString();
            _sockets.TryAdd(id, socket);
            Console.WriteLine("Connection Added: " + id);

            return id;
        }
    }
}