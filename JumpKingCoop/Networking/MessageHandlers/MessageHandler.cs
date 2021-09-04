using JumpKingCoop.Networking.Connection;
using JumpKingCoop.Networking.Messages;
using JumpKingCoop.Networking.Serialization;
using JumpKingCoop.Networking.Sessions;
using JumpKingCoop.Networking.Statistics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingCoop.Networking.MessageHandlers
{
    public abstract class MessageHandler : ISocketProvider
    {
        private Queue<RawMessage> messagesToSend = new Queue<RawMessage>();

        private const int MaxInactivity = 60;
        private const int LiveRequestPeriod = 1;
        private const int MaxMessagesInOneTick = 16;
        private const int Ticks = 30;
        private readonly float TickPeriod = 1.0f / Ticks;

        private float timePassedSinceLastTick = 0.0f;
        public NetworkStatistics Statistics { get; private set; } = new NetworkStatistics();

        protected Session session = new Session(NetworkManager.Config.SessionPassword, Session.GenerateClient(NetworkManager.Config.IpAddress, NetworkManager.Config.Port));

        public UDPSocket Socket { get; private set; } = new UDPSocket(NetworkManager.Config.Port);

        public Session Session { get => session; }

        public MessageHandler()
        {
        }

        public void EnqueueMessage<T>(T message, Client client = null)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var data = MessageSerializer.SerializeMessage(message);
            stopwatch.Stop();
            Statistics.SerializationTime.Feed(stopwatch.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L)));

            if(client != null)
                messagesToSend.Enqueue(new RawMessage(data, client.Address));
            else
                messagesToSend.Enqueue(new RawMessage(data, null));
        }

        public void EnqueueImmediateMessage<T>(T message, Client client = null)
        {
            EnqueueMessage(message, client);
            Flush();
        }

        public void EnqueueMulticastSessionMessage<T>(T message, Client except = null)
        {
            foreach (var client in session.Clients)
            {
                if (!client.Equals(except))
                    EnqueueMessage(message, client);
            }
        }

        public void EnqueueImmediateMulticastSessionMessage<T>(T message, Client except = null)
        {
            EnqueueMulticastSessionMessage(message, except);
            Flush();
        }

        protected void Flush()
        {
            SendAll();
        }

        private void SendAll()
        {
            while(messagesToSend.Count > 0)
            {
                var message = messagesToSend.Dequeue();
                Statistics.UploadBytes.Feed(message.Data.Length);

                if ( (message.Address == null || message.Address.Equals(session.Self.Address)) && InternalMessageBroker.HasListeners())
                {
                    InternalMessageBroker.Send(this, message);
                }
                //Send through socket
                else
                {
                    if (message.Address != null)
                        Socket.Send(message.Address, message.Data);
                    else
                        Socket.Send(message.Data);
                }
            }
        }

        private bool HandleAuthorization(IPEndPoint source, object message)
        {
            if (message is IAuthorizable)
            {
                var authorizable = message as IAuthorizable;

                if (authorizable.Client == null)
                    return false;

                //Sender address is assigned right here
                authorizable.Client.Address = source;

                //JoinRequests does not have any meaningful Client ID so it can't be authorized
                if (message is JoinRequestMessage || message is JoinRequestResponseMessage)
                    return true;

                var existingClient = session.Exists(authorizable.Client);
                if(existingClient != null)
                {
                    existingClient.UpdateTimestamp();
                    authorizable.Client = existingClient;
                    return true;
                }
            }

            return false; //Unhandled packets are ignored
        }

        private void HandleLiveRequest(LiveRequest message)
        {
            if (Socket.Type == UDPSocket.SocketType.Server)
                EnqueueMessage(new LiveResponse(session.Self), message.Client);
            else
                EnqueueMessage(new LiveResponse(session.Self));
        }

        private void HandleLiveResponse(LiveResponse message)
        {
            message.Client.UpdatePing();
            Statistics.Ping.Feed(message.Client.Ping);
        }


        private void SendLiveRequest(Client client)
        {
            client.UpdateLiveRequest();

            if(Socket.Type == UDPSocket.SocketType.Server)
                EnqueueMessage(new LiveRequest(session.Self), client);
            else
                EnqueueMessage(new LiveRequest(session.Self));
        }

        private void HandleClientTimeout()
        {
            var timeNow = DateTime.Now;
            var allInactiveClients = session.Clients.Where(client => (timeNow - client.LastUpdated).TotalSeconds >= MaxInactivity).ToArray();
            foreach (var inactiveClient in allInactiveClients)
                OnClientTimeout(inactiveClient);

            var suspectedInactive = session.Clients.Where(client => (timeNow - client.LastLiveRequest).TotalSeconds >= LiveRequestPeriod).ToArray();
            foreach (var suspect in suspectedInactive)
                SendLiveRequest(suspect);
        }

        protected abstract void ProcessMessage(object message);

        protected abstract void OnClientTimeout(Client client);

        public virtual void Update(float deltaTime)
        {
            timePassedSinceLastTick += deltaTime;

            var handledMessages = 0;
            while (Socket.HasMessage() && handledMessages < MaxMessagesInOneTick)
            {
                var rawMessage = Socket.DequeueMessage();
                Statistics.DownloadBytes.Feed(rawMessage.Data.Length);

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var message = MessageSerializer.DeserializeMessage(rawMessage.Data);
                stopwatch.Stop();
                Statistics.DerializationTime.Feed(stopwatch.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L)));

                if (!HandleAuthorization(rawMessage.Address, message))
                    return; //In case if message was not authorized, etc...

                if (message is LiveRequest)
                    HandleLiveRequest(message as LiveRequest);
                else if (message is LiveResponse)
                    HandleLiveResponse(message as LiveResponse);

                ProcessMessage(message);
                handledMessages++;
            }

            HandleClientTimeout();

            if (timePassedSinceLastTick < TickPeriod)
                return;

            timePassedSinceLastTick = 0.0f;

            SendAll();
        }

        public abstract void Start();
        public abstract void Stop();
    }
}
