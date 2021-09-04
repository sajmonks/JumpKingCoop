using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingCoop.Networking.Statistics
{
    public class NetworkStatistics
    {
        public StatisticalData DownloadBytes { get; set; } = new StatisticalData();
        public StatisticalData UploadBytes { get; set; } = new StatisticalData();
        public StatisticalData SerializationTime { get; set; } = new StatisticalData();
        public StatisticalData DerializationTime { get; set; } = new StatisticalData();
        public StatisticalData Ping { get; set; } = new StatisticalData();
        public int PacketsDiscarded { get; set; } = 0;
        public double SecondsSinceLastCleaned { get => (DateTime.Now - LastCleaned).TotalSeconds; }
        private DateTime LastCleaned { get; set; } = DateTime.Now;
        public void Clean()
        {
            LastCleaned = DateTime.Now;
            DownloadBytes.Clean();
            UploadBytes.Clean();
            SerializationTime.Clean();
            DerializationTime.Clean();
            Ping.Clean();
            PacketsDiscarded = 0;
        }
    }
}
