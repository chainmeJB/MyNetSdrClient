using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EchoServer
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var logger = new ConsoleLogger();
            var listenerFactory = new DefaultTcpListenerFactory();
            var handler = new EchoClientHandler(logger);

            var server = new EchoServer(port: 5000, listenerFactory, logger, handler);

            await server.StartAsync();
        }
    }
}