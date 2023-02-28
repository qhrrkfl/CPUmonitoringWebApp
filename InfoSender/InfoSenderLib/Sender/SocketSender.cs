using InfoSenderLib.Counters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace InfoSenderLib.Sender
{

    public class SocketSender
    {


        List<ICounterTick> tickers = new List<ICounterTick>();
        Socket client;
        System.Timers.Timer timer = null;
        State myState;
        IPEndPoint _ipEndPoint;
        public void addTicker(ICounterTick ticker)
        {
            tickers.Add(ticker);
        }
        public int elapseTime { get; private set; }
        CancellationTokenSource CTS;
        public bool isRunning { get; private set; }

        public bool init(IPEndPoint ipEndPoint)
        {
            isRunning = false;
            _ipEndPoint = ipEndPoint;
            client = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);


            try
            {
                myState = State.Connecting;
                client.Connect(ipEndPoint);
                myState = State.Idling;
                return true;
            }
            catch (Exception ex)
            {
                if (client.Connected)
                {
                    client.Disconnect(false);
                }

                this.timer = new System.Timers.Timer();
                this.timer.AutoReset = false;
                this.timer.Elapsed += new ElapsedEventHandler(reconnectingElapseEvent);
                this.timer.Interval = 5000;
                this.timer.Start();

                myState = State.Reconnecting;
                return false;
            }
        }



        void elapsedTickEvent(object sender, ElapsedEventArgs arg)
        {
            foreach (var ele in tickers)
            {
                ele.tick();
            }

            List<Byte> header = new List<byte>();
            List<Byte> Body = new List<Byte>();

            foreach (var ele in tickers)
            {
                header.AddRange(ele.getHeader());
                Body.AddRange(ele.getBody());
            }
            header.AddRange(RESPOND.GetHeaderEndByte());
            header.AddRange(Body);
            header.AddRange(RESPOND.GetEOMByte());
            int cntBytes = header.Count;
            Console.WriteLine("header" + cntBytes.ToString());
            var lenBytes = BitConverter.GetBytes(cntBytes);



            List<byte> message = new List<byte>();
            message.AddRange(lenBytes);
            message.AddRange(header);
            try
            {
                int ret = client.Send(message.ToArray(), SocketFlags.None);
            }
            catch(Exception ex)
            {
                ((System.Timers.Timer)sender).Enabled = false;
                myState = State.Reconnecting;
                if (myState == State.Reconnecting)
                {
                    Task.Run(() =>
                    {
                        Thread.Sleep(1000);
                        Console.WriteLine("connectFailed reconnectmodeStart");
                        this.setreconnectMode();
                    });
                }
            }
            if (myState == State.Sending)
            {
                ((System.Timers.Timer)sender).Enabled = true;
            }
        }

        void reconnectingElapseEvent(object sender, ElapsedEventArgs arg)
        {
            try
            {
                if (myState == State.Reconnecting)
                {
                    myState = State.Reconnecting;
                    Console.WriteLine("retry connect");
                    client.Connect(_ipEndPoint);
                    myState = State.Idling;
                    ((System.Timers.Timer)sender).Enabled = false;
                }
            }
            catch (Exception ex)
            {
                myState = State.Reconnecting;
                ((System.Timers.Timer)sender).Enabled = true;
            }
            finally
            {
                if (myState == State.Idling)
                {
                    Task.Run(() =>
                    {
                        Thread.Sleep(1000);
                        Console.WriteLine("connection retry successed sending starts");
                        this.tickStart(3000);
                    });
                }
            }
        }

        public void tickStart(int elapse)
        {
            if (myState == State.Idling)
            {
                if (this.timer != null)
                {
                    this.timer.Stop();
                    this.timer.Dispose();
                }
                this.timer = new System.Timers.Timer();
                this.timer.AutoReset = false;
                timer.Elapsed += new System.Timers.ElapsedEventHandler(elapsedTickEvent);
                timer.Interval = elapse;
                timer.Start();
                myState = State.Sending;
            }
        }

        void setreconnectMode()
        {
            if(this.timer != null)
            {
                this.timer.Enabled = false;
                timer.Dispose();
            }

            if(client != null)
            {
                client.Dispose();
                client = new Socket(_ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            }

            myState = State.Reconnecting;
            this.timer = new System.Timers.Timer();
            this.timer.AutoReset = false;
            this.timer.Elapsed += new ElapsedEventHandler(reconnectingElapseEvent);
            this.timer.Interval = 5000;
            this.timer.Start();
        }

        public void tickStop()
        {
            timer.Stop();
            myState = State.Stopped;
        }


        #region controlling myself




       

        #endregion
    }
}
