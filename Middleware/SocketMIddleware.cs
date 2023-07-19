using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;

namespace backend.Middleware
{
    public class SocketMiddleware
    {
        private readonly RequestDelegate n;

        private readonly ConnectionManager _manager;

        public SocketMiddleware(RequestDelegate next, ConnectionManager manager)
        {
            n = next;
            _manager = manager;
        }

        async Task Receive(WebSocket s, Action<WebSocketReceiveResult, byte[]> m)
        {
            var b = new byte[1024 * 4];
            while (s.State == WebSocketState.Open)
            {
                var r = await s.ReceiveAsync(buffer: new ArraySegment<byte>(b), cancellationToken: CancellationToken.None);
                m(r,b);

            }
        }



        private async Task RouteJSONMessageAsync(string message)
        {
            var routeIb = JsonConvert.DeserializeObject<SocketMessage>(message);
            // var routeIb = JsonConvert.DeserializeObject<dynamic>(message);
            // outeIb.To.Tostring()
            if (Guid.TryParse(routeIb.To, out Guid guidOutput))
            {
                Console.WriteLine("Targeted");
                var s = _manager.GetAllSockets().FirstOrDefault(_ => _.Key == routeIb.To.ToString());

                if (s.Value != null)
                {
                    if (s.Value.State == WebSocketState.Open)
                    {
                        await s.Value.SendAsync(Encoding.UTF8.GetBytes(routeIb.Message.ToString()), WebSocketMessageType.Text, true,CancellationToken.None);
                    }
                }
                else
                {
                    Console.WriteLine("Invalid recipient");
                }
            }
            else
            {
                Console.WriteLine("broadcast");
                foreach (var s in _manager.GetAllSockets())
                {
                    if (s.Value.State == WebSocketState.Open)
                    {
                        await s.Value.SendAsync(Encoding.UTF8.GetBytes(routeIb.Message.ToString()), WebSocketMessageType.Text, true,CancellationToken.None);
                    }
                }
            }
        }
        private async Task SendConnIdAsync(WebSocket socket, string id)
        {
            var buffer = Encoding.UTF8.GetBytes("ConnID: " + id);
            await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        public async Task InvokeAsync(HttpContext c)
        {
            if (c.WebSockets.IsWebSocketRequest) {
                WebSocket w = await c.WebSockets.AcceptWebSocketAsync();
                Console.WriteLine("Websocket connected");

                var id = _manager.AddSocket(w);
                await SendConnIdAsync(w, id);

                await Receive(w, async (result, buffer) => {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        Console.WriteLine("Message Received");
                        Console.WriteLine($"Message: {Encoding.UTF8.GetString(buffer, 0, result.Count)}");
                        await RouteJSONMessageAsync(Encoding.UTF8.GetString(buffer, 0, result.Count));
                        return;
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        var id = _manager.GetAllSockets().FirstOrDefault(_ => _.Value == w).Key;
                        Console.WriteLine("Received close message");

                        _manager.GetAllSockets().TryRemove(id, out WebSocket socket);
                        await socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                        return;
                    }
                });
            }
            else
            {
                Console.WriteLine("Request delegate 1");
                await n(c);
            }
        }
    }
}