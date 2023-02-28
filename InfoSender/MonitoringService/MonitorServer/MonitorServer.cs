using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using InfoSenderLib;
using System.Net.WebSockets;
namespace MonitoringService.MonitorServer
{

    public class MonitorServer : IMonitorServer
    {
        private Listener Listener;
        public MonitorServer(int Port)
        {
            IPEndPoint ipendpoint = null;
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    IPAddress myIp = ip;
                    ipendpoint = new IPEndPoint(myIp, Port);
                }
            }

            if (ipendpoint != null)
            {


                try
                {
                    Listener = new Listener(ipendpoint);
                    Listener.init(100);
                    Console.WriteLine("listener on");
                }
                catch (Exception ex)
                {

                }
            }
        }

        public Guid addWebsocket(WebSocket ws, TaskCompletionSource<object> tcs , string topic)
        {
            Guid id = Guid.NewGuid();
          
            var con = new WebSockCon(ws, tcs,id);
            if (Listener.webSocks.ContainsKey(topic))
            {
                var list = Listener.webSocks[topic];
                list.Add(con);
            }
            else
            {
                var list = new List<WebSockCon>();
                list.Add(con);
                Listener.webSocks.TryAdd(topic, list);
            }
            return id;
        }
        void IMonitorServer.addWebsocket(WebSocket ws, TaskCompletionSource<object> tcs, string topic, string targetKey)
        {
           
            var con = new WebSockCon(ws, tcs, targetKey.TrimEnd('\0') , topic);
            if (Listener.webSocks.ContainsKey(topic))
            {
                var list = Listener.webSocks[topic];
                list.Add(con);

            }
            else
            {
                var list = new List<WebSockCon>();
                list.Add(con);
                Listener.webSocks.TryAdd(topic, list);
            }
        }
    }

    public interface IMonitorServer
    {
        Guid addWebsocket(WebSocket ws, TaskCompletionSource<object> tcs,string topic);
        void addWebsocket(WebSocket ws, TaskCompletionSource<object> tcs, string topic , string targetKey);

    }
}
