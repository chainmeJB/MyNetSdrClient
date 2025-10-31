using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetSdrClientApp.Networking;

namespace NetSdrClientApp.Messages
{
    public class ViolatingMessage
    {
        public TcpClientWrapper tcpWrapper { get; set; }
    }
}