using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using EchoServer;
using EchoServerTests.Mocks;

namespace EchoServerTests
{
    [TestFixture]
    public class EchoServerTests
    {
        [Test]
        public async Task StartAsync_LogsStartAndStop()
        {
            var logger = new MockLogger();
            var listenerFactory = new MockListenerFactory();
            var clientHandler = new MockClientHandler();

            var server = new EchoServer.EchoServer(
                port: 0,
                listenerFactory: listenerFactory,
                logger: logger,
                clientHandler: clientHandler);

            var serverTask = server.StartAsync();
            await Task.Delay(100);
            server.Stop();

            Assert.IsTrue(logger.Messages.Any(m => m.Contains("Server started")), "Waiting for a start message");
            Assert.IsTrue(logger.Messages.Any(m => m.Contains("Server stopped")), "Waiting for a stop message");
        }

        [Test]
        public async Task EchoClientHandler_EchoesDataBack()
        {
            var logger = new MockLogger();
            var handler = new EchoClientHandler(logger);

            using var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;

            var acceptTask = listener.AcceptTcpClientAsync();

            using var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, port);

            using var serverClient = await acceptTask;

            var cts = new CancellationTokenSource();
            var handleTask = handler.HandleClientAsync(serverClient, cts.Token);

            using (var stream = client.GetStream())
            {
                var payload = System.Text.Encoding.UTF8.GetBytes("hello");
                await stream.WriteAsync(payload, 0, payload.Length);

                var buffer = new byte[payload.Length];
                int read = await stream.ReadAsync(buffer, 0, buffer.Length);

                Assert.AreEqual("hello", System.Text.Encoding.UTF8.GetString(buffer, 0, read));
            }

            cts.Cancel();
            await handleTask;

            Assert.IsTrue(logger.Messages.Any(m => m.Contains("Echoed")), "Echo message expected");
        }

        [Test]
        public void Constructor_Injection_Works()
        {
            var logger = new MockLogger();
            var listenerFactory = new MockListenerFactory();
            var handler = new MockClientHandler();

            var server = new EchoServer.EchoServer(12345, listenerFactory, logger, handler);

            Assert.IsNotNull(server);
            var fields = server.GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsTrue(fields.Length > 0);
        }
    }
}
