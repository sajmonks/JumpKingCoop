using JumpKingCoop.Networking.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingCoop.Networking.Messages
{
    public interface IAuthorizable
    {
        Client Client { get; set; }
    }
}
