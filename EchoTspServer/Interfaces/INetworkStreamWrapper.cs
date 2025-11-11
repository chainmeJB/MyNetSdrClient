using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EchoServer.Interfaces
{
    public interface INetworkStreamWrapper : IDisposable
    {
        Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken token);
        Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken token);
    }
}
