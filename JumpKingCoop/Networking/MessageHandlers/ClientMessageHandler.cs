using EntityComponent;
using JumpKing;
using JumpKing.Player;
using JumpKing.Util;
using JumpKingCoop.Networking.Connection;
using JumpKingCoop.Networking.Entities;
using JumpKingCoop.Networking.Messages;
using JumpKingCoop.Networking.Serialization;
using JumpKingCoop.Networking.Sessions;
using JumpKingCoop.Networking.Utils;
using JumpKingCoop.Utils;
using JumpKingCoop.Wrappers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingCoop.Networking.MessageHandlers
{
    public abstract class ClientMessageHandler : MessageHandler
    {
        #region Tasks
        protected enum JoinServerState
        {
            Started,
            Joined,
            Refused,
            Timeout
        }

        protected TimedTask<JoinServerState> joinServerTask = new TimedTask<JoinServerState>(JoinServerState.Started);

        protected enum CreateRemotePlayerState
        {
            Started,
            Success,
            Refused,
            Timeout
        }

        protected TimedTask<CreateRemotePlayerState> createRemotePlayerTask = new TimedTask<CreateRemotePlayerState>(CreateRemotePlayerState.Started);

        #endregion

        public ClientMessageHandler()
        {
        }

        public override void Update(float deltaTime)
        {
            joinServerTask.Update();
            base.Update(deltaTime);
        }

        public override void Start()
        {
            Socket.StartClient(NetworkManager.Config.IpAddress);
        }

        public override void Stop()
        {
            InternalMessageBroker.Clear();
            Socket.Stop();
        }

        public void AttemptConnect()
        {
            var joinRequest = new JoinRequest();
            joinRequest.Map = 0;
            joinRequest.Password = session.Password;
            EnqueueImmediateMessage(new JoinRequestMessage(session.Self, joinRequest));
            joinServerTask.Start(3.0f, () => 
            {
                OnJoinTimeout();
                return JoinServerState.Timeout; 
            });
        }

        public void CreateLocalPlayer()
        {
            var createEntityRequest = new RequestEntityCreation(session.Self, typeof(RemotePlayer));
            EnqueueMessage(createEntityRequest);
            createRemotePlayerTask.Start(3.0f, () =>
            {
                OnCreateLocalPlayerRefused("Timeout");
                return CreateRemotePlayerState.Timeout;
            });
        }

        public void UpdateEntityState(int index, EntityState state)
        {
            var updateEntityState = new UpdateEntityState(session.Self, index, state);
            EnqueueMessage(updateEntityState);
        }

        protected override void ProcessMessage(object message)
        {
            if (message is JoinRequestResponseMessage)
                HandleJoinRequestResponseMessage(message as JoinRequestResponseMessage);
            else if (message is RequestEntityCreationResponse)
                HandleCreateEntityResponse(message as RequestEntityCreationResponse);
            else if (message is CreateEntity)
                HandleCreateEntity(message as CreateEntity);
            else if (message is UpdateEntityState)
                HandleUpdateEntityState(message as UpdateEntityState);
            else if (message is RemoveEntity)
                HandleRemoveEntity(message as RemoveEntity);
        }

        protected override void OnClientTimeout(Client client)
        {
            //We are the client so the socket needs to be stopped
            session.Clients.Remove(client);
            Stop();
            OnServerDisconnected("Server connection lost");
        }

        protected void HandleJoinRequestResponseMessage(JoinRequestResponseMessage message)
        {
            if(message.RequestAccepted == false)
            {
                joinServerTask.Stop(JoinServerState.Refused);
                OnJoinRefused(message.RejectReason);
            }
            else
            {
                session.RegisterSelf(message.RemoteClient);
                session.RegisterClient(message.Client);

                joinServerTask.Stop(JoinServerState.Joined);
                OnJoinedServer();
            }
        }

        protected void HandleCreateEntityResponse(RequestEntityCreationResponse message)
        {
            if (message.IsAccepted)
            {
                createRemotePlayerTask.Stop(CreateRemotePlayerState.Success);
                OnCreateLocalPlayerAccepted(message.Index);
            }
            else
            {
                createRemotePlayerTask.Stop(CreateRemotePlayerState.Refused);
                OnCreateLocalPlayerRefused("Refused");
            }
        }

        protected void HandleCreateEntity(CreateEntity message)
        {
            OnCreateEntity(message.EntityType, message.EntityIndex);
        }

        protected void HandleUpdateEntityState(UpdateEntityState message)
        {
            OnUpdateEntityState(message.Index, message.State);
        }

        protected void HandleRemoveEntity(RemoveEntity message)
        {
            OnRemoveEntity(message.EntityIndex);
        }

        protected abstract void OnJoinedServer();
        protected abstract void OnJoinTimeout();
        protected abstract void OnJoinRefused(string reason);
        protected abstract void OnCreateLocalPlayerAccepted(int index);
        protected abstract void OnCreateLocalPlayerRefused(string reason);
        protected abstract void OnCreateEntity(string type, int index);
        protected abstract void OnRemoveEntity(int index);
        protected abstract void OnUpdateEntityState(int index, EntityState state);
        protected abstract void OnServerDisconnected(string reason);
    }
}
