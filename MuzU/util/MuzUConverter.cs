using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuzU.util
{
    public class MuzUConverter
    {
        internal static int GetTimeSignatureNumerator(String str)
        {
            if (!int.TryParse(str.Split("/")[0], out int r)) return -1;
            return r;
        }

        internal static int GetTimeSignatureDenominator(String str)
        {
            if (!int.TryParse(str.Split("/")[1], out int r)) return -1;
            return r;
        }

        public static double GetBPM(long microsecondsPerQuarterNote, string timeSignature)
        {
            int n = GetTimeSignatureNumerator(timeSignature);
            int d = GetTimeSignatureDenominator(timeSignature);
            double microsecondsPerBeat = microsecondsPerQuarterNote * 4.0 / d;
            double minutesePerBeat = microsecondsPerBeat / 60000000;
            return 1.0 / minutesePerBeat;
        }

        public static long GetMicrosecondsPerQuarterNote(double bPM, string timeSignature)
        { 
            int n = GetTimeSignatureNumerator(timeSignature);
            int d = GetTimeSignatureDenominator(timeSignature);
            double minutesPerBeat = 1 / bPM;
            double microsecondsPerBeat = minutesPerBeat * 60000000;
            return (long)(microsecondsPerBeat * d / 4.0);
        }
    }
}
