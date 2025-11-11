using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EchoServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var logger = new ConsoleLogger();
            using var server = new EchoServer(5000, logger);

            _ = Task.Run(() => server.StartAsync());

            string host = "127.0.0.1";
            int port = 60000;
            int intervalMilliseconds = 5000;

            using (var sender = new UdpTimedSender(host, port, logger))
            {
                logger.Log("Press any key to stop sending...");
                sender.StartSending(intervalMilliseconds);

                logger.Log("Press 'q' to quit...");
                while (Console.ReadKey(intercept: true).Key != ConsoleKey.Q)
                {
                    // Очікуємо натискання 'q'
                }

                sender.StopSending();
                server.Stop();
                logger.Log("Sender stopped.");
            }
        }
    }
}