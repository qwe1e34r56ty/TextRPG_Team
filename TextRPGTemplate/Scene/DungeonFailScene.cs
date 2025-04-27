using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextRPG.Context;
using TextRPG.View;

namespace TextRPG.Scene
{
    public class DungeonFailScene : AScene
    {
        public DungeonFailScene(GameContext gameContext, Dictionary<string, AView> viewMap, SceneText sceneText, SceneNext sceneNext) : base(gameContext, viewMap, sceneText, sceneNext)
        {

        }
        public override void DrawScene()
        {
            ClearScene();

            List<string> dynamicText = new();
                dynamicText.Add($"{gameContext.enteredDungeon!.Name} 클리어를 실패했습니다.");
                dynamicText.Add("\n");
                dynamicText.Add("[탐험 결과]");
                gameContext.ch.hp = 0;
            dynamicText.Add($"체력 {gameContext.prevHp} -> {gameContext.ch.hp}");
            dynamicText.Add($"마나 {gameContext.prevMp} -> {gameContext.ch.Mp}");

            //dynamicText.Add($"500 G 를 내면 체력을 회복할 수 있습니다. (보유 골드:{gameContext.ch.gold})");
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
