using JumpKingCoop.Networking.Messages.Attributes;
using JumpKingCoop.Networking.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingCoop.Networking.Messages
{
    [Message]
    public class RemoveEntity : IAuthorizable
    {
        public Client Client { get; set; }

        public int EntityIndex { get; set; }

        public RemoveEntity()
        {
        }

        public RemoveEntity(Client client, int index)
        {
            Client = client;
            EntityIndex = index;
        }
    }
}
