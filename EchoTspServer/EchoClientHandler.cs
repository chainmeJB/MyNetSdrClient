using EchoServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EchoServer
{
    public class EchoClientHandler : IClientHandler
    {
        private readonly ILogger _logger;

        public EchoClientHandler(ILogger logger)
        {
            _logger = logger;
        }

        public async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            using (NetworkStream stream = client.GetStream())
            {
                byte[] buffer = new byte[8192];
                int bytesRead;

                try
                {
                    while (!token.IsCancellationRequested && (bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
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
                    client.Close();
                    _logger.Log("Client disconnected.");
                }
            }
        }
    }
}
