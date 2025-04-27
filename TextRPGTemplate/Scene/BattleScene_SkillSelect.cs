using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TextRPG.Context;
using TextRPG.Scene;
using TextRPG.View;
using TextRPGTemplate.Context;
using static System.Net.Mime.MediaTypeNames;

namespace TextRPGTemplate.Scene
{
    internal class BattleScene_SkillSelect : AScene
    {
        public BattleScene_SkillSelect(GameContext gameContext, Dictionary<string, AView> viewMap, SceneText sceneText, SceneNext sceneNext) : base(gameContext, viewMap, sceneText, sceneNext)
        {

        }

       
        public override void DrawScene()
        {
            ClearScene();
            List<string> dynamicText = new();

            dynamicText.Add("[보유 스킬]");
            dynamicText.Add("");

            Skill equipSKill;
            for (int i = 0; i < gameContext.ch.equipSkillList.Length; i++)
            {
                if (gameContext.ch.equipSkillList[i] != null)
                {
                    equipSKill = gameContext.ch.equipSkillList[i];
                    int skillDamage = (int)((gameContext.ch.getTotalAttack() + equipSKill.effectAmount[0]) + (gameContext.ch.getStat(equipSKill.statType) * equipSKill.skillFactor));

                    dynamicText.Add($"[{i + 1}] {equipSKill.skillName} | {(skillDamage)} 데미지 | 소모 마나 : {equipSKill.costMana}");
                    dynamicText.Add($"    쿨타임 : {equipSKill.curCoolTime}/{equipSKill.coolTime} | 횟수 : {equipSKill.curUseCount}/{equipSKill.maxUseCount}");
                }
            }

            //dynamicText.Add($"{gameContext.skillList[0].skillName}");

            ((DynamicView)viewMap[ViewID.Dynamic]).SetText(dynamicText.ToArray());
            Render();
        }


        //기능
        public override string respond(int i)
        {
            if (i > 0 && i <= gameContext.ch.equipSkillList.Length)
            {
                if (gameContext.ch.equipSkillList[i-1] != null)
                {
                   return Battle(gameContext.ch.equipSkillList[i - 1]);
                }
                else
                {
                    ((LogView)viewMap[ViewID.Log]).AddLog($"잘못된 입력입니다.");
                    return SceneID.Nothing;
                }
            }
            else if(i < 0 || i > gameContext.ch.equipSkillList.Length)
            {
                ((LogView)viewMap[ViewID.Log]).AddLog($"잘못된 입력입니다.");
                return SceneID.Nothing;
            }
            return SceneID.BattleScene;
        }

        public string Battle(Skill selectSkill)
        {
            if (IsUseable(selectSkill))
            {
                ApplySecondaryEffect();

                UseSkill(selectSkill);
               
                foreach (var monster in gameContext.currentBattleMonsters!.Where(m => m.HP > 0).ToList())
                {
                    MonsterAttack(monster);
                }

                string battleResult = CheckBattleEnd();
                if (battleResult != null) return battleResult;

                foreach (var skill in gameContext.ch.equipSkillList)
                {
                    if (skill != null)
                    {
                        skill.EndTurn();
                    }
                }

                return SceneID.BattleScene;
            }
            else
            {
                return SceneID.Nothing;
            }
        }

        private string? CheckBattleEnd()
        {
            // 모든 몬스터가 죽은 경우
            if (gameContext.currentBattleMonsters!.All(m => m.HP <= 0))
            {
                // 보상 계산 (죽은 몬스터만)
                var deadMonsters = gameContext.currentBattleMonsters!.Where(m => m.HP <= 0).ToList();

                gameContext.clearedMonsters = deadMonsters
                    .Select(m => new MonsterData
                    {
                        Name = m.Name,
                        ExpReward = m.ExpReward,
                        GoldReward = m.GoldReward
                    })
                    .ToList();


                // 전투 몬스터 리스트 초기화

                ((LogView)viewMap[ViewID.Log]).AddLog("모든 몬스터를 처치했습니다!");
                //((LogView)viewMap[ViewID.Log]).ClearText();

                return SceneID.DungeonClear;
            }

            // 플레이어 사망 경우
            if (gameContext.ch.hp <= 0)
            {
                battleIdleAnimationPlay();
                return SceneID.DungeonFail;
            }

            return null; // 전투 계속
        }

        public bool IsUseable(Skill selectSkill)
        {
            if (selectSkill.costMana > gameContext.ch.Mp)
            {
                ((LogView)viewMap[ViewID.Log]).AddLog($"마나가 부족합니다.");
                return false;
            }
            else if (selectSkill.curUseCount == 0)
            {
                ((LogView)viewMap[ViewID.Log]).AddLog($"스킬을 모두 사용하였습니다.");
                return false;
            }
            else if (selectSkill.curCoolTime < selectSkill.coolTime)
            {
                ((LogView)viewMap[ViewID.Log]).AddLog($"쿨타임 입니다.");
                return false;
            }
            else
            {
                return true;
            }
        }

        public void UseSkill(Skill selectSkill)
        {
            if (selectSkill.targetType == TargetType.Enemy)
            {
                ExecutorToEnemy(selectSkill);
            }
            else if(selectSkill.targetType == TargetType.Self)
            {
                ExecutorToSelf(selectSkill);
            }
        }

        private void MonsterAttack(MonsterData monster)
        {
            monster.isActionable = true; //턴 시작시 몬스터 상태를 true로 초기화

            ApplySecondaryEffect(monster);

            if (monster.HP <= 0) return;

            if (!monster.isActionable)
            {
                ((LogView)viewMap[ViewID.Log]).AddLog($"{monster.Name}은 행동불능 상태!");
            }
            else
            {
                int damage = (int)(monster.Power - gameContext.ch.getTotalGuard());
                if (damage < 0) damage = 0;

                gameContext.ch.hp -= damage;

                if (gameContext.ch.hp < 0) gameContext.ch.hp = 0;

                ((LogView)viewMap[ViewID.Log]).AddLog($"{monster.Name}가 {gameContext.ch.name}에게 공격! {damage} 데미지!");

                if (gameContext.ch.hp <= 0)
                {
                    ((LogView)viewMap[ViewID.Log]).AddLog("플레이어가 쓰러졌습니다. 게임 오버!");
                }
                battleIdleAnimationPlay();
            }
        }

        public void ApplySecondaryEffect()
        {
            for (int i = 0; i < gameContext.ch.StatusEffects?.Count; i++)
            {
                switch (gameContext.ch.StatusEffects[i].effectType)
                {
                    case StatusEffectType.Stun:
                        //player.isActionable = false;
                        break;
                    case StatusEffectType.DoT:
                        /*
                        monster.HP = Math.Max(0, monster.HP - (int)(monster.StatusEffects[i].effectAmount));
                        ((LogView)viewMap[ViewID.Log]).AddLog($"{monster.Name}에게 상태 이상 발생! {monster.StatusEffects[i].effectAmount}의 데미지 !");
                        if (monster.HP <= 0)
                        {
                            ((LogView)viewMap[ViewID.Log]).AddLog($"{monster.Name} 처치!");
                        }
                        */
                        break;
                }
                if (gameContext.ch.StatusEffects[i].duration == 0)
                {
                    gameContext.ch.StatusEffects.Remove(gameContext.ch.StatusEffects[i]);
                    i--;
                }
                else
                {
                    gameContext.ch.StatusEffects[i].duration--;
                }
            }
        }

        public void ApplySecondaryEffect(MonsterData monster)
        {
            for (int i = 0; i < monster.StatusEffects?.Count; i++)
            {
                switch (monster.StatusEffects[i].effectType)
                {
                    case StatusEffectType.Stun:
                        monster.isActionable = false;
                        break;
                    case StatusEffectType.DoT:
                        monster.HP = Math.Max(0, monster.HP - (int)(monster.StatusEffects[i].effectAmount));
                        ((LogView)viewMap[ViewID.Log]).AddLog($"{monster.Name}에게 상태 이상 발생! {monster.StatusEffects[i].effectAmount}의 데미지 !");
                        if (monster.HP <= 0)
                        {
                            ((LogView)viewMap[ViewID.Log]).AddLog($"{monster.Name} 처치!");
                        }
                        break;
                }
                if (monster.StatusEffects[i].duration == 0)
                {
                    monster.StatusEffects.Remove(monster.StatusEffects[i]);
                    i--;
                }
                else
                {
                    monster.StatusEffects[i].duration--;
                }
            }
        }

        public StatusEffectType ConvertEffect(SecondaryEffect secondaryEffect)
        {
            StatusEffectType type;
            switch (secondaryEffect)
            {
                case SecondaryEffect.Stun:
                    type = StatusEffectType.Stun;
                    break;
                case SecondaryEffect.DoT:
                    type = StatusEffectType.DoT;
                    break;
                case SecondaryEffect.Curse:
                    type = StatusEffectType.Curse;
                    break;
                default:
                    type = StatusEffectType.None;
                    break;
            }
            return type;
        }

        private MonsterData? ChooseTarget()
        {
            var aliveMonsters = gameContext.currentBattleMonsters!
                .Where(m => m.HP > 0)
                .ToList();

            if (aliveMonsters.Count == 0)
            {

                ((LogView)viewMap[ViewID.Log]).AddLog("공격할 수 있는 몬스터가 없습니다.");
                return null;
            }
           ((LogView)viewMap[ViewID.Log]).AddLog("어떤 몬스터를 공격하시겠습니까?");
            for (int i = 0; i < aliveMonsters.Count; i++)
            {
                ((LogView)viewMap[ViewID.Log]).AddLog($"{i + 1}. {aliveMonsters[i].Name} (HP: {aliveMonsters[i].HP}/{aliveMonsters[i].MaxHP})");
            }
           ((LogView)viewMap[ViewID.Log]).Update();
            ((LogView)viewMap[ViewID.Log]).Render();

            int choice;
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out choice) && choice > 0 && choice <= aliveMonsters.Count)
                {
                    Console.Clear(); // 추가: 화면 정리
                    return aliveMonsters[choice - 1];
                }

                ((InputView)viewMap[ViewID.Input]).SetCursor();
                ((LogView)viewMap[ViewID.Log]).AddLog("잘못된 선택입니다. 다시 입력하세요.");
                Console.ReadLine(); // 잘못된 입력 소비
                ((InputView)viewMap[ViewID.Input]).SetCursor();
            }
           ((LogView)viewMap[ViewID.Log]).Update();
            ((LogView)viewMap[ViewID.Log]).Render();
        }
        public void ExecutorToEnemy(Skill selectSkill)
        {
            MonsterData target = ChooseTarget();

            if (target == null) return;

            DrawScene();

            battleAttackAnimationPlay(target);
            selectSkill.curCoolTime = 0;
            selectSkill.curUseCount--;
            gameContext.ch.Mp -= selectSkill.costMana;

            int skillDamage = (int)((gameContext.ch.getTotalAttack() + selectSkill.effectAmount[0]) + (gameContext.ch.getTotalStat(selectSkill.statType) * selectSkill.skillFactor));

            int damage = (skillDamage - target.Power);
            if (damage < 0) damage = 0;

            target.preHP = target.HP;
            target.HP = Math.Max(0, target.HP - damage);
            ((LogView)viewMap[ViewID.Log]).AddLog($"{gameContext.ch.name}가 {target.Name}에게 {selectSkill.skillName}! {damage} 데미지!");

            if (target.HP <= 0)
            {
                ((LogView)viewMap[ViewID.Log]).AddLog($"{target.Name} 처치!");

                for (int i = 0; i < selectSkill.secondaryEffects.Count; i++)
                {
                    if (selectSkill.secondaryEffects[i] == SecondaryEffect.Overflow)
                    {
                        int flowDamage = damage - target.preHP;
                        ApplyOverflow(target, flowDamage);
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < selectSkill.secondaryEffects.Count; i++)
                {
                    if (selectSkill.secondaryEffects[i] != SecondaryEffect.None)
                    {
                        target.StatusEffects.Add(new StatusEffect(selectSkill, ConvertEffect(selectSkill.secondaryEffects[i]), selectSkill.duration[i], selectSkill.effectAmount[i+1]));
                    }
                    else if (selectSkill.secondaryEffects[i] == SecondaryEffect.Pierce)
                    {

                    }
                }
            }
        }

        public void ApplyOverflow(MonsterData target, int flowDamage)
        {
            for (int j = 0; j < gameContext.currentBattleMonsters.Count; j++)
            {
                if (gameContext.currentBattleMonsters[j].HP > 0)
                {
                    gameContext.currentBattleMonsters[j].HP = Math.Max(0, gameContext.currentBattleMonsters[j].HP - flowDamage);
                    ((LogView)viewMap[ViewID.Log]).AddLog($"{target.Name}에게 오버플로우!");
                    break;
                }
            }
        }

        private void ExecutorToSelf(Skill selectSkill)
        {
            gameContext.ch.StatusEffects.Add(new StatusEffect(selectSkill, StatusEffectType.Buff, selectSkill.duration[0], selectSkill.effectAmount[0]));
        }
    }
}
