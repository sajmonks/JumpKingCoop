using JumpKingCoop.Networking.Entities;
using JumpKingCoop.Networking.Messages.Attributes;
using JumpKingCoop.Networking.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JumpKingCoop.Networking.Messages
{
    [Message]
    public class UpdateEntityState : IAuthorizable
    {
        public Client Client { get; set; }

        public int Index { get; set; }

        public EntityState State { get; set; }

        public UpdateEntityState()
        {
        }

        public UpdateEntityState(Client client, int index, EntityState state)
        {
            Client = client;
            Index = index;
            State = state;
        }
    }
}
