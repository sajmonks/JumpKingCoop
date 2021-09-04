using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingCoop.Networking.Sessions
{
    public class Session
    {
        public List<Client> Clients { get; private set; } = new List<Client>();

        public Client Self { get; private set; }

        public string Password { get; private set; }

        public Session(string password, Client self)
        {
            Password = password;
            Self = self;
        }

        public Session(string password)
        {
            Password = password;
        }

        public bool CanRegister(IPEndPoint owner)
        {
            return !OwnerExist(owner);
        }

        public void RegisterSelf(Client client)
        {
            Self = client;
        }

        public bool RegisterClient(Client client)
        {
            if(CanRegister(client.Address))
            {
                Clients.Add(client);
                return true;
            }
            return false;
        }

        public Client Exists(Client client)
        {
            return Clients.Where(x => x.Equals(client)).FirstOrDefault();
        }


        public bool OwnerExist(IPEndPoint owner)
        {
            return Clients.Any(x => x.Address == owner);
        }

        public bool IdExist(long id)
        {
            return Clients.Any(x => x.ID == id);
        }

        public static Client GenerateClient(string address, int port, Session currentSession = null)
        {
            var owner = new IPEndPoint(IPAddress.Parse(address), port);
            return GenerateClient(owner, currentSession);
        }

        public static Client GenerateClient(IPEndPoint owner, Session currentSession = null)
        {
            if (currentSession == null)
            {
                return new Client(owner, new Random().Next());
            }
            else
            {
                bool isUnique = false;
                long temporaryId = 0;
                do
                {
                    temporaryId = new Random().Next();
                    isUnique = !currentSession.IdExist(temporaryId) && !currentSession.IdExist(currentSession.Self.ID);
                }
                while (!isUnique);

                return new Client(owner, temporaryId);
            }
        }
    }
}
