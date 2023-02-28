using InfoSenderLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MonitoringService.MonitorServer
{
    public class Listener
    {
        string logrepository;
        Task listeningTask;
        Task pollingThread;
        Task WebSockConnectionCheck;
        IPEndPoint _serverIP;
        Socket listener;
        CancellationTokenSource ListenTokensoruce;
        CancellationTokenSource pollingTokenSource;
        ConcurrentDictionary<string, Socket> _Connection = new ConcurrentDictionary<string, Socket>();
        public ConcurrentDictionary<string, List<WebSockCon>> webSocks = new ConcurrentDictionary<string, List<WebSockCon>>();



        public Listener(IPEndPoint serverIP)
        {
            _serverIP = serverIP;
            logrepository = @"E:\log";
        }

        async Task sendIps()
        {
            byte[] message = PacketHelper.logginMessage(_Connection.Keys.ToArray());
            if (!webSocks.ContainsKey("CONNECTION")) { return; }
            var ele = webSocks["CONNECTION"];
            List<Task<Tuple<WebSocketReceiveResult , WebSockCon>>> taskcompletion = new List<Task<Tuple<WebSocketReceiveResult , WebSockCon>>>();

            foreach(var sock in ele)
            { 
                byte[] rMessage = new byte[1024];
                taskcompletion.Add(Task<Tuple<WebSocketReceiveResult, WebSockCon>>.Run(async () =>
                {
                    WebSocketReceiveResult ret = null; 
                    try
                    {
                      await sock.WEBSOCK.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true, CancellationToken.None);
                        Console.WriteLine("send");
                    }
                    catch(Exception ex)
                    {

                    }

                    try
                    {
                        CancellationTokenSource cts = new CancellationTokenSource();
                        System.Timers.Timer t = new System.Timers.Timer();
                        t.Elapsed += (object obj, ElapsedEventArgs evt) => { sock.topic = "timeout";  Console.WriteLine("timeout");  cts.Cancel(); };
                        t.AutoReset = false;
                        t.Interval = 5000;
                        t.Start();
                        ret = await sock.WEBSOCK.ReceiveAsync(new ArraySegment<byte>(rMessage), cts.Token);
                        t.Stop();
                        t.Close();
                        t.Dispose();
                        Console.WriteLine("rev");
                    }
                    catch(Exception ex)
                    {

                    }
                    return new Tuple<WebSocketReceiveResult, WebSockCon>(ret,sock);
                }));
            }

           await Task<Tuple<WebSocketReceiveResult, WebSockCon>>.WhenAny(taskcompletion.ToArray());

            foreach(var conCheck in taskcompletion)
            {
                var tu = conCheck.Result;
                if ((tu.Item1 != null&&tu.Item1.CloseStatus.HasValue) || (tu.Item1 == null && tu.Item2.topic == "timeout"))
                {

                    try
                    {
                        
                        await tu.Item2.WEBSOCK.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                    }
                    catch(Exception ex)
                    {

                    }
                    tu.Item2._tcs.TrySetResult(new object());
                    ele.Remove(tu.Item2);
                }
            }
        }

        async Task sendMonitorinfo(byte[] data ,string key)
        {
            byte[] message = data;
            if (!webSocks.ContainsKey(PacketHelper.ACTOR.MONITORD.ToString())) { return; }
            var ele = webSocks[PacketHelper.ACTOR.MONITORD.ToString()];
            List<Task<Tuple<WebSocketReceiveResult, WebSockCon>>> taskcompletion = new List<Task<Tuple<WebSocketReceiveResult, WebSockCon>>>();

            foreach (var sock in ele)
            {
                if (sock.targetIP == key)
                {
                    byte[] rMessage = new byte[1024];
                    taskcompletion.Add(Task<Tuple<WebSocketReceiveResult, WebSockCon>>.Run(async () =>
                    {
                        WebSocketReceiveResult ret = null;
                        try
                        {
                            await sock.WEBSOCK.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                        catch (Exception ex)
                        {

                        }

                        try
                        {
                            CancellationTokenSource cts = new CancellationTokenSource();
                            System.Timers.Timer t = new System.Timers.Timer();
                            t.Elapsed += (object obj, ElapsedEventArgs evt) => { sock.topic = "timeout"; Console.WriteLine("timeout"); cts.Cancel(); };
                            t.AutoReset = false;
                            t.Interval = 5000;
                            t.Start();
                            ret = await sock.WEBSOCK.ReceiveAsync(new ArraySegment<byte>(rMessage), cts.Token);
                            t.Stop();
                            t.Close();
                            t.Dispose();
                        }
                        catch (Exception ex)
                        {

                        }
                        return new Tuple<WebSocketReceiveResult, WebSockCon>(ret, sock);
                    }));
                }
            }
            if (taskcompletion.Count > 0)
            {
                await Task<Tuple<WebSocketReceiveResult, WebSockCon>>.WhenAny(taskcompletion.ToArray());
            }


            foreach (var conCheck in taskcompletion)
            {
                var tu = conCheck.Result;
                if ((tu.Item1 != null && tu.Item1.CloseStatus.HasValue) || (tu.Item1 == null && tu.Item2.topic == "timeout"))
                {

                    try
                    {
                        await tu.Item2.WEBSOCK.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {

                    }
                    tu.Item2._tcs.TrySetResult(new object());
                    ele.Remove(tu.Item2);
                }
            }
        }

        public bool init(int backlog)
        {
            try
            {
                listener = new Socket(_serverIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(_serverIP);
                ListenTokensoruce = new CancellationTokenSource();
                var ct = ListenTokensoruce.Token;

                listeningTask = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            break;
                        }
                        // blocking operation
                        listener.Listen(backlog);
                        var handle = listener.Accept();
                        var adress = ((IPEndPoint)handle.RemoteEndPoint).Address.ToString();
                        var port = ((IPEndPoint)handle.RemoteEndPoint).Port;
                        Console.WriteLine("acceptConnection : " + adress +":"+ port);
                        _Connection.TryAdd(adress +":"+ port, handle);

                         sendIps();
                    }
                }, ListenTokensoruce.Token);

                pollingTokenSource = new CancellationTokenSource();
                pollingThread = Task.Run(async () =>
                {
                    List<string> lstDeleteKey = new List<string>();
                    List<Task> lstwait = new List<Task>();
                    while (true)
                    {
                        Thread.Sleep(1000);

                        foreach (var ele in _Connection)
                        {
                            var soc = ele.Value;
                            string ip = ele.Key.ToString();
                            if (soc.Connected)
                            {
                                int buffersize = 1024;
                                byte[] buffer = new byte[buffersize];
                                lstwait.Add(soc.ReceiveAsync(buffer, SocketFlags.None).ContinueWith(x =>
                                {

                                    if (x.Exception != null && x.Exception.InnerException != null)
                                    {
                                        Console.WriteLine(((IPEndPoint)soc.RemoteEndPoint)?.Address.ToString() + "recieve error");
                                    }
                                    else
                                    {
                                        int revBytes = x.Result;
                                        if (revBytes == buffersize)
                                        {
                                            Console.WriteLine("버퍼 터진거 구현하세요");
                                        }
                                        else
                                        {
                                            byte[] message = new byte[revBytes];
                                            Buffer.BlockCopy(buffer, sizeof(int), message, 0, revBytes - sizeof(int));
                                            sendMonitorinfo(message, ip);
                                        }
                                    }

                                }));
                            }
                            else
                            {
                                Console.WriteLine(string.Format("{0} is disconnected", ele.Key.ToString()));
                                lstDeleteKey.Add(ele.Key);
                            }
                        }
                        if (lstDeleteKey.Count > 0)
                        {
                            foreach (var ele in lstDeleteKey)
                            {
                                Socket s;
                                _Connection.TryRemove(ele, out s);
                                try
                                {
                                    s.Dispose();
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                            sendIps();
                            lstDeleteKey.Clear();
                        }
                        //1tick 처리 기다림.
                        Task.WaitAll(lstwait.ToArray());
                    }
                }, pollingTokenSource.Token);

                //WebSockConnectionCheck = Task.Run(() =>
                //{

                //    while (true)
                //    {
                //        Thread.Sleep(1000);

                //        foreach (var ele in this.webSocks)
                //        {

                //            foreach (var inner in ele.Value)
                //            {
                //                if (inner.WEBSOCK.State == WebSocketState.Closed)
                //                {
                //                    inner._tcs.SetResult(new object());
                //                    try
                //                    {
                //                        inner.WEBSOCK.Dispose();
                //                    }
                //                    catch (Exception ex)
                //                    {

                //                    }

                //                }

                //                ele.Value.Remove(inner);
                //            }
                //        }
                //    }
                //});

            }
            catch (Exception ex)
            {
                ListenTokensoruce.Cancel();
                listeningTask.Wait();
                if (listener != null)
                {
                    listener.Close();
                    listener.Dispose();
                }

                return false;
            }

            return true;

        }

        void dispose()
        {
            ListenTokensoruce.Cancel();
            listeningTask.Wait();

            pollingTokenSource.Cancel();
            pollingThread.Wait();
            foreach (var ele in _Connection)
            {
                ele.Value.Disconnect(false);
                ele.Value.Dispose();
            }
            listener.Dispose();
        }



    }





}
