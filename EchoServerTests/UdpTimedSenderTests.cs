using EchoServer;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EchoServer.Interfaces;

namespace EchoServerTests
{
    [TestFixture]
    public class UdpTimedSenderTests
    {
        private Mock<ILogger> _mockLogger;

        [SetUp]
        public void SetUp()
        {
            _mockLogger = new Mock<ILogger>();
        }

        [Test]
        public void Constructor_ShouldInitialize()
        {
            // Arrange & Act
            using var sender = new UdpTimedSender("127.0.0.1", 5000, _mockLogger.Object);

            // Assert
            Assert.IsNotNull(sender);
        }

        [Test]
        public void StartSending_ShouldThrowWhenAlreadyRunning()
        {
            // Arrange
            using var sender = new UdpTimedSender("127.0.0.1", 5000, _mockLogger.Object);
            sender.StartSending(1000);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => sender.StartSending(1000));
            Assert.That(ex.Message, Does.Contain("already running"));

            // Cleanup
            sender.StopSending();
        }

        [Test]
        public void StopSending_ShouldStopTimer()
        {
            // Arrange
            using var sender = new UdpTimedSender("127.0.0.1", 5000, _mockLogger.Object);
            sender.StartSending(1000);

            // Act
            sender.StopSending();

            // Assert - no exception should be thrown
            Assert.Pass();
        }

        [Test]
        public void StopSending_ShouldNotThrowWhenNotStarted()
        {
            // Arrange
            using var sender = new UdpTimedSender("127.0.0.1", 5000, _mockLogger.Object);

            // Act & Assert
            Assert.DoesNotThrow(() => sender.StopSending());
        }

        [Test]
        public void Dispose_ShouldCleanupResources()
        {
            // Arrange
            var sender = new UdpTimedSender("127.0.0.1", 5000, _mockLogger.Object);
            sender.StartSending(1000);

            // Act & Assert
            Assert.DoesNotThrow(() => sender.Dispose());
        }
    }
}
