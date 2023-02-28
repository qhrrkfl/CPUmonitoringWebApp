using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Http;
using System.Reflection.Metadata.Ecma335;
using System.Net.Sockets;

namespace MonitoringService.MonitorServer
{
    // stack overlfow 해법은 예전 mvc같고 
    // msdn의 해법은 미들웨어를 만들라는 거였따.

    public class WebSocketHandler : ConnectionHandler
    {
        IMonitorServer _server;
        public WebSocketHandler(ILogger<WebSocketHandler> logger, IMonitorServer server)
        {
            int a = 0;
            _server = server;
        }

        public override async Task OnConnectedAsync(ConnectionContext c)
        {

            var Http = c.GetHttpContext();
            if (Http.WebSockets.IsWebSocketRequest)
            {
                try
                {
                    var socket = await c.GetHttpContext().WebSockets.AcceptWebSocketAsync();
                  
                 
                }
                catch(Exception ex)
                {

                }
            }

           

        }
    }
}
