using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextRPG.Context;
using TextRPG.View;
using TextRPGTemplate.Animation;

namespace TextRPG.Scene
{
    public static class SceneID
    {
        public const string Main = "Main";
        public const string Status = "Status";
        public const string Inventory = "Inventory";
        public const string Wear = "Wear";
        public const string Shop = "Shop";
        public const string Nothing = "None";
        public const string Buy = "Buy";
        public const string Rest = "Rest";
        public const string Sell = "Sell";
        public const string DungeonSelect = "DungeonSelect";
        public const string BattleScene = "BattleScene";
        public const string BattleScene_Skill = "BattleScene_Skill";
        public const string DungeonClear = "DungeonClear";
        public const string DungeonFail = "DungeonFail";
        public const string StatUp = "StatUp";
        public const string NPCScene = "NPCScene";
        public const string QuestScene = "QuestScene";
        public const string QuestClearScene = "QuestClearScene";
        public const string GetJob = "GetJob";
        public const string SkillManager = "SkillManager";
        public const string SkillLearn = "SkillLearn";
        public const string SkillEquip = "SkillEquip";

    }
    public abstract class AScene
    {
        protected GameContext gameContext { get; set; }
        protected Dictionary<string, AView> viewMap { get; set; }
        protected SceneText sceneText { get; set; }
        protected SceneNext sceneNext { get; set; }
        Dictionary<int, Func<GameContext, Animation[]>> makeIdleAnimations = new Dictionary<int, Func<GameContext, Animation[]>>();
        Dictionary<int, Func<GameContext, Animation[]>> makeAttackAnimations = new Dictionary<int, Func<GameContext, Animation[]>>();
        public AScene(GameContext gameContext, Dictionary<string, AView> viewMap, SceneText sceneText, SceneNext sceneNext)
        {
            this.gameContext = gameContext;
            this.viewMap = viewMap;
            this.sceneText = sceneText;
            this.sceneNext = sceneNext;
        }
        public void Render()
        {
            foreach (var pair in viewMap)
            {
                pair.Value.Update();
                pair.Value.Render();
            }
            ((InputView)viewMap[ViewID.Input]).SetCursor();
        }
        public virtual void DrawScene()
        {
            ((ScriptView)viewMap[ViewID.Script]).SetText(sceneText.scriptText!);
            ((ChoiceView)viewMap[ViewID.Choice]).SetText(sceneText.choiceText!);
            ((DynamicView)viewMap[ViewID.Dynamic]).SetText(System.Array.Empty<string>());
            foreach (var pair in viewMap)
            {
                pair.Value.Update();
                pair.Value.Render();
            }
            ((InputView)viewMap[ViewID.Input]).SetCursor();
        }
        public abstract string respond(int i);
        public void ClearScene()
        {
            ((ScriptView)viewMap[ViewID.Script]).SetText(sceneText.scriptText!);
            ((ChoiceView)viewMap[ViewID.Choice]).SetText(sceneText.choiceText!);
            ((DynamicView)viewMap[ViewID.Dynamic]).SetText(System.Array.Empty<string>());
            //((SpriteView)viewMap[ViewID.Sprite]).SetText(System.Array.Empty<string>());

            foreach (var pair in viewMap)
            {
                pair.Value.Update();
                pair.Value.Render();
            }
            ((InputView)viewMap[ViewID.Input]).SetCursor();
        }
        public void convertSceneAnimationPlay(int i)
        {
            if (gameContext.animationMap.ContainsKey(sceneNext.next![i]))
            {
                Animation?[] animations = { gameContext.animationMap[sceneNext.next![i]] };
                gameContext.animationPlayer.play(animations, (SpriteView)viewMap[ViewID.Sprite]);
            }
        }

        public void convertSceneAnimationPlay(string s)
        {
            if (gameContext.animationMap.ContainsKey(s))
            {
                if (s != SceneID.Status)
                {
                    Animation?[] animations = { gameContext.animationMap[s] };
                    gameContext.animationPlayer.play(animations, (SpriteView)viewMap[ViewID.Sprite]);
                }
                else
                {
                    statusAnimationPlay();
                }

            }
        }

        public void statusAnimationPlay()
        {
            List<Animation> animationsList = new List<Animation>();
            Animation[] animationsArray = animationsList.ToArray();
            Animation animation = gameContext.animationMap[$"{gameContext.ch.job}Status"]!.DeepCopy();
            animationsList.Add(animation);
            animationsArray = animationsList.ToArray();
            gameContext.animationPlayer.play(animationsArray, (SpriteView)viewMap[ViewID.Sprite]);
        }

        public void battleIdleAnimationPlay()
        {
            List<Animation> animationsList = new List<Animation>();
            Animation[] animationsArray = animationsList.ToArray();
            BattleAnimationPos battleAnimationPos = gameContext.battleAnimationPos[gameContext.currentBattleMonsters.Count - 1];

            Animation animation;

            for (int i = 0; i < gameContext.currentBattleMonsters.Count; i++)
            {
                if (gameContext.currentBattleMonsters[i].HP > 0)
                {
                    animation = gameContext.animationMap[$"{gameContext.currentBattleMonsters[i].ID}Idle"]!.DeepCopy();
                }
                else
                {
                    animation = gameContext.animationMap["MonsterDie"]!.DeepCopy();
                    
                }
                animation.x[0] += battleAnimationPos.monsterPosX[i];
                animation.y[0] += battleAnimationPos.monsterPosY[i];
                animation.frames = animation.frames[0..1];
                animationsList.Add(animation);
            }
            if (gameContext.ch.hp > 0)
            {
                animation = gameContext.animationMap[$"{gameContext.ch.job}Idle"]!.DeepCopy();
            }
            else
            {
                animation = gameContext.animationMap["CharacterDie"]!.DeepCopy();
            }
            animation.x[0] += battleAnimationPos.characterPosX;
            animation.y[0] += battleAnimationPos.characterPosY;
            animationsList.Add(animation);

            animationsArray = animationsList.ToArray();

            gameContext.animationPlayer.play(animationsArray, (SpriteView)viewMap[ViewID.Sprite]);
        }

        public void battleRunAnimationPlay()
        {
            List<Animation> animationsList = new List<Animation>();
            Animation[] animationsArray = animationsList.ToArray();
            BattleAnimationPos battleAnimationPos = gameContext.battleAnimationPos[gameContext.currentBattleMonsters.Count - 1];

            Animation animation = gameContext.animationMap[$"{gameContext.ch.job}Idle"]!.DeepCopy();
            animation.x[0] += battleAnimationPos.characterPosX;
            animation.y[0] += battleAnimationPos.characterPosY;
            animationsList.Add(animation);
            animationsArray = animationsList.ToArray();

            gameContext.animationPlayer.play(animationsArray, (SpriteView)viewMap[ViewID.Sprite]);
        }

        public void battleSignatureAnimationPlay()
        {
            List<Animation> animationsList = new List<Animation>();
            Animation[] animationsArray = animationsList.ToArray();
            BattleAnimationPos battleAnimationPos = gameContext.battleAnimationPos[gameContext.currentBattleMonsters.Count - 1];

            if (gameContext.animationMap.ContainsKey($"{gameContext.ch.job}Signature")){
                Animation animation = gameContext.animationMap[$"{gameContext.ch.job}Signature"]!.DeepCopy();
                animation.x[0] += battleAnimationPos.characterPosX;
                animation.y[0] += battleAnimationPos.characterPosY;
                animationsList.Add(animation);
                animationsArray = animationsList.ToArray();

                gameContext.animationPlayer.play(animationsArray, (SpriteView)viewMap[ViewID.Sprite]);
            }
        }

        public void battleAttackAnimationPlay(MonsterData target)
        {
            List<Animation> animationsList = new List<Animation>();
            Animation[] animationsArray = animationsList.ToArray();
            BattleAnimationPos battleAnimationPos = gameContext.battleAnimationPos[gameContext.currentBattleMonsters.Count - 1];

            Animation animation;

            for (int i = 0; i < gameContext.currentBattleMonsters.Count; i++)
            {
                if(target == gameContext.currentBattleMonsters[i])
                {
                    animation = gameContext.animationMap[$"{gameContext.currentBattleMonsters[i].ID}Defend"]!.DeepCopy();
                }
                else
                {
                    animation = gameContext.animationMap[$"{gameContext.currentBattleMonsters[i].ID}Idle"]!.DeepCopy();
                }
                if (gameContext.currentBattleMonsters[i].HP <= 0)
                {
                    animation = gameContext.animationMap[$"MonsterDie"]!.DeepCopy();
                }
                for (int j = 0; j < animation.frames.Length; j++)
                {
                    animation.x[j] += battleAnimationPos.monsterPosX[i];
                    animation.y[j] += battleAnimationPos.monsterPosY[i];
                }
                animationsList.Add(animation);
            }
            animation = gameContext.animationMap[$"{gameContext.ch.job}Attack"]!.DeepCopy();
            for (int i = 0; i < animation.frames.Length; i++)
            {
                animation.x[i] += battleAnimationPos.characterPosX;
                animation.y[i] += battleAnimationPos.characterPosY;
            }
            animationsList.Add(animation);
            animationsArray = animationsList.ToArray();

            gameContext.animationPlayer.play(animationsArray, (SpriteView)viewMap[ViewID.Sprite]);
        }
    }
}