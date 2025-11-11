using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EchoServer;

namespace EchoServerTests
{
    [TestFixture]
    public class ConsoleLoggerTests
    {
        [Test]
        public void Log_ShouldNotThrow()
        {
            // Arrange
            var logger = new ConsoleLogger();

            // Act & Assert
            Assert.DoesNotThrow(() => logger.Log("Test message"));
        }
    }
}
