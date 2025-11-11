using EchoServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EchoServerTests
{
    [TestFixture]
    public class TcpListenerWrapperTests
    {
        [Test]
        public void Start_ShouldStartListener()
        {
            // Arrange
            var wrapper = new TcpListenerWrapper(IPAddress.Loopback, 0); // Port 0 = random available port

            // Act & Assert
            Assert.DoesNotThrow(() => wrapper.Start());

            // Cleanup
            wrapper.Stop();
        }

        [Test]
        public void Stop_ShouldStopListener()
        {
            // Arrange
            var wrapper = new TcpListenerWrapper(IPAddress.Loopback, 0);
            wrapper.Start();

            // Act & Assert
            Assert.DoesNotThrow(() => wrapper.Stop());
        }

        [Test]
        public void AcceptTcpClientAsync_ShouldWaitForConnection()
        {
            // Arrange
            var wrapper = new TcpListenerWrapper(IPAddress.Loopback, 0);
            wrapper.Start();

            // Act
            var acceptTask = wrapper.AcceptTcpClientAsync();

            // Assert
            Assert.IsFalse(acceptTask.IsCompleted); // Should be waiting for connection

            // Cleanup
            wrapper.Stop();
        }
    }

}
