using EntityComponent;
using JumpKing.Player;
using JumpKingCoop.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingCoop.Utils
{
    public static class LocalPlayer
    {
        private static PlayerEntity playerEntity = null;

        private static PlayerEntity_Wrapper playerEntityWrapper = null;

        private static BodyComp_Wrapper bodyComp = null;

        public static PlayerEntity Entity 
        {
            get
            {
                if (playerEntity == null)
                    TryToGetLocalPlayer();

                return playerEntity;
            }
        }

        public static PlayerEntity_Wrapper EntityWrapper
        {
            get
            {
                if (playerEntityWrapper == null)
                    TryToGetLocalPlayer();

                return playerEntityWrapper;
            }
        }

        public static BodyComp_Wrapper BodyComp
        {
            get
            {
                if (bodyComp == null)
                    TryToGetLocalPlayer();

                return bodyComp;
            }
        }

        private static void TryToGetLocalPlayer()
        {
            if (playerEntity == null)
            {
                playerEntity = EntityManager.instance.Find<PlayerEntity>();
                playerEntityWrapper = new PlayerEntity_Wrapper(playerEntity);
                bodyComp = playerEntityWrapper.GetBodyComp();
            }
        }
    }
}
