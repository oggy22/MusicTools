using System;
using MusicCore;
using System.Collections.Generic;
using System.Linq;

namespace MusicComposer
{
    public class RhythmMaker
    {
        static public void FindTwoOfSameLength(RhythmPatternBase[] patterns, out RhythmPatternBase pattern1, out RhythmPatternBase pattern2)
        {
            RhythmPatternBase p1, p2;
            do
            {
                p1 = patterns[rand.Next(patterns.Length)];
            } while (patterns.Count(p => (p.Length == p1.Length)) < 2);

            do
            {
                p2 = patterns[rand.Next(patterns.Length)];
                if (p1 != p2 && p1.Length == p2.Length)
                    break;
            } while (true);
            pattern1 = p1;
            pattern2 = p2;
        }

        static Random rand = new Random();

        static public RhythmPatternBase CreateRhytmPattern(int beatsPerMeasure=3, int minLenght = 15)
        {
            HashSet<RhythmPatternBase> patterns = new HashSet<RhythmPatternBase>();

            for (int i = 0; i < 5; i++)
                patterns.Add(new RhythmPattern(beatsPerMeasure, 2, rand));

            while (true)
            {
                string st = RhytmPatternComposite.sts[rand.Next(RhytmPatternComposite.sts.Length)];
                RhythmPatternBase pattern1, pattern2;
                FindTwoOfSameLength(patterns.ToArray(), out pattern1, out pattern2);
                RhythmPatternBase patternNew = new RhytmPatternComposite(st, pattern1, pattern2);
                Console.WriteLine($"{st} = {patternNew.MeasuresCount}");
                if (patternNew.MeasuresCount >= minLenght)
                    return patternNew;
                patterns.Add(patternNew);
            }
        }
    }
}
