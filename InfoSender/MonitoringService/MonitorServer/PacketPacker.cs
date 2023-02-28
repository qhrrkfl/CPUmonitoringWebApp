using InfoSenderLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace MonitoringService.MonitorServer
{
    public class WebSockCon
    {
        public WebSockCon(WebSocket ws, TaskCompletionSource<object> tcs,Guid paramId)
        {
            _ws = ws;
            _tcs = tcs;
            topic = "CONNECTION";
            id = paramId;
        }


        public WebSockCon(WebSocket ws, TaskCompletionSource<object> tcs, string paramTargetip , string Paramtopic)
        {
            _ws = ws;
            _tcs = tcs;
            topic = Paramtopic;
            id = Guid.Empty;
            targetIP = paramTargetip.TrimEnd('\0');
        }
        public Guid id { get; private set; }
        public string targetIP { get; private set; }
        WebSocket _ws;
       public WebSocket WEBSOCK { get { return _ws; } }
        public TaskCompletionSource<object> _tcs;
        public string topic;


        public void release()
        {
            bool set = false;
            while (!set)
            {
                set = _tcs.TrySetResult(new object());
            }
        }
    }
    public static class PacketHelper
    {
        public enum ACTOR
        {
            IPLOGIN,
            IPLOGOUT,
            MONITORD,
            UUID
        }
        //장비 로그인 , 로그아웃
        public static byte[] logginMessage(string[] ips)
        {
            var iplist = string.Join(',', ips);
            iplist = ACTOR.IPLOGIN.ToString()+ iplist + RESPOND.HeaderEnd;
            Console.WriteLine("sending meesage " + iplist);
            return UTF8Encoding.UTF8.GetBytes(iplist);
        }

        public static byte[] packingUUID(string UUID)
        {
            string UUIDPACKET = ACTOR.UUID.ToString() + "|" + UUID + RESPOND.EOM;
           return  UTF8Encoding.UTF8.GetBytes(UUIDPACKET);
        }

        public static JObject parserData(string data)
        {
            int idxHeadEnd = data.IndexOf(RESPOND.HeaderEnd);
            string headers = data.Substring(0, idxHeadEnd);
            var lstKey = headers.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            string values = data.Substring(idxHeadEnd + RESPOND.HeaderEnd.Length, data.Length - idxHeadEnd + RESPOND.HeaderEnd.Length - RESPOND.EOM.Length);
            var lstValue = values.Split(',', StringSplitOptions.RemoveEmptyEntries);


            JObject json = new JObject();
            var cate = lstKey.First();
            lstKey.RemoveAt(0);
            json.Add(new JProperty(cate, new JObject()));
            int i = 0;

            foreach (var ele in lstKey)
            {
                ((JObject)json[cate]).Add(new JProperty(ele, lstValue[i]));
                i++;
            }

            return json;

        }

    }
}
