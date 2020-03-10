using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Osu.Calculator
{
    public class BeatmapInfo
    {
        public List<TimingPoint> TimingPoints { get; private set; }
        public double ApproachRate { get; private set; }
        public double CircleSize { get; private set; }
        public double DrainRate { get; private set; }
        public double OverallDifficulty { get; private set; }
        public double StarRating { get; private set; }

        public BeatmapInfo()
        {
        }

        public void Compute()
        {
            double firstHitWindowsGreat = 0;
            double clockRate = 0;
            double baseDiffApproach = 0;


            double preempt = DifficultyRange(baseDiffApproach, 1800, 1200, 450) / clockRate;
            double hitWindowGreat = (int)(firstHitWindowsGreat / 2) / clockRate;
            
            
            OverallDifficulty = (80 - hitWindowGreat) / 6;
            ApproachRate = preempt > 1200 ? (1800 - preempt) / 120 : (1200 - preempt) / 150 + 5;
        }

        private double DifficultyRange(double diff, double min, double mid, double max)
        {
            if (diff > 5)
                return mid + (max - mid) * (diff - 5) / 5;
            else if (diff < 5)
                return mid - (mid - min) * (5 - diff) / 5;

            return mid;
        }
    }

    public struct TimingPoint
    {
    }
}
