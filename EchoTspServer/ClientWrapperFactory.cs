using EchoServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EchoServer
{
    public class ClientWrapperFactory : IClientWrapperFactory
    {
        public IClientWrapper CreateClientWrapper(TcpClient client)
        {
            return new TcpClientWrapper(client);
        }
    }
}
