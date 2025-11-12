using NUnit.Framework;
using Moq;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using EchoServer;
using EchoServer.Interfaces;

namespace EchoServerTests
{
    [TestFixture]
    public class EchoServerTests
    {
        private Mock<ILogger> _mockLogger;
        private Mock<IClientWrapperFactory> _mockClientFactory;
        private Mock<ITcpListenerWrapper> _mockListener;
        private CancellationTokenSource _cts;

        [SetUp]
        public void SetUp()
        {
            _mockLogger = new Mock<ILogger>();
            _mockClientFactory = new Mock<IClientWrapperFactory>();
            _mockListener = new Mock<ITcpListenerWrapper>();
            _cts = new CancellationTokenSource();
        }

        [TearDown]
        public void TearDown()
        {
            _cts?.Dispose();
        }

        [Test]
        public void Constructor_ShouldInitializeWithDefaultDependencies()
        {
            // Arrange & Act
            var server = new EchoServer.EchoServer(5000);

            // Assert
            Assert.That(server.IsRunning, Is.False);
        }

        [Test]
        public void Constructor_ShouldAcceptCustomLogger()
        {
            // Arrange & Act
            var server = new EchoServer.EchoServer(5000, _mockLogger.Object);

            // Assert
            Assert.That(server, Is.Not.Null);
            Assert.That(server.IsRunning, Is.False);
        }

        [Test]
        public async Task StartAsync_ShouldStartServerAndHandleClients()
        {
            // Arrange
            var mockStream = new Mock<INetworkStreamWrapper>();
            var mockClient = new Mock<IClientWrapper>();
            var mockTcpClient = new Mock<TcpClient>();
            var cts = new CancellationTokenSource();

            byte[] testData = new byte[] { 1, 2, 3, 4, 5 };

            mockStream.SetupSequence(s => s.ReadAsync(It.IsAny<byte[]>(), 0, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(testData.Length))
                .Returns(Task.FromResult(0));

            mockStream.Setup(s => s.WriteAsync(It.IsAny<byte[]>(), 0, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            mockClient.Setup(c => c.GetStream()).Returns(mockStream.Object);
            mockClient.Setup(c => c.Dispose());

            _mockClientFactory.Setup(f => f.CreateClientWrapper(It.IsAny<TcpClient>()))
                .Returns(mockClient.Object);

            _mockListener.Setup(l => l.Start());
            _mockListener.Setup(l => l.Stop());
            _mockListener.SetupSequence(l => l.AcceptTcpClientAsync())
                .ReturnsAsync(mockTcpClient.Object)
                .Returns(Task.FromException<TcpClient>(new ObjectDisposedException("Listener")));

            Func<IPAddress, int, ITcpListenerWrapper> listenerFactory = (addr, port) => _mockListener.Object;
            var server = new EchoServer.EchoServer(5000, _mockLogger.Object, _mockClientFactory.Object, listenerFactory);

            // Act
            var serverTask = Task.Run(() => server.StartAsync());
            await Task.Delay(100); // Give server time to start
            server.Stop();
            await serverTask;

            // Assert
            _mockLogger.Verify(l => l.Log(It.Is<string>(msg => msg.Contains("Server started"))), Times.Once);
            _mockLogger.Verify(l => l.Log("Client connected."), Times.AtLeastOnce);
        }

        [Test]
        public async Task StartAsync_ShouldHandleMultipleClients()
        {
            // Arrange
            var mockStream = new Mock<INetworkStreamWrapper>();
            var mockClient1 = new Mock<IClientWrapper>();
            var mockClient2 = new Mock<IClientWrapper>();
            var mockTcpClient1 = new Mock<TcpClient>();
            var mockTcpClient2 = new Mock<TcpClient>();

            mockStream.SetupSequence(s => s.ReadAsync(It.IsAny<byte[]>(), 0, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(10))
                .Returns(Task.FromResult(0));

            mockStream.Setup(s => s.WriteAsync(It.IsAny<byte[]>(), 0, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            mockClient1.Setup(c => c.GetStream()).Returns(mockStream.Object);
            mockClient2.Setup(c => c.GetStream()).Returns(mockStream.Object);

            _mockClientFactory.SetupSequence(f => f.CreateClientWrapper(It.IsAny<TcpClient>()))
                .Returns(mockClient1.Object)
                .Returns(mockClient2.Object);

            _mockListener.Setup(l => l.Start());
            _mockListener.Setup(l => l.Stop());
            _mockListener.SetupSequence(l => l.AcceptTcpClientAsync())
                .ReturnsAsync(mockTcpClient1.Object)
                .ReturnsAsync(mockTcpClient2.Object)
                .Returns(Task.FromException<TcpClient>(new ObjectDisposedException("Listener")));

            Func<IPAddress, int, ITcpListenerWrapper> listenerFactory = (addr, port) => _mockListener.Object;
            var server = new EchoServer.EchoServer(5000, _mockLogger.Object, _mockClientFactory.Object, listenerFactory);

            // Act
            var serverTask = Task.Run(() => server.StartAsync());
            await Task.Delay(150);
            server.Stop();
            await serverTask;

            // Assert
            _mockLogger.Verify(l => l.Log("Client connected."), Times.AtLeast(2));
        }

        [Test]
        public async Task StartAsync_ShouldStopOnCancellation()
        {
            // Arrange
            var cts = new CancellationTokenSource();

            _mockListener.Setup(l => l.Start());
            _mockListener.Setup(l => l.Stop());
            _mockListener.Setup(l => l.AcceptTcpClientAsync())
                .Returns(async () =>
                {
                    await Task.Delay(5000);
                    return new TcpClient();
                });

            Func<IPAddress, int, ITcpListenerWrapper> listenerFactory = (addr, port) => _mockListener.Object;
            var server = new EchoServer.EchoServer(5000, _mockLogger.Object, _mockClientFactory.Object, listenerFactory);

            // Act
            var serverTask = Task.Run(() => server.StartAsync());
            await Task.Delay(50);
            server.Stop();
            await serverTask;

            // Assert
            _mockLogger.Verify(l => l.Log(It.Is<string>(msg => msg.Contains("Server started"))), Times.Once);
            _mockLogger.Verify(l => l.Log("Server shutdown."), Times.Once);
        }

        [Test]
        public async Task StartAsync_ShouldHandleClientExceptions()
        {
            // Arrange
            var mockTcpClient = new Mock<TcpClient>();

            _mockListener.Setup(l => l.Start());
            _mockListener.Setup(l => l.Stop());
            _mockListener.SetupSequence(l => l.AcceptTcpClientAsync())
                .ReturnsAsync(mockTcpClient.Object)
                .Returns(Task.FromException<TcpClient>(new ObjectDisposedException("Listener")));

            var mockClient = new Mock<IClientWrapper>();
            var mockStream = new Mock<INetworkStreamWrapper>();

            mockStream.Setup(s => s.ReadAsync(It.IsAny<byte[]>(), 0, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new IOException("Network error"));

            mockClient.Setup(c => c.GetStream()).Returns(mockStream.Object);
            _mockClientFactory.Setup(f => f.CreateClientWrapper(It.IsAny<TcpClient>()))
                .Returns(mockClient.Object);

            Func<IPAddress, int, ITcpListenerWrapper> listenerFactory = (addr, port) => _mockListener.Object;
            var server = new EchoServer.EchoServer(5000, _mockLogger.Object, _mockClientFactory.Object, listenerFactory);

            // Act
            var serverTask = Task.Run(() => server.StartAsync());
            await Task.Delay(150);
            server.Stop();
            await serverTask;

            // Assert
            _mockLogger.Verify(l => l.Log(It.Is<string>(msg => msg.Contains("Error:"))), Times.AtLeastOnce);
        }

        [Test]
        public void Stop_ShouldStopServer()
        {
            // Arrange
            Func<IPAddress, int, ITcpListenerWrapper> listenerFactory = (addr, port) => _mockListener.Object;
            var server = new EchoServer.EchoServer(5000, _mockLogger.Object, _mockClientFactory.Object, listenerFactory);

            // Act
            server.Stop();

            // Assert
            _mockLogger.Verify(l => l.Log("Server stopped."), Times.Once);
            Assert.That(server.IsRunning, Is.False);
        }

        [Test]
        public void Stop_ShouldNotThrowWhenCalledMultipleTimes()
        {
            // Arrange
            var server = new EchoServer.EchoServer(5000, _mockLogger.Object);

            // Act & Assert
            Assert.DoesNotThrow(() => server.Stop());
            Assert.DoesNotThrow(() => server.Stop());
        }

        [Test]
        public void Dispose_ShouldStopServer()
        {
            // Arrange
            var server = new EchoServer.EchoServer(5000, _mockLogger.Object);

            // Act
            server.Dispose();

            // Assert
            Assert.That(server.IsRunning, Is.False);
        }
    }
}