using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingCoop.Networking.Entities
{
    public interface INetworkEntity
    {
        void UpdateEntity(EntityState state);
    }
}
