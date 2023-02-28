using Microsoft.AspNetCore.Hosting.Server;
using MonitoringService.MonitorServer;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Unicode;

namespace MonitorWebApp.Middleware
{
    public class WebSocketMiddleWare
    {
        RequestDelegate _next;
        public WebSocketMiddleWare(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IMonitorServer server)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            if (context.Request.Path == "/WEBAPI/WEBSOCKET/WebConReq")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    Console.WriteLine("WEBSOCK REQUEST");
                    var sock = await context.WebSockets.AcceptWebSocketAsync();
                    byte[] buff = new byte[1024];

                    var task = Task.Run(async () =>
                    {
                        await sock.ReceiveAsync(new ArraySegment<byte>(buff), CancellationToken.None);
                    });

                    var firstTask = await Task.WhenAny(task, Task.Delay(10000));
                    if (firstTask == task)
                    {
                        var logginMeg = UTF8Encoding.UTF8.GetString(buff, 0, 1024);
                        if (logginMeg.Contains(PacketHelper.ACTOR.MONITORD.ToString()))
                        {
                            var ipPort = logginMeg.Split('|').Last();
                            Console.WriteLine(logginMeg);
                            server.addWebsocket(sock, tcs, PacketHelper.ACTOR.MONITORD.ToString(), ipPort);
                            Console.WriteLine("adding complete");
                        }
                        else
                        {
                            var id = server.addWebsocket(sock, tcs, "CONNECTION");
                        }
                    }
                    else
                    {
                        Console.WriteLine("recieve timed out");
                        tcs.SetResult(false);
                    }

                    //
                    //byte[] message =  UTF8Encoding.UTF8.GetBytes(id.ToString());
                    //sock.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true, CancellationToken.None);

                    await tcs.Task;
                    Console.WriteLine("WEBSOCK Close MiddleWare is gone");
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                }
            }
            else
            {
                await _next(context);
            }
        }
    }
}
