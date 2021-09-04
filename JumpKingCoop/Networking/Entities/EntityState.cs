using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JumpKingCoop.Networking.Entities
{
    //#TODO better serialization for entity state??
    [XmlInclude(typeof(PlayerState))]
    public abstract class EntityState
    {
        public abstract void UpdateState(EntityState destinationState);
    }
}
