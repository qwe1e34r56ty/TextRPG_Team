using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextRPGTemplate.Context;

namespace TextRPG.Context
{
    [Serializable]
    public class JobRequirement
    {
        public string? JobName { get; set; }
        public int MinLevel { get; set; }
        public int MinStr { get; set; }
        public int MinInt { get; set; }
        public int MinDex { get; set; }
        public int MinLuk { get; set; }
        public string Message => $"{JobName} 전직 가능";
        public bool IsEligible(Character ch)
        {
            return ch.Level >= MinLevel &&
                   ch.Str >= MinStr &&
                   ch.Int >= MinInt &&
                   ch.Dex >= MinDex &&
                   ch.Luk >= MinLuk;
        }
    }

    public class AfterJobStat
    {
        public int iD { get; set; }
        public string? job { get; set; }
        public int? addStr { get; set; }
        public int? addDex { get; set; }
        public int? addInt { get; set; }
        public int? addLuk { get; set; }

        public int? addHp { get; set; }
        public int? addMp { get; set; }
        public int? addPoint { get; set; }
        public int? addattack { get; set; }
        public int? addguard { get; set; }
        public int? addcritical { get; set; }
        public List<string>? jobSkills { get; set; }
        public StatType stattype { get; set; }
    }

} 