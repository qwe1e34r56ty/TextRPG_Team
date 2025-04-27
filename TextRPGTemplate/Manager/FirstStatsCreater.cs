using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using TextRPG;
using TextRPG.Context;

namespace TextRPGTemplate.Managers
{
    public class FirstStatsCreater : CharacterBase
    {
        public Random rnd = new Random();
        public int Gold;
        public int MaxExp => 100 * (int)Math.Pow(1.2, Level - 1);
        public int Critical;
        public int statLimit = 200;


        public void ShuffleStats()
        {
            int[] stats = { Str, Int, Dex, Luk };
            for (int i = 0; i < stats.Length; i++)
            {
                int j = rnd.Next(i, stats.Length);
                int temp = stats[i];
                stats[i] = stats[j];
                stats[j] = temp;
            }

            Str = stats[0];
            Int = stats[1];
            Dex = stats[2];
            Luk = stats[3];
        }

        private void RandomStats()
        {
            int remaining = statLimit;
            Str = rnd.Next(0, remaining + 1);
            remaining -= Str;
            Int = rnd.Next(0, remaining + 1);
            remaining -= Int;
            Dex = rnd.Next(0, remaining + 1);
            remaining -= Dex;
            Luk = rnd.Next(0, remaining + 1);
            remaining -= Luk;

            ShuffleStats();
        }

        public SaveData CreateSaveData()
        {
            string defaultJson = File.ReadAllText(JsonPath.defaultDataJsonPath);
            SaveData saveData = JsonSerializer.Deserialize<SaveData>(defaultJson)!;
            ToSaveData(saveData, this.name);
            return saveData;
        }
        public void SaveAsDefault()
        {
            SaveData newDefault = CreateSaveData();
            string json = JsonSerializer.Serialize(newDefault, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(JsonPath.defaultDataJsonPath, json);
        }

        public FirstStatsCreater(string name,bool autoGenerate = true)
        {
            Level = 1;
            CurrentExp = 0;
            Point = 0;
            Gold = 20000;
            this.name = name; // 기본 이름 설정
            this.job = "";  // 기본 직업 설정

            if (autoGenerate)
            {
                RandomStats();
                Calculate();
            }
        }

        public void Calculate()
        {
            MaxHp = 50 + (Str * 2);
            MaxMp = 50 + (Int * 1);

            hp = MaxHp;
            Mp = MaxMp;

            attack = Str * 1.5f;  // 공격력 계산
            guard = Dex * 1.2f;   // 방어력 계산
        }
        public void GenerateSaveData()
        {
            string defaultJson = File.ReadAllText(JsonPath.defaultDataJsonPath);
            SaveData saveData = JsonSerializer.Deserialize<SaveData>(defaultJson)!;

            ToSaveData(saveData, this.name); // 핵심 리팩토링 포인트!

            File.WriteAllText(JsonPath.saveDataJsonPath,
                JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true }));
        }



        public void ToSaveData(SaveData saveData, string name = "")
        {

            if (!string.IsNullOrWhiteSpace(name))
                saveData.name = name;

            saveData.Str = this.Str;
            saveData.Int = this.Int;
            saveData.Dex = this.Dex;
            saveData.Luk = this.Luk;

            saveData.hp = this.hp;
            saveData.MaxHp = this.MaxHp;
            saveData.Mp = this.Mp;
            saveData.MaxMp = this.MaxMp;

            saveData.gold = this.Gold;
            //saveData.defaultAttack = this.attack;
            //saveData.defaultGuard = this.guard;
            saveData.Level = this.Level;
            saveData.CurrentExp = this.CurrentExp;
            saveData.Exp = this.Exp;
            saveData.Point = this.Point;
            saveData.critical = this.Critical;
            saveData.clearCount = 0; // 기본값 초기화
        }

        public void FirstStats()
        {


            Console.Clear();

            Calculate();
            hp = MaxHp;
            Mp = MaxMp;

            Console.WriteLine("=== 랜덤 캐릭터 스탯 생성 ===");
            Console.WriteLine($"레벨(Level): {Level}");
            Console.WriteLine($"힘(Str): {Str}");
            Console.WriteLine($"지능(Int): {Int}");
            Console.WriteLine($"민첩(Dex): {Dex}");
            Console.WriteLine($"운(Luk): {Luk}");
            Console.WriteLine($"총합: {Str + Int + Dex}/{statLimit}");
            Console.WriteLine($"체력(Hp): {hp}/{MaxHp}");
            Console.WriteLine($"마나(Mp): {Mp}/{MaxMp}");
            Console.WriteLine($"골드(Gold): {Gold}");
        }
        public void GenerateStats(bool showWindow = true) // 저기 나타낸 스텟값에서 아래 나타낼 창
        {
            while (true)
            {
                RandomStats();
                Calculate();

                if (showWindow)
                {
                    Console.Clear();
                    FirstStats();
                }

                Console.WriteLine("다시 생성하려면 Enter, 확정하시하려면 Q 입력");
                string? input = Console.ReadLine();

                if (input != null && input.ToLower() == "q")
                {
                    break;  // "Q" 입력시 루프 종료
                }

                // "Enter" 입력시 루프 계속
                else if (input == "")
                {
                    // 아무 것도 하지 않고 루프가 계속됩니다
                }
            }
        }
    }
}