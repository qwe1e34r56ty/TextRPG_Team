using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextRPG.Context;
using TextRPG.View;

namespace TextRPG.Scene
{
    public class WearScene : AScene
    {
        public WearScene(GameContext gameContext, Dictionary<string, AView> viewMap, SceneText sceneText, SceneNext sceneNext) : base(gameContext, viewMap, sceneText, sceneNext)
        {

        }
        public override void DrawScene()
        {
            ClearScene();

            List<string> dynamicText = new();
            dynamicText.Add("[아이템 목록]");
            int displayIndex = 1; // 1부터 시작하는 표시용 인덱스

            for (int i = 0; i < gameContext.ch.inventory.items.Count; i++)
            {
                Item tmp = gameContext.ch.inventory.items[i];
                if (tmp.isPotion)
                {
                    continue; // 포션은 건너뜀
                }
                else
                {
                    // displayIndex를 사용하여 1, 2, 3... 순으로 출력
                    dynamicText.Add($"- {displayIndex} {(tmp.equiped ? "[E]" : "")} {tmp.name} \t | {(tmp.attack > 0 ? "공격력" : "방어력")} + {(tmp.attack > 0 ? tmp.attack : tmp.guard)} \t | {tmp.description}");
                    displayIndex++; // 포션을 제외한 아이템만 증가
                }
            }
    ((DynamicView)viewMap[ViewID.Dynamic]).SetText(dynamicText.ToArray());
            Render();
        }
        public override string respond(int inputIndex)
        {
            if (inputIndex == 0)
            {
                // 0번은 뒤로 가기
                //convertSceneAnimationPlay(sceneNext.next![0]);
                return sceneNext.next![0];
            }

            // 포션을 제외한 유효한 아이템 인덱스 찾기
            int actualIndex = -1;
            int displayCount = 0;

            for (int i = 0; i < gameContext.ch.inventory.items.Count; i++)
            {
                if (!gameContext.ch.inventory.items[i].isPotion)
                {
                    displayCount++;
                    if (displayCount == inputIndex)
                    {
                        actualIndex = i; // 실제 인덱스 저장
                        break;
                    }
                }
            }

            if (actualIndex >= 0)
            {
                // 아이템 장착/해제 로직 (기존 코드 유지, 단 i → actualIndex로 변경)
                bool weaponEquiped = false;
                bool armorEquiped = false;
                Character ch = gameContext.ch;

                foreach (var item in ch.inventory.items!)
                {
                    if (item.weapon && item.equiped) weaponEquiped = true;
                    if (item.armor && item.equiped) armorEquiped = true;
                }

                Item selectedItem = ch.inventory.items[actualIndex];

                if (selectedItem.weapon && !selectedItem.equiped && weaponEquiped)
                {
                    foreach (var item in ch.inventory.items.Where(x => x.weapon && x.equiped))
                        item.equiped = false;
                }

                if (selectedItem.armor && !selectedItem.equiped && armorEquiped)
                {
                    foreach (var item in ch.inventory.items.Where(x => x.armor && x.equiped))
                        item.equiped = false;
                }

                selectedItem.equiped = !selectedItem.equiped;
            }
            else
            {
                ((LogView)viewMap[ViewID.Log]).AddLog("잘못된 입력입니다!");
            }

            convertSceneAnimationPlay(sceneNext.next![inputIndex]);
            return sceneNext.next![inputIndex];
        }
    }
}