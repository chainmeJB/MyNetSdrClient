using EchoServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EchoServer
{
    public class DefaultTcpListenerFactory : ITcpListenerFactory
    {
        public TcpListener Create(int port) => new TcpListener(IPAddress.Any, port);
    }
}
