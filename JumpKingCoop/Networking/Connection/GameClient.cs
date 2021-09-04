using EntityComponent;
using JumpKing;
using JumpKing.Player;
using JumpKingCoop.Networking.Entities;
using JumpKingCoop.Networking.MessageHandlers;
using JumpKingCoop.Networking.Sessions;
using JumpKingCoop.Networking.Utils;
using JumpKingCoop.Utils;
using JumpKingCoop.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingCoop.Networking.Connection
{
    public sealed class GameClient : ClientMessageHandler
    {
        private int EntityIndex = -1;

        private bool JoinedServer = false;

        private Dictionary<int, Entity> entities = new Dictionary<int, Entity>();

        public override void Start()
        {
            base.Start();
            AttemptConnect();
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (JoinedServer && EntityIndex > 0)
                UpdatePlayer();
        }

        protected override void OnJoinedServer()
        {
            TextScheduler.ScheduleText("Joined the server");
            CreateLocalPlayer();
            JoinedServer = true;
        }

        protected override void OnJoinRefused(string reason)
        {
            TextScheduler.ScheduleText("Connection refused: " + reason);
        }

        protected override void OnJoinTimeout()
        {
            TextScheduler.ScheduleText("Connection to server timed out.");
        }

        protected override void OnCreateLocalPlayerAccepted(int index)
        {
            EntityIndex = index;
        }

        protected override void OnCreateLocalPlayerRefused(string reason)
        {
            EntityIndex = -1;
        }

        protected override void OnCreateEntity(string type, int index)
        {
            if (!entities.ContainsKey(index))
                entities.Add(index, TypeHelper.InstantiateNetworkEntity<RemotePlayer>(type));
        }

        protected override void OnRemoveEntity(int index)
        {
            if (entities.ContainsKey(index))
            {
                EntityManager.instance.RemoveObject(entities[index]);
                entities.Remove(index);
            }
        }

        protected override void OnUpdateEntityState(int index, EntityState state)
        {
            if(entities.ContainsKey(index))
                (entities[index] as INetworkEntity).UpdateEntity(state);
        }

        protected override void OnServerDisconnected(string reason)
        {
               TextScheduler.ScheduleText("Disconnected from the server: " + reason);

            foreach(var entity in entities)
                EntityManager.instance.RemoveObject(entity.Value);
        }

        private PlayerState lastPlayerState = null;

        private void UpdatePlayer()
        {
            var localPlayer = EntityManager.instance.Find<PlayerEntity>();
            var playerEntity = new PlayerEntity_Wrapper(localPlayer);
            var bodyComp = playerEntity.GetBodyComp();

            //Build a new player state for local player
            var localPlayerState = new PlayerState();
            localPlayerState.Position = bodyComp.GetPosition();
            localPlayerState.FlipEffect = playerEntity.GetFlipEffect();
            localPlayerState.Name = NetworkManager.Config.Nickname;

            //Get sprite name
            var playerSpriteType = typeof(JKContentManager.PlayerSprites);
            var spriteProperties = playerSpriteType.GetProperties();

            foreach (var spriteProperty in spriteProperties)
            {
                if (spriteProperty.GetValue(null) == playerEntity.GetSprite())
                {
                    localPlayerState.SpriteName = spriteProperty.Name;
                    break;
                }
            }

            if(!localPlayerState.Equals(lastPlayerState))
            {
                UpdateEntityState(EntityIndex, localPlayerState);
                lastPlayerState = localPlayerState;
            }
        }
    }
}
