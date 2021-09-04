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
    public class RequestEntityCreation : IAuthorizable
    {
        public Client Client { get; set; }

        public string EntityType { get; set; }

        public RequestEntityCreation()
        {
        }

        public RequestEntityCreation(Client client, Type entityType)
        {
            this.Client = client;
            this.EntityType = entityType.FullName;
        }
    }

    [Message]
    public class RequestEntityCreationResponse : IAuthorizable
    {
        public Client Client { get; set; }

        public bool IsAccepted { get; set; }

        public int Index { get; set; }

        public RequestEntityCreationResponse()
        {
        }

        public RequestEntityCreationResponse(Client client, bool isAccepted, int index)
        {
            this.Client = client;
            this.IsAccepted = isAccepted;
            this.Index = index;
        }
    }
}
