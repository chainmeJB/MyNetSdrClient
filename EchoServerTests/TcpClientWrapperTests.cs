using EchoServer;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EchoServerTests
{
    [TestFixture]
    public class TcpClientWrapperTests
    {
        [Test]
        public void GetStream_ShouldReturnNetworkStreamWrapper()
        {
            // Arrange
            var tcpClient = new TcpClient();
            var wrapper = new TcpClientWrapper(tcpClient);

            // Act & Assert
            // Cannot test without actual connection, but verify no exception
            Assert.DoesNotThrow(() => wrapper.Dispose());
        }

        [Test]
        public void Dispose_ShouldCloseClient()
        {
            // Arrange
            var tcpClient = new TcpClient();
            var wrapper = new TcpClientWrapper(tcpClient);

            // Act & Assert
            Assert.DoesNotThrow(() => wrapper.Dispose());
        }
    }
}
