using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextRPG.Context;
using TextRPG.View;

namespace TextRPG.Scene
{
    public class StatusScene : AScene
    {
        public StatusScene(GameContext gameContext, Dictionary<string, AView> viewMap, SceneText sceneText, SceneNext sceneNext) : base(gameContext, viewMap, sceneText, sceneNext)
        {

        }
        public override void DrawScene()
        {
            ClearScene();

            List<string> dynamicText = new();
            List<Item> equipItemList = new();
            foreach (var item in gameContext.ch.inventory.items)
            {
                if (item.equiped)
                {
                    equipItemList.Add(item);
                }
                else
                {
                    continue;
                }
            }
            int itemAddattack = 0;
            int itemAddguard = 0;
            foreach (var item in equipItemList)
            {
                if (item.attack > 0)
                {
                    itemAddattack += item.attack;
                }
                else
                {
                    itemAddguard += item.guard;
                }
            }
            float totalAttack = (gameContext.ch.defaultAttack) + itemAddattack;
            float totalGuard = (gameContext.ch.defaultGuard) + itemAddguard;
            Character ch = gameContext.ch;

            dynamicText.Add($" Lv. {ch.Level}\n");
            dynamicText.Add($" 이 름 : {ch.name}\n");
            dynamicText.Add($" 직 업 : {ch.job}\n");
            dynamicText.Add($" 힘 : {ch.Str}\n");
            dynamicText.Add($" 지 능 : {ch.Int}\n");
            dynamicText.Add($" 민 첩 : {ch.Dex}\n");
            dynamicText.Add($" 운 : {ch.Luk}\n");
            dynamicText.Add($"체 력 : {ch.hp} / {ch.MaxHp}");
            dynamicText.Add($"마 나 : {ch.Mp} / {ch.MaxMp}");
            dynamicText.Add($"경험치 : {ch.CurrentExp} / {ch.MaxExp}");
            dynamicText.Add($"Gold : {ch.gold}G");
            dynamicText.Add($"Critical : {ch.critical}");
            dynamicText.Add($"주스텟 : {ch.statType}");
            dynamicText.Add($"총 공격력 {totalAttack} : 기본 공격력({ch.defaultAttack}) + 추가 공격력 ({itemAddattack})");
            dynamicText.Add($"총 방어력 {totalGuard} : 기본 방어력({ch.defaultGuard}) + 추가 방어력 ({itemAddguard})");

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
