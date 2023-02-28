using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Diagnostics.PerformanceData;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using InfoSenderLib;

namespace InfoSenderLib.Counters
{
    // <시간,퍼센티지>의 데이터를 계속 생성


    public class CpuCounter :  ICounterTick
    {
        readonly COUNTERCATE myCate;
        PerformanceCounter counter;
        public CountData cd;
        public CpuCounter()
        {
            myCate = COUNTERCATE.CPU;
            counter = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total");
            this.tick();
        }

        public void tick()
        {
            Thread.Sleep(1000);
            cd.usage = counter.NextValue();
            cd.usage = Math.Min(100f, cd.usage);
            cd.processName = "Total";
            cd.countTime = DateTime.Now;
        }

        public byte[] getHeader()
        {

            string value = myCate.ToString()+ ",processName,usage,countTime,"   ;
          return  Encoding.UTF8.GetBytes(value);
          
        }

        public byte[] getBody()
        {

            StringBuilder sb = new StringBuilder();
            sb.Append(","+cd.processName.ToString());
            sb.Append(","+cd.usage.ToString());
            
            sb.Append("," + cd.countTime.ToString("yyyy-MM-ddTHH:mm:ssK"));
            Console.WriteLine(sb.ToString());
            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }


   
}
