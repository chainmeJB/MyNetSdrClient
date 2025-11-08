using EchoServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EchoServer
{
    public class EchoServer
    {
        private readonly int _port;
        private readonly ITcpListenerFactory _listenerFactory;
        private readonly ILogger _logger;
        private readonly IClientHandler _clientHandler;
        private TcpListener _listener;
        private CancellationTokenSource _cancellationTokenSource;

        public EchoServer(
            int port,
            ITcpListenerFactory listenerFactory,
            ILogger logger,
            IClientHandler clientHandler)
        {
            _port = port;
            _listenerFactory = listenerFactory;
            _logger = logger;
            _clientHandler = clientHandler;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task StartAsync()
        {
            _listener = _listenerFactory.Create(_port);
            _listener.Start();
            _logger.Log($"Server started on port {_port}.");

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    _logger.Log("Client connected.");

                    _ = Task.Run(() => _clientHandler.HandleClientAsync(client, _cancellationTokenSource.Token));
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }

            _logger.Log("Server shutdown.");
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _listener?.Stop();
            _cancellationTokenSource.Dispose();
            _logger.Log("Server stopped.");
        }
    }
}
