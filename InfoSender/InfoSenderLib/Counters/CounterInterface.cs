using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoSenderLib.Counters
{
    public interface ICounterTick
    {
        /// <summary>
        /// onEVent
        /// </summary>
        public void tick();
        public byte[] getHeader();
        public byte[] getBody();
            

    }
}
