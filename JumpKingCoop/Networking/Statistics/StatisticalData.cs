using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingCoop.Networking.Statistics
{
    public class StatisticalData
    {
        public double Min { get; private set; }
        public double Max { get; private set; }
        public int Samples { get; private set; }
        public double Sum { get; private set; }
        public double Average { get => Samples == 0 ? 0 : Sum / Samples; }

        public StatisticalData()
        {
            Clean();
        }

        public void Clean()
        {
            Min = float.MaxValue;
            Max = float.MinValue;
            Samples = 0;
            Sum = 0.0f;
        }

        public void Feed(double value)
        {
            Sum += value;
            Samples += 1;

            if (value < Min)
                Min = value;

            if (value > Max)
                Max = value;
        }

        public override string ToString()
        {
            return String.Format("Min: {0:0.0} Max: {1:0.0} Samples: {2:0.0} Sum: {3:0.0} Average: {4:0.0}", Min, Max, Samples, Sum, Average);
        }
    }
}
