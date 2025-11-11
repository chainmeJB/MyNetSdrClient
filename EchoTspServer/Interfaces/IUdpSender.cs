using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EchoServer.Interfaces
{
    public interface IUdpSender : IDisposable
    {
        void StartSending(int intervalMilliseconds);
        void StopSending();
    }
}
