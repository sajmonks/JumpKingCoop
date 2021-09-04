using JumpKingCoop.Logs;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace JumpKingCoop.Networking
{
    public class RawMessage
    {
        public byte[] Data { get; set; }

        public IPEndPoint Address { get; set; }

        public RawMessage(byte[] data, IPEndPoint address)
        {
            Data = data;
            Address = address;
        }
    }


    public class UDPSocket
    {
        public enum SocketType
        {
            None,
            Client,
            Server
        }

        public int Port { get; private set; }

        public SocketType Type { get; private set; } = SocketType.None;

        public IPEndPoint RemoteEndPoint { get; private set;}

        public bool Connected 
        { 
            get
            {
                if ((socket != null && socket.Client != null))
                {
                    if (Type == SocketType.Server)
                        return socket.Client.IsBound;
                    else
                        return socket.Client.Connected;
                }
                else if (InternalMessageBroker.HasListeners())
                    return true;

                return false;
            }
        }

        protected const int MaxMessages = 30;

        protected readonly ConcurrentQueue<RawMessage> messageQueue = new ConcurrentQueue<RawMessage>();

        protected UdpClient socket = null;

        protected int messagesLost = 0;

        public UDPSocket(int port)
        {
            this.Port = port;
        }

        public void StartServer()
        {
            Type = SocketType.Server;
            RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            socket = new UdpClient(Port);
            socket.BeginReceive(new AsyncCallback(DoReceive), socket);

            Logger.Log(String.Format("Starting UDP socket as Server on port: {0}.", Port));
        }

        public void StartClient(string serverIPV4)
        {
            Type = SocketType.Client;
            RemoteEndPoint = new IPEndPoint(IPAddress.Parse(serverIPV4), Port);

            if (!InternalMessageBroker.HasListeners())
            {
                socket = new UdpClient();
                socket.Connect(RemoteEndPoint);
                socket.BeginReceive(new AsyncCallback(DoReceive), null);
                Logger.Log(String.Format("Starting UDP socket as Client {0}{1}.", serverIPV4, Port));
            }
            else
                Logger.Log("UDP socket for the Client was not started, the communication will be conducted through internal message brooker.");
        }

        public void Stop()
        {
            if(socket != null)
            {
                try
                {
                    socket.Close();
                }
                catch(Exception exception)
                {
                    Logger.Log(String.Format("Exception caught on attempting to close the socket:\n{0}", exception.Message));
                }
            }
            Logger.Log(String.Format("UDP socket for type {0} closed.", Type));
            Type = SocketType.None;
            RemoteEndPoint = null;
        }

        public int GetMessagesCount()
        {
            return messageQueue.Count;
        }

        public int GetMessagesLost()
        {
            return messagesLost;
        }

        public bool HasMessage()
        {
            return messageQueue.Count > 0 ? true : false;
        }

        public void EnqueueMessage(RawMessage rawMessage)
        {
            messageQueue.Enqueue(rawMessage);
        }

        public RawMessage DequeueMessage()
        {
            if (HasMessage())
            {
                RawMessage message = null;
                if (messageQueue.TryDequeue(out message))
                    return message;
            }
            return null;
        }

        private void DoReceive(IAsyncResult asyncResult)
        {
            try
            {
                //UdpClient socket = asyncResult.AsyncState as UdpClient;
                var source = new IPEndPoint(IPAddress.Any, 0);
                var buffer = socket.EndReceive(asyncResult, ref source);

                if (buffer.Length > 0)
                {
                    messageQueue.Enqueue(new RawMessage(buffer, source));

                    //It's a circular queue, it's size is defined by MaxMessages.
                    while (messageQueue.Count > MaxMessages)
                    {
                        RawMessage toDequeue = null;
                        if (messageQueue.TryDequeue(out toDequeue))
                            messagesLost++;
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Log(String.Format("Exception caught while receiving data from the socket:\n{0}", exception.Message));
            }

            try
            {
                socket.BeginReceive(new AsyncCallback(DoReceive), null);
            }
            catch (Exception exception)
            {
                Logger.Log(String.Format("Exception caught on BeginReceive:\n{0}", exception.Message));
            }
        }

        public void Send(byte[] message)
        {
            try
            {
                socket.Send(message, message.Length);
            }
            catch (Exception exception)
            {
                Logger.Log(String.Format("Exception caught on when sending the data to the socket:\n{0}", exception.Message));
            }
        }

        public void Send(string destinationIPV4, byte[] message)
        {
            try
            {
                socket.Send(message, message.Length, new IPEndPoint(IPAddress.Parse(destinationIPV4), Port));
            }
            catch (Exception exception)
            {
                Logger.Log(String.Format("Exception caught on when sending the data to the socket:\n{0}", exception.Message));
            }
        }

        public void Send(IPEndPoint destination, byte[] message)
        {
            try
            {
                socket.Send(message, message.Length, destination);
            }
            catch (Exception exception)
            {
                Logger.Log(String.Format("Exception caught on when sending the data to the socket:\n{0}", exception.Message));
            }
        }
    }
}
