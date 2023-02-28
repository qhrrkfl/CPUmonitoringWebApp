using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace InfoSenderLib
{
    public enum State
    {
        Connecting,
        Reconnecting,
        Idling,
        Sending,
        Stopped
    }

   

    public enum COUNTERCATE
    {
        CPU
    }
    public static class RESPOND
    {
        public static readonly string ACK = "|ACK|";
        public static readonly string EOM = "<|EOM|>";
        
        public static readonly string HeaderEnd = "|HEAD|";
        public static readonly int lenByteSize = sizeof(int);
        public static byte[] GetAckByte()
        {
            return UTF8Encoding.UTF8.GetBytes(ACK);
        }

        public  static byte[] GetEOMByte()
        {
            return UTF8Encoding.UTF8.GetBytes(EOM);
        }

        public static byte[] GetHeaderEndByte()
        {
            return UTF8Encoding.UTF8.GetBytes(HeaderEnd);
        }
    }
    public struct CountData
    {
        public string processName;
        public float usage;
        public DateTime countTime;
    }

 

    static public class IpUtill
    {
        // get MY Ip
        static public IPAddress GetMYIp()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }

            return null;
        }
    }
}
