using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextRPG.Context;
using TextRPG.View;

namespace TextRPG.Scene
{
    public class StatUpScene : AScene
    {
        public StatUpScene(GameContext gameContext, Dictionary<string, AView> viewMap, SceneText sceneText, SceneNext sceneNext) : base(gameContext, viewMap, sceneText, sceneNext)
        {

        }

        public override void DrawScene()
        {
            ClearScene();

            List<string> dynamicText = new();
            dynamicText.Add("어떤 스텟을 올릴까요?");
            dynamicText.Add($"레벨업 포인트 : {gameContext.ch.Point}");
            dynamicText.Add($"1. Str : {gameContext.ch.Str}");
            dynamicText.Add($"2. Dex : {gameContext.ch.Dex}");
            dynamicText.Add($"3. Int : {gameContext.ch.Int}");
            dynamicText.Add($"4. Luk : {gameContext.ch.Luk}");
            ((DynamicView)viewMap[ViewID.Dynamic]).SetText(dynamicText.ToArray());
            Render();
        }

        public override string respond(int i)
        {
            if (gameContext.ch.Point > 0 )
            {
                switch (i)
                {
                    case 1: gameContext.ch.Str++; break;
                    case 2: gameContext.ch.Dex++; break;
                    case 3: gameContext.ch.Int++; break;
                    case 4: gameContext.ch.Luk++; break;
                }
                gameContext.ch.Point--;  // 포인트 차감
            }
            else if (i != 0)
            {
                // 포인트 부족 메시지 출력
                ((DynamicView)viewMap[ViewID.Dynamic]).SetText(new string[]
                {
            "스탯 포인트가 부족합니다!",
            "",
            $"Str : {gameContext.ch.Str}",
            $"Dex : {gameContext.ch.Dex}",
            $"Int : {gameContext.ch.Int}",
            $"Luk : {gameContext.ch.Luk}",
                });

                Render();
                Thread.Sleep(1500); // 잠깐 보여주고 다시 DrawScene 호출
            }
            convertSceneAnimationPlay(sceneNext.next![i]);
            return sceneNext.next![i];
        }
    }
}
