using JumpKingCoop.Networking.Messages.Attributes;
using JumpKingCoop.Networking.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JumpKingCoop.Networking.Messages
{
    public class JoinRequest
    {
        public int Map { get; set; }
        public string Password { get; set; }
    }

    [Message]
    public class JoinRequestMessage : IAuthorizable
    {
        public Client Client { get; set; }

        public JoinRequest Request { get; set; }

        public JoinRequestMessage()
        {
        }

        public JoinRequestMessage(Client client, JoinRequest request)
        {
            this.Client = client;
            this.Request = request;
        }
    }

    [Message]
    public class JoinRequestResponseMessage : IAuthorizable
    {
        public Client Client { get; set; }

        public Client RemoteClient { get; set; }

        public bool RequestAccepted { get; set; }

        public string RejectReason { get; set; }

        public JoinRequestResponseMessage()
        {
        }

        public JoinRequestResponseMessage(Client client, string rejectReason)
        {
            Client = client;
            RequestAccepted = false;
            RejectReason = rejectReason;
        }

        public JoinRequestResponseMessage(Client server, Client client)
        {
            RequestAccepted = true;
            RemoteClient = client;
            Client = server;
        }
    }
}
