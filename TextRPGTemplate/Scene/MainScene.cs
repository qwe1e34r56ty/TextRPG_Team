using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextRPG.Context;
using TextRPG.View;
using TextRPGTemplate.Animation;
namespace TextRPG.Scene
{
    public class MainScene : AScene
    {
        public MainScene(GameContext gameContext, Dictionary<string, AView> viewMap, SceneText sceneText, SceneNext sceneNext) : base(gameContext, viewMap, sceneText, sceneNext)
        {
           
        }

        public override string respond(int i)
        {
            convertSceneAnimationPlay(sceneNext.next![i]);
            return sceneNext.next![i];
        }
    }
}
