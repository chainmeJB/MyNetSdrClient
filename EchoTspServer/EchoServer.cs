using EchoServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EchoServer
{
    public class EchoServer : IDisposable
    {
        private readonly int _port;
        private readonly ILogger _logger;
        private readonly IClientWrapperFactory _clientFactory;
        private readonly Func<IPAddress, int, ITcpListenerWrapper> _listenerFactory;

        private ITcpListenerWrapper _listener;
        private CancellationTokenSource _cancellationTokenSource;

        public bool IsRunning { get; private set; }

        public EchoServer(int port, ILogger logger = null,
            IClientWrapperFactory clientFactory = null,
            Func<IPAddress, int, ITcpListenerWrapper> listenerFactory = null)
        {
            _port = port;
            _logger = logger ?? new ConsoleLogger();
            _clientFactory = clientFactory ?? new ClientWrapperFactory();
            _listenerFactory = listenerFactory ?? ((addr, p) => new TcpListenerWrapper(addr, p));
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task StartAsync()
        {
            _listener = _listenerFactory(IPAddress.Any, _port);
            _listener.Start();
            IsRunning = true;
            _logger.Log($"Server started on port {_port}.");

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    _logger.Log("Client connected.");

                    _ = Task.Run(() => HandleClientAsync(client, _cancellationTokenSource.Token));
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.Log($"Error accepting client: {ex.Message}");
                }
            }

            IsRunning = false;
            _logger.Log("Server shutdown.");
        }

        internal virtual async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            using (var wrapper = _clientFactory.CreateClientWrapper(client))
            using (var stream = wrapper.GetStream())
            {
                try
                {
                    byte[] buffer = new byte[8192];
                    int bytesRead;

                    while (!token.IsCancellationRequested &&
                           (bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
                    {
                        await stream.WriteAsync(buffer, 0, bytesRead, token);
                        _logger.Log($"Echoed {bytesRead} bytes to the client.");
                    }
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    _logger.Log($"Error: {ex.Message}");
                }
                finally
                {
                    _logger.Log("Client disconnected.");
                }
            }
        }

        public void Stop()
        {
            if (!IsRunning && _cancellationTokenSource.IsCancellationRequested)
                return;

            _cancellationTokenSource.Cancel();
            _listener?.Stop();
            IsRunning = false;
            _logger.Log("Server stopped.");
        }

        public void Dispose()
        {
            Stop();
            _cancellationTokenSource?.Dispose();
        }
    }
}
