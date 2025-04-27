using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextRPG.Context
{
    [Serializable]
    public class DungeonData
    {
        public int Id { get; set; }  // JSON과 정확히 일치하도록 수정
        public string Name { get; set; } = "";
        public float RecommendedDefense { get; set; }  // 철자 수정
        public List<string> MonsterTypes { get; set; } = new();
        public int reward { get; set; }
        public int MonsterCountMin { get; set; }
        public int MonsterCountMax { get; set; }
        public string Description { get; set; } = "";


        public DungeonData() { }

        public DungeonData(int id, string name, float recommandArmor, int reward,
                  List<string> Type, int minMonsters = 1, int maxMonsters = 3)
        {
            this.Id = id;
            this.Name = name;
            this.RecommendedDefense = recommandArmor;
            this.reward = reward;
            this.MonsterTypes = Type;
            this.MonsterCountMin = minMonsters;
            this.MonsterCountMax = maxMonsters;
        }


    }
}
