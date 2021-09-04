using JumpKingCoop.Networking.Connection;
using JumpKingCoop.Networking.Entities;
using JumpKingCoop.Networking.MessageHandlers;
using JumpKingCoop.Networking.Messages;
using JumpKingCoop.Networking.Sessions;
using JumpKingCoop.Networking.Utils;
using JumpKingCoop.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingCoop.Networking.Connection
{
    //#TODO How to synchronize to make sure that all players have the same number of created entities
    public sealed class GameServer : ServerMessageHandler
    {
        private class EntityEntry
        {
            public Client Client { get; set; }

            public string EntityType { get; set; }

            public EntityState State { get; set; }

            public EntityEntry(Client client, string entityType, EntityState state)
            {
                Client = client;
                EntityType = entityType;
                State = state;
            }
        }

        private int LastEntityIndex = 1;

        private Dictionary<int, EntityEntry> states = new Dictionary<int, EntityEntry>();

        //#TODO Should this be on this abstraction level???
        protected override bool OnJoinRequest(Client client, JoinRequest joinRequest, ref string rejectReason)
        {
            if(Session.Clients.Count >= NetworkManager.Config.MaxPlayers)
            {
                rejectReason = "Server is full!";
                return false;
            }

            if(!joinRequest.Password.Equals(NetworkManager.Config.SessionPassword))
            {
                rejectReason = "Wrong password!";
                return false;
            }

            return true;
        }

        protected override void OnClientJoined(Client client)
        {
            TextScheduler.ScheduleText("New player has joined the game.");
            BroadcastAllCreateClient();
            
            //#TODO better approach?
            foreach(var state in states)
            {
                //Send out all states to all players
                //Except the state of the newly joined client
                if(!state.Value.Client.Equals(client))
                {
                    BroadcastEntityStateUpdate(state.Key, state.Value.State, null);
                }
            }
        }

        protected override bool OnCreateEntityRequest(Client client, string entityType, ref int index)
        {
            var isNetworkEntity = TypeHelper.InheritsInterface(entityType, typeof(INetworkEntity));
            var hasGetEntityStateType = TypeHelper.GetNetworkEntityStateType(entityType) != null;
            var hasNoOtherRegisteredStates = !states.Values.Any(x => x.Client.Equals(client));

            if (isNetworkEntity && hasNoOtherRegisteredStates && hasGetEntityStateType)
            {
                index = LastEntityIndex++;
                return true;
            }

            return false;
        }

        protected override void OnCreateEntity(Client client, string entityType, int index)
        {
            var entityStateType = TypeHelper.GetNetworkEntityStateType(entityType);
            var entityState = Activator.CreateInstance(entityStateType) as EntityState;
            states.Add(index, new EntityEntry(client, entityType, entityState));
            BroadcastCreateClient(entityType, index, client);
        }

        protected override void OnEntityStateUpdate(Client client, int index, EntityState state)
        {
            if(states.ContainsKey(index))
            {
                var entityEntry = states[index];
                if(client.Equals(entityEntry.Client))
                {
                    entityEntry.State.UpdateState(state);
                    BroadcastEntityStateUpdate(index, state, client);
                }
            }
        }

        protected override void OnClientDisconnected(Client client)
        {
            TextScheduler.ScheduleText("Client disconnected");

            //Remove entities of the inactive player
            var inactiveStates = states.Where(x => x.Value.Client.Equals(client)).ToArray();
            foreach (var inactiveState in inactiveStates)
            {
                BroadcastRemoveEntity(inactiveState.Key, client);
                states.Remove(inactiveState.Key);
            }
        }

        private void BroadcastAllCreateClient()
        {
            foreach (var state in states)
                BroadcastCreateClient(state.Value.EntityType, state.Key, state.Value.Client);
        }
    }
}
