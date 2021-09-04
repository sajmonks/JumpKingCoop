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
    public class CreateEntity : IAuthorizable
    {
        public Client Client { get; set; }

        public string EntityType { get; set; }

        public int EntityIndex { get; set; }

        public CreateEntity()
        {
        }

        public CreateEntity(Client client, Type type, int index)
        {
            Client = client;
            EntityType = type.FullName;
            EntityIndex = index;
        }

        public CreateEntity(Client client, string type, int index)
        {
            Client = client;
            EntityType = type;
            EntityIndex = index;
        }
    }
}
