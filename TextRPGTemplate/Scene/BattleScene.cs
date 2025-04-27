using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using TextRPG.Context;
using TextRPG.View;
using TextRPGTemplate.Context;
using static System.Net.Mime.MediaTypeNames;

namespace TextRPG.Scene
{
    public class BattleScene : AScene
    {
        private Random rnd = new Random();
        private Character player;

        public BattleScene(GameContext gameContext, Dictionary<string, AView> viewMap,
                 SceneText sceneText, SceneNext sceneNext)
    :   base(gameContext, viewMap, sceneText, sceneNext)
        {

            // 2. 플레이어 참조
            this.player = gameContext.ch ?? throw new ArgumentNullException(nameof(gameContext.ch));

            // 3. 몬스터 리스트 검증
            if (this.gameContext.currentBattleMonsters!.Count == 0)
            {
                throw new InvalidOperationException(
                    $"던전 선택 후 몬스터가 생성되지 않았습니다. " +
                    $"DungeonSelectScene.respond()에서 몬스터를 생성해야 합니다.");
            }
        }

        public override void DrawScene()
        {
            ClearScene();
            List<string> dynamicText = new();
            foreach (var monster in gameContext.currentBattleMonsters!)
            {
                dynamicText.Add($"몬스터: {monster.Name} | HP: {monster.HP}/{monster.MaxHP}");
            }

            dynamicText.Add($"플레이어: {player.name} | HP: {player.hp}/{player.MaxHp} | MP: {player.Mp}/{player.MaxMp}");

            ((DynamicView)viewMap[ViewID.Dynamic]).SetText(dynamicText.ToArray());
            Render();
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
                        GoldReward = m.GoldReward,
                        Dropitem = m.Dropitem
                    })
                    .ToList();


                // 전투 몬스터 리스트 초기화

                ((LogView)viewMap[ViewID.Log]).AddLog("모든 몬스터를 처치했습니다!");
                //((LogView)viewMap[ViewID.Log]).ClearText();
                battleIdleAnimationPlay();
                return SceneID.DungeonClear;
            }

            // 플레이어 사망 경우
            if (player.hp <= 0)
            {
                battleIdleAnimationPlay();
                return SceneID.DungeonFail;
            }

            return null; // 전투 계속
        }
        public override string respond(int input)
        {
            var battleResult = CheckBattleEnd();
            if (battleResult != null) return battleResult;

            bool actionPerformed = true;

            switch (input)
            {
                case 1: actionPerformed = PerformPhysicalAttack(); break;
                case 2: actionPerformed = PerformMagicAttack(); break;
                case 3: return sceneNext.next![input];
                case 4: if (TryEscape()) return SceneID.DungeonSelect; break;
                case 5: actionPerformed = UsePotion(); break;
                case 8:
                    if (gameContext.ch.job != "")
                    {
                        battleSignatureAnimationPlay();
                        for (int i = 0; i < gameContext.currentBattleMonsters.Count; i++)
                        {
                            gameContext.currentBattleMonsters[i].HP = 0;
                        }
                        battleIdleAnimationPlay();
                    }
                    else
                    {
                        return SceneID.BattleScene;
                    }
                    break;
                default:
                    ((LogView)viewMap[ViewID.Log]).AddLog("잘못된 입력입니다. 다시 선택해주세요.");
                    Thread.Sleep(1000);
                    return SceneID.BattleScene;
            }

            if (!actionPerformed)
            {
                // 공격 대상 선택 취소 시 턴 유지
                return SceneID.BattleScene;
            }

            ApplySecondaryEffect();

            battleResult = CheckBattleEnd();
            if (battleResult != null) return battleResult;

            foreach (var monster in gameContext.currentBattleMonsters!.Where(m => m.HP > 0).ToList())
            {
                MonsterAttack(monster);

                battleResult = CheckBattleEnd();
                if (battleResult != null) return battleResult;
            }

            foreach (var skill in gameContext.ch.equipSkillList)
            {
                if (skill != null)
                {
                    skill.EndTurn();
                }
            }  
            return SceneID.BattleScene;
        }

        private bool PerformPhysicalAttack()
        {
            DrawScene();
            
            var target = ChooseTarget();
            if (target == null) return false;

            //int damage = (int)(player.getTotalAttack() +player.Str - target.Power);
            int damage = (int)player.defaultAttack + (int)(player.getPlusAttack()) + (int)player.getStat(player.statType)/3 - target.Power; ;
            if (damage < 0) damage = 0;

            DrawScene();

            battleAttackAnimationPlay(target);
            target.HP = Math.Max(0, target.HP - damage);
            ((LogView)viewMap[ViewID.Log]).AddLog($"{player.name}가 {target.Name}에게 물리 공격! {damage} 데미지!");


            if (target.HP <= 0)
            {
                ((LogView)viewMap[ViewID.Log]).AddLog($"{target.Name} 처치!");
            }
            battleIdleAnimationPlay();
            return true;
        }
 
        private bool PerformMagicAttack()
        {
            DrawScene();
            var target = ChooseTarget();
            if (target == null) return false;

            //int damage = (int)(player.getTotalAttack() * player.Int - target.Power); // 마법은 물리 공격보다 강하게 설정
            int damage = (int)player.defaultAttack + (int)(player.getPlusAttack()) + player.Int - target.Power; ;
            if (damage < 0) damage = 0;

            DrawScene();

            battleAttackAnimationPlay(target);
            target.HP = Math.Max(0, target.HP - damage);
            ((LogView)viewMap[ViewID.Log]).AddLog($"{player.name}가 {target.Name}에게 마법 공격! {damage} 데미지!");

            if (target.HP <= 0)
            {
                ((LogView)viewMap[ViewID.Log]).AddLog($"{target.Name} 처치!");
            }
            battleIdleAnimationPlay();
            return true;
        }

        private bool TryEscape()
        {
            int escapeChance = rnd.Next(100);
            if (escapeChance < 50)
            {
                ((LogView)viewMap[ViewID.Log]).AddLog("도망 실패!");
                return false;
            }
            else
            {
                // 도망 성공 시 몬스터 리스트 초기화
                battleRunAnimationPlay();
                ((LogView)viewMap[ViewID.Log]).AddLog("도망 성공!");
                return true;
            }
        }

        private bool UsePotion()
        {
            var potions = gameContext.shop.items
        .Where(i => i.isPotion && i.quantity > 0)
        .ToList();

            if (potions.Count == 0)
            {
                ((LogView)viewMap[ViewID.Log]).AddLog("사용할 수 있는 포션이 없습니다.");
                return false;
            }

            List<string> potionMenu = new List<string> { "사용할 포션을 선택하세요:" };
            for (int i = 0; i < potions.Count; i++)
            {
                potionMenu.Add($"{i + 1}. {potions[i].name} (남은 수량: {potions[i].quantity})");
            }
            potionMenu.Add("0. 취소");

            ((DynamicView)viewMap[ViewID.Dynamic]).SetText(potionMenu.ToArray());
            Render();

            int choice;
            while (true)
            {
                ((LogView)viewMap[ViewID.Log]).AddLog("선택: ");
                if (int.TryParse(Console.ReadLine(), out choice))
                {
                    if (choice == 0) return false;

                    if (choice >= 1 && choice <= potions.Count)
                    {
                        var selectedPotion = potions[choice - 1];
                        int healAmount = 0;
                        int manaAmount = 0;

                        if (selectedPotion.key == "HealPotion")
                        {
                            healAmount = 100;
                        }
                        else if (selectedPotion.key == "ManaPotion")
                        {
                            manaAmount = 100;
                        }

                        int beforeHp = player.hp;
                        int beforeMp = player.Mp;
                        player.hp = Math.Min(player.MaxHp, player.hp + healAmount);
                        player.Mp = Math.Min(player.MaxMp, player.Mp + manaAmount);

                        selectedPotion.quantity--;
                        ((LogView)viewMap[ViewID.Log]).AddLog($"{selectedPotion.name} 사용! {player.hp - beforeHp} 회복!");

                        if (selectedPotion.quantity <= 0)
                        {
                            gameContext.shop.items.Remove(selectedPotion);
                        }

                        return true;
                    }
                }

                ((LogView)viewMap[ViewID.Log]).AddLog("잘못된 입력입니다.");
            }
        }

        private void MonsterAttack(MonsterData monster)
        {
            ApplySecondaryEffect(monster);
            monster.isActionable = true; //턴 시작시 몬스터 상태를 true로 초기화

            if (monster.HP <= 0) return;


            if (!monster.isActionable)
            {
                ((LogView)viewMap[ViewID.Log]).AddLog($"{monster.Name}은 행동불능 상태!");
            }
            else
            {
                int damage = (int)(monster.Power - player.getTotalGuard());
                if (damage < 0) damage = 0;

                player.hp -= damage;

                if (player.hp < 0) player.hp = 0;

                ((LogView)viewMap[ViewID.Log]).AddLog($"{monster.Name}가 {player.name}에게 공격! {damage} 데미지!");

                if (player.hp <= 0)
                {
                    ((LogView)viewMap[ViewID.Log]).AddLog("플레이어가 쓰러졌습니다. 게임 오버!");
                }
            }
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
            ((LogView)viewMap[ViewID.Log]).AddLog("0. 취소");
            for (int i = 0; i < aliveMonsters.Count; i++)
            {
                ((LogView)viewMap[ViewID.Log]).AddLog($"{i + 1}. {aliveMonsters[i].Name} (HP: {aliveMonsters[i].HP}/{aliveMonsters[i].MaxHP})");
            }
            ((LogView)viewMap[ViewID.Log]).Update();
            ((LogView)viewMap[ViewID.Log]).Render();

            int choice;
            while (true)
            {
                ((InputView)viewMap[ViewID.Input]).SetCursor();
                if (int.TryParse(Console.ReadLine(), out choice))
                {
                    if (choice == 0)
                    {
                        ((LogView)viewMap[ViewID.Log]).AddLog("대상 선택이 취소되었습니다.");
                        return null;     // 호출한 쪽에서 판단하게
                    }
                    if (choice > 0 && choice <= aliveMonsters.Count)
                    {
                        Console.Clear();
                        return aliveMonsters[choice - 1];
                    }
                }

                ((InputView)viewMap[ViewID.Input]).SetCursor();
                ((LogView)viewMap[ViewID.Log]).AddLog("잘못된 선택입니다. 다시 입력하세요."); 
                Render();
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

        public void ApplySecondaryEffect()
        {
            for (int i = 0; i < player.StatusEffects?.Count; i++)
            {
                switch (player.StatusEffects[i].effectType)
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
                if (player.StatusEffects[i].duration == 0)
                {
                    player.StatusEffects.Remove(player.StatusEffects[i]);
                    i--;
                }
                else
                {
                    player.StatusEffects[i].duration--;
                }
            }
        }
    }
}
