using JumpKingCoop.Logs;
using JumpKingCoop.Networking.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingCoop.Networking
{
    public static class InternalMessageBroker
    {
        private static List<ISocketProvider> Listeners { get; set; } = new List<ISocketProvider>();

        public static void AddListener<T>(T socketProvider) where T : ISocketProvider
        {
            Listeners.Add(socketProvider);
            Logger.Log(String.Format("Added recipient {0} to internal message broker. Current recipients: {1}.", typeof(T).Name, Listeners.Count));
        }

        public static bool HasListeners()
        {
            return Listeners.Count > 0;
        }

        public static void Clear()
        {
            Listeners.Clear();
            Logger.Log("Recipient list for internal message broker has been cleaned");
        }

        public static void Send(ISocketProvider source, RawMessage message)
        {
            foreach(var listener in Listeners)
            {
                if(listener != source)
                {
                    //Was sent from Client to Server
                    if (message.Address == null)
                        //From the client perspective RemoteEndPoint is actually the address of the Server,
                        //while the message.Address should have the address of the 'Source' meaning the client.
                        //But in this simple context the address of the server is the same as the client so we can RemoteEndPoint.
                        message.Address = source.Socket.RemoteEndPoint;


                    listener.Socket.EnqueueMessage(message);
                }
            }
        }
    }
}
