using EchoServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EchoServerTests.Mocks
{
    public class MockLogger : ILogger
    {
        public List<string> Messages { get; } = new List<string>();
        public void Log(string message) => Messages.Add(message);
    }
}
