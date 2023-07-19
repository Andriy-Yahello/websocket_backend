
using System.Net.WebSockets;
using backend.Middleware;

var builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder.Services);
var app = builder.Build();
app.UseWebSockets();
app.MapGet("/", () => "Hello World!");
// app.Use(async (c, n) => {
    
//     Console.WriteLine("Request method: " + c.Request.Method);
// Console.WriteLine("Request protocol: " + c.Request.Protocol);
// WriteRequestParam(c);
    
// });
app.UseWebSocketServer();

static void WriteRequestParam(HttpContext c)
{
     if (c.Request. Headers != null){
        foreach (var item in c.Request.Headers)
        {
            Console.WriteLine($"{item.Key} {item.Value}");
        }
    }
}

app.Run();

static void ConfigureServices(IServiceCollection services)
{
    services.AddWebSocketManager();
}