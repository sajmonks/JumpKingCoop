using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JumpKingCoop.Networking.Sessions
{
    public class Client
    {
        private const int ConnectionTimeout = 30;

        public long ID { get; set; }

        [XmlIgnore]
        public IPEndPoint Address { get; set; }

        [XmlIgnore]
        public DateTime LastUpdated { get; protected set; }

        [XmlIgnore]
        public DateTime LastLiveRequest { get; protected set; }

        [XmlIgnore]
        public float Ping { get; protected set; }

        public Client()
        {
            ID = 0;
            Address = new IPEndPoint(IPAddress.Any, 0);
            LastUpdated = DateTime.Now;
        }

        public Client(IPEndPoint owner, long id)
        {
            this.Address = owner;
            this.ID = id;
            this.LastUpdated = DateTime.Now;
        }

        public void UpdateTimestamp() => LastUpdated = DateTime.Now;

        public void UpdateLiveRequest() => LastLiveRequest = DateTime.Now;


        public void UpdatePing()
        {
            var timeNow = DateTime.Now;
            Ping = (float)(timeNow - LastLiveRequest).TotalMilliseconds;
        }

        public override bool Equals(Object other)
        {
            if ((other == null) || !this.GetType().Equals(other.GetType()))
            {
                return false;
            }
            else
            {
                Client otherSession = (Client)other;

                if (ID.Equals(otherSession.ID))
                {
                    if (otherSession.Address == null && Address == null)
                        return true;
                    else if (otherSession.Address != null && !otherSession.Address.Equals(Address))
                        return false;
                    else if (Address != null && !Address.Equals(otherSession.Address))
                        return false;
                    else
                        return true;
                }
                return false;
            }
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + ID.GetHashCode();
            hash = hash * 31 + (Address == null ? 0 : Address.GetHashCode());
            return hash;
        }
    }
}
