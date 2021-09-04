using EntityComponent;
using JumpKingCoop.Logs;
using JumpKingCoop.Networking.Connection;
using JumpKingCoop.Networking.MessageHandlers;
using JumpKingCoop.Networking.Sessions;
using JumpKingCoop.Networking.Statistics;
using JumpKingCoop.Utils;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Serialization;

namespace JumpKingCoop.Networking
{
    public static class NetworkManager
    {
        private const int StatisticsFlushPeriod = 60;

        private static GameClient gameClient;
        private static GameServer gameServer;
        public static NetworkConfig Config { get; private set;}

        static NetworkManager()
        {
            Config = ReadConfig();
        }

        public static void Initialize()
        {
            if (Config.Enabled)
            {
                if (Config.Server)
                {
                    gameClient = new GameClient();
                    gameServer = new GameServer();

                    InternalMessageBroker.AddListener(gameServer);
                    InternalMessageBroker.AddListener(gameClient);

                    gameServer.Start();
                    gameClient.Start();

                }
                else
                {
                    gameClient = new GameClient();
                    gameClient.Start();
                }
            }
        }

        public static void Stop()
        {
            if (Config.Enabled)
            {
                InternalMessageBroker.Clear();
                gameClient?.Stop();
                gameServer?.Stop();
            }
        }

        public static void Update(float deltaTime)
        {
            gameServer?.Update(deltaTime);
            gameClient?.Update(deltaTime);

            LogServer();
            LogClient();
        }

        private static void LogClient()
        {
            if (gameClient == null || gameClient.Socket == null || !gameClient.Socket.Connected)
                return;

            if (gameClient.Statistics.SecondsSinceLastCleaned < StatisticsFlushPeriod)
                return;

            Logger.Log("=============================  Client statistics =============================");
            Logger.Log(String.Format("Data gathered in {0} seconds", gameClient.Statistics.SecondsSinceLastCleaned));
            Logger.Log(String.Format("Download bytes {0}", gameClient.Statistics.DownloadBytes));
            Logger.Log(String.Format("Upload bytes {0}", gameClient.Statistics.UploadBytes));
            Logger.Log(String.Format("Serialization time (us) {0}", gameClient.Statistics.SerializationTime));
            Logger.Log(String.Format("Deserialization time (us) {0}", gameClient.Statistics.DerializationTime));
            Logger.Log(String.Format("Ping (ms) {0}", gameClient.Statistics.Ping));
            Logger.Log("==============================================================================");
            gameClient.Statistics.Clean();
        }

        private static void LogServer()
        {
            if (gameServer == null || gameServer.Socket == null || !gameServer.Socket.Connected)
                return;

            if (gameServer.Statistics.SecondsSinceLastCleaned < StatisticsFlushPeriod)
                return;

            Logger.Log("=============================  Server statistics =============================");
            Logger.Log(String.Format("Data gathered in {0} seconds", gameServer.Statistics.SecondsSinceLastCleaned));
            Logger.Log(String.Format("Download bytes {0}", gameServer.Statistics.DownloadBytes));
            Logger.Log(String.Format("Upload bytes {0}", gameServer.Statistics.UploadBytes));
            Logger.Log(String.Format("Serialization time (us) {0}", gameServer.Statistics.SerializationTime));
            Logger.Log(String.Format("Deserialization time (us) {0}", gameServer.Statistics.DerializationTime));
            foreach(var client in gameServer.Session.Clients)
                Logger.Log(String.Format("Client {0} Ping {1}", client.ID, client.Ping));
            Logger.Log("==============================================================================");
            gameServer.Statistics.Clean();
        }

        private static NetworkConfig ReadConfig()
        {
            var configText = string.Empty;

            if (Directory.Exists("Content/mods"))
                configText = File.ReadAllText("Content/mods/JumpKingCoop.xml");
            else
                configText = File.ReadAllText("JumpKingCoop.xml");

            var serializer = new XmlSerializer(typeof(NetworkConfig));
            var streamReader = new StringReader(configText);
            var result = serializer.Deserialize(streamReader);
            streamReader.Close();

            var config = result as NetworkConfig;

            if(config.Nickname.Length >= 16)
                config.Nickname = config.Nickname.Substring(0, 15);

            if (config.SessionPassword.Length >= 32)
                config.SessionPassword = config.SessionPassword.Substring(0, 32);

            if (config.SessionPassword == null)
                config.SessionPassword = string.Empty;

            config.SessionPassword = CreateMD5(config.SessionPassword);

            config.EndPoint = new IPEndPoint(IPAddress.Parse(config.IpAddress), config.Port);

            return result as NetworkConfig;
        }

        public static string CreateMD5(string input)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.Unicode.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                    stringBuilder.Append(hashBytes[i].ToString("X2"));

                return stringBuilder.ToString();
            }
        }
    }
}
