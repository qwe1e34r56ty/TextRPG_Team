using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextRPG.Context;
using TextRPG.View;

namespace TextRPG.Scene
{
    public class DungeonClearScene : AScene
    {
        public DungeonClearScene(GameContext gameContext, Dictionary<string, AView> viewMap, SceneText sceneText, SceneNext sceneNext) : base(gameContext, viewMap, sceneText, sceneNext)
        {

        }
        public override void DrawScene()
        {
            ClearScene();
            List<string> dynamicText = new();

            // 디버그 정보 출력
            Debug.Write($"clearedMonsters 개수: {gameContext.clearedMonsters?.Count}");

            if (gameContext.clearedMonsters != null && gameContext.clearedMonsters.Count > 0)
            {
                dynamicText.Add("축하합니다!!");
                dynamicText.Add($"{gameContext.enteredDungeon!.Name}을 클리어 하였습니다");
                dynamicText.Add("");

                int totalExp = gameContext.clearedMonsters.Sum(m => m.ExpReward);
                int totalGold = gameContext.clearedMonsters.Sum(m => m.GoldReward);

                // 실제 보상 적용
                gameContext.ch.Exp += totalExp;                                   // 총 경험치 누적 (기록용)
                gameContext.ch.CurrentExp += totalExp;                            // 실제 레벨업 계산에 사용됨
                List<string> levelUpText = gameContext.ch.Levelup();              // 레벨업 처리 (CurrentExp, MaxExp 반영)
                foreach (string levelUp in levelUpText)
                {
                    ((LogView)viewMap[ViewID.Log]).AddLog(levelUp);
                }
                gameContext.ch.gold += totalGold;

                gameContext.curGold = gameContext.ch.gold;
                gameContext.curHp = gameContext.ch.hp;
                gameContext.curMp = gameContext.ch.Mp;

                dynamicText.Add($"획득 경험치: {totalExp} EXP");
                dynamicText.Add($"획득 골드: {totalGold} G");
                dynamicText.Add("");
                dynamicText.Add("[탐험 결과]");
                dynamicText.Add($"체력 {gameContext.prevHp} -> {gameContext.curHp}");
                dynamicText.Add($"마나 {gameContext.prevMp} -> {gameContext.curMp}");
                dynamicText.Add($"골드 {gameContext.prevGold}G -> {gameContext.curGold}G");
                var quest = gameContext.questData[gameContext.questinput];
                if (quest.clearquest == false)
                {
                    for (int i = 0; i < gameContext.clearedMonsters.Count; i++)
                    {
                        if (quest.questitem == gameContext.clearedMonsters[i].Dropitem)
                        {
                            quest.dropitemcount++;
                            dynamicText.Add($"{quest.questitem} 아이템을 얻었습니다!" +
                            $"({quest.dropitemcount}/{quest.questfigure})");
                        }
                    }
                }
                gameContext.clearedMonsters.Clear();



                if (quest.dropitemcount >= quest.questfigure)
                {
                    quest.clearquest = true;
                    gameContext.isaccept = false;
                }

                gameContext.ch.clearCount++;
            }

            // 뷰 업데이트
            ((DynamicView)viewMap[ViewID.Dynamic]).SetText(dynamicText.ToArray());
            Render();
        }


        public override string respond(int i)
        {
            convertSceneAnimationPlay(sceneNext.next![i]);
            return sceneNext.next![i];
        }
    }
}
