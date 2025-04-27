// Monster.cs
using System.Reflection.Emit;
using System.Xml.Linq;
using TextRPGTemplate.Context;

[Serializable]
public class MonsterData
{
    public string Name { get; set; } = "";
    public int Level { get; set; }
    public List<string> Type { get; set; } = new();  // 던전 타입 (예: ["숲", "동굴"])
    public int ExpReward { get; set; }
    public int GoldReward { get; set; }
    public int HP { get; set; }
    public int MaxHP { get; set; }
    public int Power { get; set; }
    public string Dropitem { get; set; }
    public List<StatusEffect> StatusEffects { get; set; }
    public bool isActionable = true;
    public string ID { get; set; } = "";
    public int preHP { get; set; }

    public MonsterData(MonsterData other)
    {
        this.ID = other.ID;
        this.Name = other.Name;  // other 객체의 값 복사
        this.Level = other.Level;
        this.Type = new List<string>(other.Type);  // List는 복사해야 함
        this.ExpReward = other.ExpReward;
        this.GoldReward = other.GoldReward;
        this.HP = other.HP;
        this.MaxHP = other.MaxHP;
        this.Power = other.Power;
        this.Dropitem = other.Dropitem;
        this.StatusEffects = other.StatusEffects;
    }

    public MonsterData()
    {
        MaxHP = HP;  // JSON에서 로드 시 HP 값으로 MaxHP 초기화
    }
    public MonsterData Clone()
    {
        return new MonsterData
        {
            ID = this.ID,
            Name = this.Name,
            Level = this.Level,
            HP = this.HP,
            MaxHP = this.MaxHP,
            Power = this.Power,
            ExpReward = this.ExpReward,
            GoldReward = this.GoldReward,
            Dropitem = this.Dropitem,
            Type = new List<string>(this.Type),
            StatusEffects = new List<StatusEffect>()
        };
    }
}

