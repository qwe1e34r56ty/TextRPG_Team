using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextRPG.Context;
using TextRPG.View;

namespace TextRPG.Scene
{
    public class InventoryScene : AScene
    {
        public InventoryScene(GameContext gameContext, Dictionary<string, AView> viewMap, SceneText sceneText, SceneNext sceneNext) : base(gameContext, viewMap, sceneText, sceneNext)
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
            for(int i = 0; i < gameContext.ch.inventory.items.Count; i++)
            {
                Item tmp = gameContext.ch.inventory.items[i];
                if (tmp.isPotion)
                {
                    // 포션인 경우: 체력/마나 회복량 표시
                    string effectText = "";
                    if (tmp.healAmount > 0) effectText += $"체력 +{tmp.healAmount} ";
                    if (tmp.manaAmount > 0) effectText += $"마나 +{tmp.manaAmount}";

                    dynamicText.Add($"{tmp.name} \t | {effectText} \t | {(tmp.bought ? "구매완료" : tmp.quantity + "개")}");
                }
                else
                {
                    // 일반 아이템인 경우: 기존 방식 유지
                    dynamicText.Add($"-{(tmp.equiped ? "[E]" : "")} {tmp.name}  | {(tmp.attack > 0 ? "공격력" : "방어력")} + {(tmp.attack > 0 ? tmp.attack : tmp.guard)}");
                    dynamicText.Add($" {tmp.description}");
                }
            }
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
