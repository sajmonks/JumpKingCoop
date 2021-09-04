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
    public class LiveRequest : IAuthorizable
    {
        public Client Client { get; set; }

        public LiveRequest()
        {
        }

        public LiveRequest(Client client)
        {
            Client = client;
        }
    }

    [Message]
    public class LiveResponse : IAuthorizable
    {
        public Client Client { get; set; }

        public LiveResponse()
        {
        }

        public LiveResponse(Client client)
        {
            Client = client;
        }
    }
}
