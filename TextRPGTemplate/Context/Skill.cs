using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TextRPGTemplate.Context
{
    [Serializable]
    public class Skill
    {
        public string key { get; set; }
        public string? skillName { get; set; }
        public string? description { get; set; }
        public SkillType skillType { get; set; }        //스킬의 타입 : 공격,방어, 유틸 등 
        public StatType statType { get; set; }          //주스텟 : 스킬의 계수가 될 스텟
        public TargetType targetType { get; set; }      //스킬의 대상 : 자신,적

        public bool isLearn { get; set; }               //배운 스킬인지 배우지 못한 스킬인지 판정
        public bool isEquip { get; set; }               //스킬을 골라서 사용되야 한다면 장착된 스킬인지 판정

        public int maxUseCount { get; set; }            //한 던전에서 사용할 수 있는 최대 횟수
        public int curUseCount { get; set; }            //남은 스킬 사용 횟수
        public List<int> effectAmount { get; set; }     //스킬 효과 수치 : 공격 스킬이면 공격력, 방어 스킬이면 방어력, 회복스킬이면 회복량
        public float skillFactor { get; set; }          //스텟에 비례한 스킬 계수 ex)1.2f = 120%  ex2) effectAmount + (playerStat * skillFator) = skillDamage
        public int coolTime { get; set; }               //스킬의 쿨타임 : TextRpg 특성상 턴을 기준으로 잡아야해서 int 처리
        public int curCoolTime { get; set; }            //쿨타임 중인 스킬의 현재 쿨타임 : 쿨타임 중에는 스킬을 언제 다시 사용할 수 있게 되는지 판정
        public int costMana { get; set; }
        public List<int> duration { get; set; }               //스킬의 지속시간 : 공격 스킬이면 도트뎀, 버프 스킬이면 버프가 유지되는 시간
        public List<SecondaryEffect> secondaryEffects { get; set; } //스킬의 추가 효과, 여러개 부여가능

        public int skillLevel { get; set; }             //해당 스킬을 배우기 위한 레벨
        public int skillPoint { get; set; }             //해당 스킬을 배우기 위한 포인트
        public string[] flavorText { get; set; }        //스킬 사용시나오는 대사를 출력할 배열.

        public int equipSlot { get; set; }              //스킬이 장착된 슬롯 넘버

        [JsonConstructor]
        public Skill
            (
            string key, string? skillName, string? description, SkillType skillType, StatType statType, TargetType targetType,
            bool isLearn, bool isEquip,
            int maxUseCount, List<int> effectAmount, float skillFactor, int coolTime, int curCoolTime, int costMana, List<int> duration, List<SecondaryEffect> secondaryEffects,
            int skillLevel, int skillPoint,
            string[] flavorText,
            int equipSlot
            )
        {
            this.key = key;
            this.skillName = skillName;
            this.description = description;
            this.skillType = skillType;
            this.statType = statType;
            this.targetType = targetType;

            this.isLearn = isLearn;
            this.isEquip = isEquip;

            this.maxUseCount = maxUseCount;
            this.effectAmount = new List<int>(effectAmount);
            this.skillFactor = skillFactor;
            this.coolTime = coolTime;
            this.curCoolTime = curCoolTime;
            this.costMana = costMana;
            this.duration = new List<int>(duration);
            this.secondaryEffects = new List<SecondaryEffect>(secondaryEffects);

            this.skillLevel = skillLevel;
            this.skillPoint = skillPoint;
            this.flavorText = flavorText;
            this.equipSlot = equipSlot;
        }

        public void Reset()
        {
            curCoolTime = coolTime;
            curUseCount = maxUseCount;
        }

        public void EndTurn()
        {
            if (curCoolTime < coolTime)
            {
                curCoolTime++;
            }
        }
    }
    public enum SkillType
    {
        Attack,
        Defence,
        Utility
    }

    public enum TargetType
    {
        Self,
        Enemy,
    }

    public enum StatType 
    { 
        None,
        Str,
        Dex,
        Int,
        Luk
    }

    public enum SecondaryEffect
    {
        None,          // 없음
        Stun,          // 기절
        DoT,           // 지속 효과
        Curse,         // 스텟 감소
        Pierce,        // 방어 무시
        Overflow,      // 남은 데미지 다음 적에게
    }
}
