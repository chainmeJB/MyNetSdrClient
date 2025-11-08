using EchoServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EchoServerTests.Mocks
{
    public class MockClientHandler : IClientHandler
    {
        public bool Handled { get; private set; }

        public Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            Handled = true;
            return Task.CompletedTask;
        }
    }
}
