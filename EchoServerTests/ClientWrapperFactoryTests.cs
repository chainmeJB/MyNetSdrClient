using EchoServer;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EchoServerTests
{
    [TestFixture]
    public class ClientWrapperFactoryTests
    {
        [Test]
        public void CreateClientWrapper_ShouldReturnWrapper()
        {
            // Arrange
            var factory = new ClientWrapperFactory();
            var tcpClient = new TcpClient();

            // Act
            var wrapper = factory.CreateClientWrapper(tcpClient);

            // Assert
            Assert.IsNotNull(wrapper);
            Assert.IsInstanceOf<TcpClientWrapper>(wrapper);

            // Cleanup
            wrapper.Dispose();
        }
    }
}
