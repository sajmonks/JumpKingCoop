using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JumpKingCoop.Networking
{
    public class NetworkConfig
    {
        public bool Enabled { get; set; }

        public bool Server { get; set; }

        public string IpAddress { get; set; }

        public int Port { get; set; }

        public string Nickname { get; set; }

        public string SessionPassword { get; set; }

        public int MaxPlayers { get; set; }

        [XmlIgnore]
        public IPEndPoint EndPoint { get; set; }

    }
}
