using MuzU.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuzU.util
{
    public class MuzUExtractor
    {
        private static double µs = 1000000.0;

        public static List<KeyValuePair<double, double>> Extract(TimingSequence sequence, int propertyIndex, double startSec, double endSec)
        {
            long startμs = (long)(startSec * µs);
            long endμs = (long)(endSec * µs);
            List<KeyValuePair<double, double>> result = new List<KeyValuePair<double, double>>();
            int startInd = sequence.TimingItems.FindIndex(it => it.Time > startμs);
            int endInd = sequence.TimingItems.FindLastIndex(it => it.Time < endμs);
            if (startInd == 0) result.Add(KeyValuePair.Create(startSec, sequence.TimingItems[startInd].Values[propertyIndex]));
            else result.Add(KeyValuePair.Create(startSec, sequence.TimingItems[startInd - 1].Values[propertyIndex]));
            for(int i = startInd; i <= endInd; i++)
            {
                result.Add(KeyValuePair.Create(sequence.TimingItems[i].Time/µs, sequence.TimingItems[i].Values[propertyIndex]));
            }
            //result.Add(KeyValuePair.Create(endSec, sequence.TimingItems[endInd].Values[propertyIndex]));
            return result;
        }   
    }
}
