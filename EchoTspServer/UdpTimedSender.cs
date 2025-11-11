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
    public class UdpTimedSender : IUdpSender
    {
        private readonly string _host;
        private readonly int _port;
        private readonly ILogger _logger;
        private readonly UdpClient _udpClient;
        private readonly Random _random;
        private Timer _timer;
        private ushort _sequenceNumber = 0;

        public UdpTimedSender(string host, int port, ILogger logger = null)
        {
            _host = host;
            _port = port;
            _logger = logger ?? new ConsoleLogger();
            _udpClient = new UdpClient();
            _random = new Random();
        }

        public void StartSending(int intervalMilliseconds)
        {
            if (_timer != null)
                throw new InvalidOperationException("Sender is already running.");

            _timer = new Timer(SendMessageCallback, null, 0, intervalMilliseconds);
        }

        private void SendMessageCallback(object state)
        {
            try
            {
                byte[] samples = new byte[1024];
                _random.NextBytes(samples);
                _sequenceNumber++;

                byte[] msg = CombineArrays(
                    new byte[] { 0x04, 0x84 },
                    BitConverter.GetBytes(_sequenceNumber),
                    samples
                );

                var endpoint = new IPEndPoint(IPAddress.Parse(_host), _port);
                _udpClient.Send(msg, msg.Length, endpoint);
                _logger.Log($"Message sent to {_host}:{_port}");
            }
            catch (Exception ex)
            {
                _logger.Log($"Error sending message: {ex.Message}");
            }
        }

        private byte[] CombineArrays(params byte[][] arrays)
        {
            int totalLength = 0;
            foreach (var arr in arrays)
                totalLength += arr.Length;

            byte[] result = new byte[totalLength];
            int offset = 0;
            foreach (var arr in arrays)
            {
                Buffer.BlockCopy(arr, 0, result, offset, arr.Length);
                offset += arr.Length;
            }
            return result;
        }

        public void StopSending()
        {
            _timer?.Dispose();
            _timer = null;
        }

        public void Dispose()
        {
            StopSending();
            _udpClient?.Dispose();
        }
    }
}
