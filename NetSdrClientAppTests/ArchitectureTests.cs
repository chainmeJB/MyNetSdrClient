using NetArchTest.Rules;
using NetSdrClientApp.Messages;

namespace NetSdrClientTests
{
    public class ArchitectureTests
    {
        [Test]
        public void Messages_Should_Not_HaveDependencyOn_Networking()
        {
            var result = Types.InAssembly(typeof(NetSdrMessageHelper).Assembly)
                .That()
                .ResideInNamespace("NetSdrClientApp.Messages")
                .ShouldNot()
                .HaveDependencyOn("NetSdrClientApp.Networking")
                .GetResult();

            Assert.IsTrue(result.IsSuccessful, $"Messages depend on Networking: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
        }

        [Test]
        public void Networking_Should_Not_HaveDependencyOn_Messages()
        {
            var result = Types.InAssembly(typeof(NetSdrMessageHelper).Assembly)
                .That()
                .ResideInNamespace("NetSdrClientApp.Networking")
                .ShouldNot()
                .HaveDependencyOn("NetSdrClientApp.Messages")
                .GetResult();

            Assert.IsTrue(result.IsSuccessful, $"Networking depends on Messages: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
        }

        [Test]
        public void Messages_Should_Not_HaveDependencyOn_EchoTspServer()
        {
            var result = Types.InAssembly(typeof(NetSdrMessageHelper).Assembly)
                .That()
                .ResideInNamespace("NetSdrClientApp.Messages")
                .ShouldNot()
                .HaveDependencyOn("EchoTspServer")
                .GetResult();

            Assert.IsTrue(result.IsSuccessful, $"Messages depend on EchoTspServer: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
        }
    }
}