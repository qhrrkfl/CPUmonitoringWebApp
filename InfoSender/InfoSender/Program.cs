using System.Net.Sockets;
using System.Net;
using InfoSender;
using InfoSenderLib;
using System.Text;
using System.Diagnostics;
using System.Management;
using System.Net.NetworkInformation;
using InfoSenderLib.Counters;
using InfoSenderLib.Sender;
namespace InfoSender
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IPEndPoint ipEndPoint = null;
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    //1,024 to 65,535.
                    ipEndPoint = new IPEndPoint(ip, 6120);
                    break;
                }
            }
            if (ipEndPoint != null)
            {
                SocketSender sender = null;
                sender = new SocketSender();
                sender.init(ipEndPoint);
                sender.addTicker(new CpuCounter());
                sender.tickStart(1000);

            }
            while (true)
            {
                Thread.Sleep(1000);
                // waiting...
            }
        }
        #region Test Code sender
        /*
            static void Main(string[] args)
        {
            IPEndPoint ipEndPoint = null;
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    //1,024 to 65,535.
                    ipEndPoint = new IPEndPoint(ip, 5041);
                    break;
                }
            }
            if (ipEndPoint != null)
            {
                using Socket client = new(
         ipEndPoint.AddressFamily,
         SocketType.Stream,
         ProtocolType.Tcp);

                client.Connect(ipEndPoint);
                var eom = "<|EOM|>";
                CpuCounter cpu = new CpuCounter();
                cpu.tick();
                while (true)
                {

                    // Send message.
                    var message = cpu.getBody();
                    int ret = client.Send(message, SocketFlags.None);

                    // Receive ack.
                    var buffer = new byte[1_024];
                    var received = client.Receive(buffer, SocketFlags.None);
                    var response = Encoding.UTF8.GetString(buffer, 0, received);
                    if (response == "<|ACK|>")
                    {
                        Console.WriteLine(
                            $"Socket client received acknowledgment: \"{response}\"");
                        break;
                    }
                    // Sample output:
                    //     Socket client sent message: "Hi friends 👋!<|EOM|>"
                    //     Socket client received acknowledgment: "<|ACK|>"
                }

                client.Shutdown(SocketShutdown.Both);
            }
        }

    }
        */
        #endregion

        #region test code server
        //static void Main()
        //{
        //    IPEndPoint IendPoint;
        //    var myIp = IpUtill.GetMYIp();
        //    if (myIp != null)
        //    {
        //        IendPoint = new IPEndPoint(myIp, 5041);

        //        using Socket listener = new(
        //                        IendPoint.AddressFamily,
        //                        SocketType.Stream,
        //                        ProtocolType.Tcp);



        //        listener.Bind(IendPoint);
        //        listener.Listen(100);

        //        var handler =  listener.Accept();
        //        while (true)
        //        {
        //            // Receive message.
        //            var buffer = new byte[1_024];
        //            var received =  handler.Receive(buffer, SocketFlags.None);
        //            var response = Encoding.UTF8.GetString(buffer, 0, received);

        //            var eom = "<|EOM|>";
        //            if (response.IndexOf(eom) > -1 /* is end of message */)
        //            {
        //                Console.WriteLine(
        //                    $"Socket server received message: \"{response.Replace(eom, "")}\"");

        //                var ackMessage = "<|ACK|>";
        //                var echoBytes = Encoding.UTF8.GetBytes(ackMessage);
        //                handler.Send(echoBytes, 0);
        //                Console.WriteLine(
        //                    $"Socket server sent acknowledgment: \"{ackMessage}\"");

        //                break;
        //            }
        //        }
        //    }


        //}
        #endregion
        #region 계측 관한 테스트 코드들 
        /*
        static void Main(string[] args)
        {


            // GPU에 관해서는 NVAPI로 c++로 구현되어있어 포팅해야함.
            //https://stackoverflow.com/questions/36389944/c-sharp-performance-counter-help-nvidia-gpu



            //https://social.msdn.microsoft.com/Forums/en-US/37b8b63a-da32-4497-b570-3811a2255dee/how-to-get-disk-io-countersdisk-write-time-disk-read-time-using-cnet?forum=csharplanguage
            // disk io에 관해선 자세한 사항이 있다.
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in drives)
            {
                //There are more attributes you can use.
                //Check the MSDN link for a complete example.
                Console.WriteLine(drive.Name);
                if (drive.IsReady)
                {
                    Console.WriteLine("drive check");
                    // Console.WriteLine((drive.TotalFreeSpace / (1024 * 1024)) /(drive.TotalSize/ (1024 * 1024))*100 +"% available");;
                    Console.WriteLine(drive.TotalSize / (1024 * 1024 * 1024) + "total");
                    Console.WriteLine(drive.TotalFreeSpace / (1024d * 1024d * 1024d) + "available");
                    Console.WriteLine((drive.TotalFreeSpace / (1024d * 1024d * 1024d)) / (drive.TotalSize / (1024d * 1024d * 1024d)) * 100d);

                }

            }



            PerformanceCounterCategory category = new PerformanceCounterCategory("Network Interface");
            String[] instancename = category.GetInstanceNames();

            foreach (string ele in instancename)
            {
                double usagePercentage;
                double bytePerSec;
                getNetworkUtilization(ele, out usagePercentage, out bytePerSec);
                Console.WriteLine("percentage : " + usagePercentage + "  " + "speed : " + bytePerSec);
            }


            List<string> activeNW = new List<string>();
            // network 현재 사용량
            if (NetworkInterface.GetIsNetworkAvailable())
            {

                NetworkInterface[] interfaces
                    = NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface ni in interfaces)
                {
                    Console.WriteLine("network" + ni.Name);
                    Console.WriteLine("    Bytes Sent: {0}",
                        ni.GetIPv4Statistics().BytesSent);
                    Console.WriteLine("    Bytes Received: {0}",
                        ni.GetIPv4Statistics().BytesReceived);

                    if (ni.GetIPStatistics().BytesReceived > 1024 * 1024)
                    {
                        activeNW.Add(ni.Id);
                    }
                }
            }

            float physicalMemorySize;
            using (ManagementClass mc = new ManagementClass("Win32_OperatingSystem"))
            {
                using (ManagementObject o = mc.GetInstances().Cast<ManagementObject>().FirstOrDefault())
                {
                    physicalMemorySize = float.Parse(o["TotalVisibleMemorySize"].ToString());

                }
            }


            var cpuUsage = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            PerformanceCounter theMemCounter = new PerformanceCounter("Memory", "Available KBytes");
            var firstCall = cpuUsage.NextValue();
            var firstCAllMEme = theMemCounter.NextValue();


            for (int i = 0; i < 1; i++)
            {
                //1초 pause해야 real value를 가짐.
                Thread.Sleep(1000);
                Console.WriteLine(cpuUsage.NextValue() + "%");
                Console.WriteLine((physicalMemorySize - theMemCounter.NextValue()) / physicalMemorySize * 100 +
                    "%");
            }

            Console.Read();
        }


        public static void getNetworkUtilization(string networkCard, out double utilization, out double bytePerSec)
        {

            const int numberOfIterations = 10;

            PerformanceCounter bandwidthCounter = new PerformanceCounter("Network Interface", "Current Bandwidth", networkCard);
            float bandwidth = bandwidthCounter.NextValue();//valor fixo 10Mb/100Mn/

            PerformanceCounter dataSentCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", networkCard);

            PerformanceCounter dataReceivedCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", networkCard);

            float sendSum = 0;
            float receiveSum = 0;

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int index = 0; index < numberOfIterations; index++)
            {
                sendSum += dataSentCounter.NextValue();
                receiveSum += dataReceivedCounter.NextValue();
            }
            stopwatch.Stop();
            double second = stopwatch.Elapsed.TotalSeconds;
            float dataSent = sendSum;
            float dataReceived = receiveSum;


            utilization = (8 * (dataSent + dataReceived)) / (bandwidth * numberOfIterations) * 100;
            bytePerSec = (dataSent + dataReceived) / second;
        }


    }

        */
        #endregion



    }
}