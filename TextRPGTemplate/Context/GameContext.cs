using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextRPGTemplate.Animation;
using TextRPGTemplate.Context;

namespace TextRPG.Context
{
    [Serializable]
    public class GameContext
    {
        public Character ch { get; set; }
        public List<AfterJobStat>? afterJobStat { get; set; }
        public Shop shop { get; set; }
        public List<DungeonData> dungeonList { get; set; } = new List<DungeonData>();
        public List<MonsterData> currentBattleMonsters { get; set; } = new List<MonsterData>();
        public List<MonsterData> monsterList { get; set; } = new List<MonsterData>();
        public List<MonsterData>? clearedMonsters { get; set; }
        public DungeonData? enteredDungeon { get; set; } = null;
        public int prevHp { get; set; }
        public int prevMp { get; set; }
        public int curHp { get; set; }
        public int curMp { get; set; }
        public int prevGold { get; set; }
        public int curGold {  get; set; }
        public AnimationPlayer animationPlayer { get; set; }
        public QuestData[] questData { get; set; }
        public int questinput { get; set; }
        public bool isaccept { get; set; } = false;
        public Skill[] skillList { get; set; }

        public Dictionary<string, Animation?> animationMap = new();

        public List<BattleAnimationPos> battleAnimationPos { get; set; }
        public void ResetBattleMonsters()
        {
            currentBattleMonsters.Clear();
        }
        public GameContext(SaveData saveData, 
            List<DungeonData> dungeonData, 
            List<MonsterData> monsters, 
            AnimationPlayer animationPlayer, 
            Dictionary<string, Animation?> animationMap, 
            List<BattleAnimationPos> battleAnimationPos)
        {
            // Character 생성자는 변경되지 않았으므로 기존 코드 유지
            ch = new Character(saveData);

            // Shop 생성자도 변경되지 않았음
            shop = new Shop(new List<Item>(saveData.shopItems));

            this.dungeonList = new List<DungeonData>(dungeonData);
            this.animationPlayer = animationPlayer;
            this.animationMap = animationMap;
            this.monsterList = new List<MonsterData>(monsters);
            currentBattleMonsters = new List<MonsterData>();
            skillList = saveData.skillList.ToArray();
            this.animationMap = animationMap;
            this.questData = saveData.questData;
            this.afterJobStat = saveData.afterJobStat;
            this.battleAnimationPos = battleAnimationPos;
        }
    }
}