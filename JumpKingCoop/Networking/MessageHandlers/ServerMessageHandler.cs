using EntityComponent;
using JumpKing;
using JumpKing.Player;
using JumpKingCoop.Networking.Connection;
using JumpKingCoop.Networking.Entities;
using JumpKingCoop.Networking.Messages;
using JumpKingCoop.Networking.Serialization;
using JumpKingCoop.Networking.Sessions;
using JumpKingCoop.Utils;
using JumpKingCoop.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingCoop.Networking.MessageHandlers
{
    public abstract class ServerMessageHandler : MessageHandler
    {
        public ServerMessageHandler()
        {
        }

        public override void Start()
        {
            Socket.StartServer();
        }

        public override void Stop()
        {
            InternalMessageBroker.Clear();
            Socket.Stop();
        }

        protected void BroadcastCreateClient(string type, int index, Client except)
        {
            var message = new CreateEntity(session.Self, type, index);
            EnqueueMulticastSessionMessage(message, except);
        }

        protected void BroadcastEntityStateUpdate(int index, EntityState state, Client except)
        {
            var message = new UpdateEntityState(session.Self, index, state);
            EnqueueImmediateMulticastSessionMessage(message, except);
        }

        protected void BroadcastRemoveEntity(int index, Client except)
        {
            var message = new RemoveEntity(session.Self, index);
            EnqueueMulticastSessionMessage(message, except);
        }

        protected override void ProcessMessage(object message)
        {
            if (message is JoinRequestMessage)
                HandleOnJoinRequest(message as JoinRequestMessage);
            else if (message is RequestEntityCreation)
                HandleJoinEntityRequest(message as RequestEntityCreation);
            else if (message is UpdateEntityState)
                HandleUpdateEntityState(message as UpdateEntityState);
        }

        protected override void OnClientTimeout(Client client)
        {
            session.Clients.Remove(client);
            OnClientDisconnected(client);
        }

        private void HandleOnJoinRequest(JoinRequestMessage message)
        {
            string rejectReason = string.Empty;
            var result = OnJoinRequest(message.Client, message.Request, ref rejectReason);

            if (result)
            {
                var newClient = Session.GenerateClient(message.Client.Address, session);
                session.Clients.Add(newClient);
                EnqueueMessage(new JoinRequestResponseMessage(session.Self, newClient), message.Client);
                OnClientJoined(newClient);
            }
            else
                EnqueueMessage(new JoinRequestResponseMessage(session.Self, rejectReason), message.Client);
        }

        private void HandleJoinEntityRequest(RequestEntityCreation message)
        {
            int index = -1;
            var result = OnCreateEntityRequest(message.Client, message.EntityType, ref index);
            EnqueueMessage(new RequestEntityCreationResponse(session.Self, result, index), message.Client);

            if(result == true)
                OnCreateEntity(message.Client, message.EntityType, index);
        }

        private void HandleUpdateEntityState(UpdateEntityState updateEntityState)
        {
            OnEntityStateUpdate(updateEntityState.Client, updateEntityState.Index, updateEntityState.State);
        }

        protected abstract bool OnJoinRequest(Client client, JoinRequest joinRequest, ref string rejectReason);
        protected abstract void OnClientJoined(Client client);
        protected abstract void OnClientDisconnected(Client client);
        protected abstract bool OnCreateEntityRequest(Client client, string entityType, ref int index);
        protected abstract void OnCreateEntity(Client client, string entity, int index);
        protected abstract void OnEntityStateUpdate(Client client, int index, EntityState state);
    }
}
