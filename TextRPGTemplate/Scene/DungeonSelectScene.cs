using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TextRPG.Context;
using TextRPG.View;

namespace TextRPG.Scene
{
    public class DungeonSelectScene : AScene
    {
        private Random rnd = new Random();

        public DungeonSelectScene(GameContext gameContext, Dictionary<string, AView> viewMap,
                                SceneText sceneText, SceneNext sceneNext)
            : base(gameContext, viewMap, sceneText, sceneNext) { }

        public override void DrawScene()
        {
            ClearScene();
            List<string> dynamicText = new();
            for (int i = 0; i < gameContext.dungeonList.Count; i++)
            {
                var dungeon = gameContext.dungeonList[i];
                dynamicText.Add($"{i + 1}. {dungeon.Name} \t| 방어력 {dungeon.RecommendedDefense} 이상 권장");
            }

            ((DynamicView)viewMap[ViewID.Dynamic]).SetText(dynamicText.ToArray());
            Render();
        }

        public override string respond(int i)
        {
            if (i == 0)
            {
                convertSceneAnimationPlay(sceneNext.next![i]);
                return sceneNext.next![i];
            }

            if (i < 1 || i > gameContext.dungeonList.Count)
            {
                ((LogView)viewMap[ViewID.Log]).AddLog("잘못된 번호입니다.");
                return SceneID.DungeonSelect;
            }
            if (gameContext.ch.hp <= 0)
            {
                ((LogView)viewMap[ViewID.Log]).AddLog("HP가 0입니다. 휴식 후 다시 도전하세요!");
                return SceneID.DungeonSelect;
            }
            var selectedDungeon = gameContext.dungeonList[i - 1]; 
            gameContext.currentBattleMonsters = new List<MonsterData>();
            gameContext.currentBattleMonsters = GenerateMonstersForDungeon(selectedDungeon);
            ((LogView)viewMap[ViewID.Log]).AddLog($"생성된 몬스터 수: {gameContext.currentBattleMonsters.Count}");

            foreach (var m in gameContext.currentBattleMonsters)
            {
                ((LogView)viewMap[ViewID.Log]).AddLog($"- {m.Name} (HP: {m.HP}/{m.MaxHP})");
            }
            if (gameContext.currentBattleMonsters == null || gameContext.currentBattleMonsters.Count == 0)
            {
                ((LogView)viewMap[ViewID.Log]).AddLog("몬스터 생성 실패! 전투를 시작할 수 없습니다.");
                return SceneID.DungeonSelect;
            }
            ((LogView)viewMap[ViewID.Log]).AddLog($"선택된 던전 타입: {string.Join(",", selectedDungeon.MonsterTypes)}");
            ((LogView)viewMap[ViewID.Log]).AddLog($"전체 몬스터 타입 목록:");
            // 생성된 몬스터 로그 출력
            foreach (var m in gameContext.currentBattleMonsters)
            {
                ((LogView)viewMap[ViewID.Log]).AddLog($"[던전 입장] {m.Name} 등장 (Lv.{m.Level})");
            }

            if (gameContext.currentBattleMonsters.Count == 0)
            {
                ((LogView)viewMap[ViewID.Log]).AddLog("경고: 생성된 몬스터가 없습니다!");
                return SceneID.DungeonSelect;
            }

            gameContext.enteredDungeon = selectedDungeon;
            gameContext.prevHp = gameContext.ch.hp;
            gameContext.prevMp = gameContext.ch.Mp;
            gameContext.prevGold = gameContext.ch.gold;
            battleIdleAnimationPlay();
            return SceneID.BattleScene;
        }

        private List<MonsterData> GenerateMonstersForDungeon(DungeonData dungeon)
        {
            var monsters = new List<MonsterData>();
            int monsterCount = rnd.Next(dungeon.MonsterCountMin, dungeon.MonsterCountMax + 1);

            for (int i = 0; i < gameContext.ch.equipSkillList.Length; i++)
            {
                gameContext.ch.equipSkillList[i]?.Reset();
            }

            for (int i = 0; i < monsterCount; i++)
            {
                var monster = GenerateMonsterForDungeon(dungeon);
                if (monster != null)
                {
                    monsters.Add(monster);
                }
                else
                {
                    ((LogView)viewMap[ViewID.Log]).AddLog($"경고: {dungeon.Name}에서 몬스터 생성 실패");
                }
            }

            return monsters;
        }

        private MonsterData GenerateMonsterForDungeon(DungeonData dungeon)
        {
            // 대소문자 무시하고 공백 제거 후 비교
            var validMonsters = gameContext.monsterList
                .Where(m => m.Type.Any(mType =>
                    dungeon.MonsterTypes.Any(dType =>
                        string.Equals(
                            mType?.Trim(),
                            dType?.Trim(),
                            StringComparison.OrdinalIgnoreCase)))
                )
                .ToList();

            if (validMonsters.Count == 0)
            {
                ((LogView)viewMap[ViewID.Log]).AddLog($"경고: {dungeon.Name}에 적합한 몬스터 없음");
                ((LogView)viewMap[ViewID.Log]).AddLog($"던전 타입: {string.Join(",", dungeon.MonsterTypes)}");
                ((LogView)viewMap[ViewID.Log]).AddLog($"가능한 몬스터 타입: {string.Join(",",gameContext.monsterList.SelectMany(m => m.Type).Distinct())}");
                return null;
            }
            return WeightedRandomSelection(validMonsters)?.Clone();
        }

        private MonsterData WeightedRandomSelection(List<MonsterData> monsters)
        {
            if (monsters.Count == 0) return null;
            if (monsters.Count == 1) return monsters[0];

            // 가중치 계산 (레벨이 낮을수록 높은 가중치)
            var weights = monsters.Select(m => 1f / (m.Level + 1)).ToList();
            float totalWeight = weights.Sum();
            float randomPoint = (float)rnd.NextDouble() * totalWeight;

            for (int i = 0; i < monsters.Count; i++)
            {
                if (randomPoint < weights[i])
                {
                    return monsters[i];
                }
                randomPoint -= weights[i];
            }

            return monsters.Last();
        }
    }
}
