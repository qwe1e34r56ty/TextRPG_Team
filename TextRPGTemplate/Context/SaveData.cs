using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextRPGTemplate.Animation;
using TextRPGTemplate.Context;
namespace TextRPG.Context
{
    [Serializable]
    public class SaveData : CharacterBase
    {

        public List<AfterJobStat>? afterJobStat {  get; set; }
        public Item[] items { get; set; } = Array.Empty<Item>();
        public Item[] shopItems { get; set; } = Array.Empty<Item>();
        public QuestData[] questData { get; set; } = Array.Empty<QuestData>();
        public List<Skill> learnSkillList { get; set; }
        public Skill[] skillList { get; set; }
        public List<Skill> characterSkillList { get; set; }
        public StatType stattype { get; set; } = StatType.Str;
        public SaveData() { }

        public SaveData(GameContext gameContext)
        {
            var ch = gameContext.ch;

            name = ch.name;
            job = ch.job;

            Level = ch.Level;
            Str = ch.Str;
            Dex = ch.Dex;
            Int = ch.Int;
            Luk = ch.Luk;
            stattype = ch.statType;

            attack = ch.defaultAttack;
            guard = ch.defaultGuard;
            hp = ch.hp;
            MaxHp = ch.MaxHp;
            Mp = ch.Mp;
            MaxMp = ch.MaxMp;
            Exp = ch.Exp;
            Point = ch.Point;
            CurrentExp = ch.CurrentExp;
            gold = ch.gold;

            critical = ch.critical;
            clearCount = ch.clearCount;

            items = ch.inventory.items!.ToArray();
            shopItems = gameContext.shop.items!.ToArray();
            questData = gameContext.questData;
            afterJobStat = gameContext.afterJobStat;

            skillList = gameContext.skillList.ToArray();
            characterSkillList = ch.characterSkillList.ToList();
        }
    }
}
