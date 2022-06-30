using MuzU.data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        public static List<KeyValuePair<double, string>> ExtractWords(TimingSequence sequence, double startSec, double endSec)
        {
            List<double> times = sequence.TimingItems.Select(it => it.Time / µs).ToList();
            Regex regex = new Regex("[,.!?\"\\|/]");
            string lyrics = regex.Replace(sequence.Lyrics, "");
            string[] words = lyrics.Split(new char[] { '\t', '\n', '\r', ' ' });
            List<KeyValuePair<double, string>> twords = new List<KeyValuePair<double, string>>();
            int index = 1;
            for(int i = 0; i < words.Length && index<times.Count(); i++)
            {
                twords.Add(KeyValuePair.Create(times[index], words[i].Replace("$", "").ToLower()));
                index += words[i].Count(it => it == '$');
            }

            List<KeyValuePair<double, string>> result = new List<KeyValuePair<double, string>>();
            int startInd = twords.FindIndex(it => it.Key > startSec);
            int endInd = twords.FindLastIndex(it => it.Key< endSec);
            if (startInd == 0) result.Add(KeyValuePair.Create(startSec, twords[startInd].Value));
            else result.Add(KeyValuePair.Create(startSec, twords[startInd - 1].Value));
            for (int i = startInd; i <= endInd; i++)
            {
                result.Add(twords[i]);
            }
            //result.Add(KeyValuePair.Create(endSec, sequence.TimingItems[endInd].Values[propertyIndex]));
            return result;
        }

        public static List<KeyValuePair<double, string>> ExtractSyllables(TimingSequence sequence, double startSec, double endSec)
        {
            List<double> times = sequence.TimingItems.Select(it => it.Time / µs).ToList();
            Regex regex = new Regex("[,.!?\"\\|/]");
            string lyrics = regex.Replace(sequence.Lyrics, "");
            lyrics = Regex.Replace(lyrics, @"\s", "_");
            string[] syllables = lyrics.Split('$');
            List<KeyValuePair<double, string>> result = new List<KeyValuePair<double, string>>();
            int startInd = times.FindIndex(it => it > startSec);
            int endInd = times.FindLastIndex(it => it < endSec);
            if (startInd == 0) result.Add(KeyValuePair.Create(startSec, syllables[startInd]));
            else result.Add(KeyValuePair.Create(startSec, syllables[startInd - 1]));
            for(int i = startInd; i<= endInd && i < syllables.Length; i++)
            {
                var s = syllables[i];
                if(Regex.IsMatch(s, @"[a-zA-Z0-9]")) result.Add(KeyValuePair.Create(times[i], s));
            }
            return result;
        }
    }
} 
