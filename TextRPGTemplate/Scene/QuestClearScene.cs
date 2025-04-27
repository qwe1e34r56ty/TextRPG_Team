using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextRPG.Context;
using TextRPG.View;
using TextRPGTemplate.Context;

// QuestClear시 여기로 이동
namespace TextRPG.Scene
{
    public class QuestClearScene : AScene
    {
        public QuestClearScene(GameContext gameContext, Dictionary<string, AView> viewMap, SceneText sceneText, SceneNext sceneNext) : base(gameContext, viewMap, sceneText, sceneNext)
        {

        }

        public override void DrawScene()
        {
            ClearScene();

            List<string> dynamicText = new();
            QuestData quest = gameContext.questData[gameContext.questinput];

            if (quest.clearquest && !quest.rewardReceived)
            {
                dynamicText.Add($"완료한 퀘스트 : [{quest.npc}]의 부탁으로 [{quest.questitem}] 가져오기");
                dynamicText.Add($"{quest.dropitemcount}/{quest.questfigure}\n");

                quest.dropitemcount -= quest.questfigure;

                dynamicText.Add("보상 : ");
                switch (gameContext.questinput)
                {
                    case 0:
                        {
                            gameContext.ch.MaxHp += 10;
                            gameContext.ch.hp += 10;
                            dynamicText.Add($"상점 주인이 치유 물약을 내밉니다.");
                            dynamicText.Add($"HP가 증가했습니다! ( +10 )");
                            break;
                        }
                    case 1:
                        {
                            gameContext.ch.attack += 2;
                            dynamicText.Add($"대장장이 아저씨가 수고했다며 가문의 비기를 전수해 줍니다.");
                            dynamicText.Add($"힘이 증가했습니다! ( +2 )");
                            break;
                        }
                    case 2:
                        {
                            gameContext.ch.Point += 3;
                            dynamicText.Add($"꼬마가 감사하다며 신비한 약초를 건네줍니다.");
                            dynamicText.Add("포인트를 얻었습니다. ( +3 )");
                            break;
                        }
                }
                quest.rewardReceived = true;
            }
            else if (quest.rewardReceived)
            {
                dynamicText.Add($"이미 보상을 받았습니다.");
            }

            ((DynamicView)viewMap[ViewID.Dynamic]).SetText(dynamicText.ToArray());
            //((SpriteView)viewMap[ViewID.Sprite]).SetText(sceneText.spriteText!);
            Render();
        }

        public override string respond(int i)
        {
            if (sceneNext.next![i] != SceneID.NPCScene)
            {
                convertSceneAnimationPlay(sceneNext.next![i]);
            }
            return sceneNext.next![i];
        }
    }
}
