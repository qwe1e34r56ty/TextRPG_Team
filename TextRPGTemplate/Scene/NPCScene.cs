using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextRPG.Context;
using TextRPG.View;
using TextRPGTemplate.Context;

// 마을 사람들과 대화
namespace TextRPG.Scene
{
    public class NPCScene : AScene
    {
        public NPCScene(GameContext gameContext, Dictionary<string, AView> viewMap, SceneText sceneText, SceneNext sceneNext) : base(gameContext, viewMap, sceneText, sceneNext)
        {

        }

        public override void DrawScene()
        {
            ClearScene();

            List<string> dynamicText = new();
            dynamicText.Add("누구와 대화를 나눠보시겠습니까?\n");
            dynamicText.Add("> 1. 상점 주인");
            dynamicText.Add("> 2. 대장장이 아저씨");
            dynamicText.Add("> 3. 꽃을 파는 꼬마");

            if (gameContext.isaccept == true)
            {
                dynamicText.Add("이미 퀘스트를 진행 중입니다.");
                dynamicText.Add($"{gameContext.questData[gameContext.questinput].dropitemcount}/{gameContext.questData[gameContext.questinput].questfigure}\n");
            }
                ((DynamicView)viewMap[ViewID.Dynamic]).SetText(dynamicText.ToArray());
            //((SpriteView)viewMap[ViewID.Sprite]).SetText(sceneText.spriteText!);
            Render();
        }

        public override string respond(int i)
        {
            if (gameContext.isaccept == false)
            {
                if (i == 0)
                {
                    convertSceneAnimationPlay(sceneNext.next![i]);
                    return sceneNext.next![i];
                }
                else if (i < gameContext.questData.Length + 1)
                {
                    gameContext.questinput = i - 1; //번호에 맞는 npc
                    QuestData quest = gameContext.questData[gameContext.questinput];
                    if (quest.clearquest == false)
                    {
                        return SceneID.QuestScene;
                    }
                    else if (quest.clearquest == true)
                    {
                        return SceneID.QuestClearScene;
                    }
                }
                else
                {
                    ((LogView)viewMap[ViewID.Log]).AddLog("잘못된 입력입니다.");
                    return sceneNext.next![i];
                }
            }
            else if (gameContext.isaccept == true)
            {
                if(i == 0)
                {
                    convertSceneAnimationPlay(sceneNext.next![i]);
                    return sceneNext.next![i];
                }
                //Thread.Sleep(1000);
                ((LogView)viewMap[ViewID.Log]).AddLog("이미 수락한 퀘스트가 있습니다.");
                return SceneID.Nothing;
            }

            convertSceneAnimationPlay(sceneNext.next![i]);
            return sceneNext.next![i];
        }
    }
}
