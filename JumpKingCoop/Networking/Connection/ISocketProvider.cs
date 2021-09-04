using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingCoop.Networking.Connection
{
    public interface ISocketProvider
    {
        UDPSocket Socket { get; }

        void Start();
        void Update(float deltaTime);
        void Stop();
    }
}
