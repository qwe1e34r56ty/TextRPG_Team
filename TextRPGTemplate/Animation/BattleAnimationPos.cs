using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextRPGTemplate.Animation
{
    [Serializable]
    public class BattleAnimationPos
    {
        public int monsterCount { get; set; } = 0;
        public int characterPosX { get; set; } = 0;
        public int characterPosY { get; set; } = 0;
        public int[] monsterPosX { get; set; } = Array.Empty<int>();
        public int[] monsterPosY { get; set; } = Array.Empty<int>();
    }
}
