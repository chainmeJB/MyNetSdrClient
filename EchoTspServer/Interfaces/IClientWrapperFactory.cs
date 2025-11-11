using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EchoServer.Interfaces
{
    public interface IClientWrapperFactory
    {
        IClientWrapper CreateClientWrapper(TcpClient client);
    }
}
