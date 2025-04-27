using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextRPGTemplate.Context
{
    public class StatusEffect
    {
        public Skill skill;
        public StatusEffectType effectType;
        public int duration;
        public float effectAmount;

        public StatusEffect(Skill skill, StatusEffectType type, int duration, float effectAmount)
        {
            this.skill = skill;
            this.effectType = type;
            this.duration = duration;
            this.effectAmount = effectAmount;
        }
    }
}
public enum StatusEffectType
{
    None,
    Stun,
    DoT,
    Curse,
    Buff
}