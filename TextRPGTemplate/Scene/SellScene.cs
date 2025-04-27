using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextRPG.Context;
using TextRPG.View;

namespace TextRPG.Scene
{
    public class SellScene : AScene
    {
        public SellScene(GameContext gameContext, Dictionary<string, AView> viewMap, SceneText sceneText, SceneNext sceneNext) : base(gameContext, viewMap, sceneText, sceneNext)
        {

        }
        public override void DrawScene()
        {
            ClearScene();

            List<string> dynamicText = new();
            dynamicText.Add("[보유 골드]");
            dynamicText.Add($"{gameContext.ch.gold} G");
            dynamicText.Add("");
            dynamicText.Add("[아이템 목록]");
            for (int i = 0; i < gameContext.ch.inventory.items.Count; i++)
            {
                Item tmp = gameContext.ch.inventory.items[i];
                if (tmp.isPotion)
                {
                    // 포션인 경우: 체력/마나 회복량 표시
                    string effectText = "";
                    if (tmp.healAmount > 0) effectText += $"체력 +{tmp.healAmount} ";
                    if (tmp.manaAmount > 0) effectText += $"마나 +{tmp.manaAmount}";

                    dynamicText.Add($"-{i + 1}.{tmp.name} | {effectText} | {(tmp.bought ? "구매완료" : tmp.price + "G")}");
                }
                else
                {
                    // 일반 아이템인 경우: 기존 방식 유지
                    dynamicText.Add($"-{i + 1}.{tmp.name} | {(tmp.attack > 0 ? "공격력" : "방어력")} + {(tmp.attack > 0 ? tmp.attack : tmp.guard)} | {tmp.price + "G"}");
                }

                dynamicText.Add($"{tmp.description}");
            }

    ((DynamicView)viewMap[ViewID.Dynamic]).SetText(dynamicText.ToArray());
            Render();
        }

        public override string respond(int i)
        {
            Character ch = gameContext.ch;
            if (i > 0 && i < ch.inventory.items?.Count + 1)
            {
                ch.gold += (int)(ch.inventory.items![i - 1].price * 0.85f);
                ch.inventory.items![i - 1].bought = false;
                ch.inventory.items![i - 1].equiped = false;
                ((LogView)viewMap[ViewID.Log]).AddLog($"{gameContext.shop!.items![i - 1].name} 을 판매했습니다!");
                gameContext.ch.inventory.items!.Remove(ch.inventory.items![i - 1]);
            }
            else if (i != 0)
            {
                ((LogView)viewMap[ViewID.Log]).AddLog("잘못된 입력입니다!");
            }
            else if (i == 0)
            {
                //((LogView)viewMap[ViewID.Log]).AddLog("메인 화면으로 돌아갑니다!");
            }
            if (sceneNext.next![i] != SceneID.Shop)
            {
                convertSceneAnimationPlay(sceneNext.next![i]);
            }
            return sceneNext.next![i];
        }
    }
}
