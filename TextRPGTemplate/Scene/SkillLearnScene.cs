﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextRPG.Context;
using TextRPG.Scene;
using TextRPG.View;
using TextRPGTemplate.Context;

namespace TextRPG.Scene
{
    public class SkillLearnScene : AScene
    {
        public SkillLearnScene(GameContext gameContext, Dictionary<string, AView> viewMap, SceneText sceneText, SceneNext sceneNext) : base(gameContext, viewMap, sceneText, sceneNext)
        {

        }

        public override void DrawScene()
        {
            ClearScene();
            List<string> dynamicText = new();

            dynamicText.Add("[스킬 목록]");
            dynamicText.Add("");

            if (gameContext.ch.characterSkillList == null || gameContext.ch.characterSkillList.Count == 0)
            {
                dynamicText.Add("배울수 있는 스킬이 없습니다.");
            }
            else
            {
                for (int i = 0; i < gameContext.ch.characterSkillList.Count; i++)
                {
                    Skill skill = gameContext.ch.characterSkillList[i];

                    string skillType = "";
                    switch (skill.skillType)
                    {
                        case SkillType.Attack: skillType = "공격 스킬"; break;
                        case SkillType.Defence: skillType = "방어 스킬"; break;
                        case SkillType.Utility: skillType = "보조 스킬"; break;
                    }

                    string statType = "";
                    switch (skill.statType)
                    {
                        case StatType.None: statType = "없음"; break;
                        case StatType.Str: statType = "힘"; break;
                        case StatType.Dex: statType = "민첩"; break;
                        case StatType.Int: statType = "지능"; break;
                        case StatType.Luk: statType = "운"; break;
                    }

                    if (skill.isLearn)
                    {
                        dynamicText.Add($"[{i + 1}] {skill.skillName} | [습득 완료]");
                    }
                    else
                    {
                        dynamicText.Add($"[{i + 1}] {skill.skillName} | 요구조건 : {skill.skillLevel} Level | {skill.skillPoint} Point");
                    }
                    dynamicText.Add($"    {skill.description}");
                    dynamicText.Add("");
                }
            }

            ((DynamicView)viewMap[ViewID.Dynamic]).SetText(dynamicText.ToArray());

            Render();
        }


        //기능
        public override string respond(int i)
        {
            if (i > 0 && i <= (gameContext.ch.characterSkillList?.Count ?? 0))
            {
                LearnSkill(i);
            }
            else if (i > (gameContext.ch.characterSkillList?.Count ?? 0) || i < 0)
            {
                ((LogView)viewMap[ViewID.Log]).AddLog($"잘못된 입력입니다.");
            }
            if (sceneNext.next![i] != SceneID.SkillManager)
            {
                convertSceneAnimationPlay(sceneNext.next![i]);
            }
            return sceneNext.next![i];
        }

        public void LearnSkill(int i)
        {
            if (gameContext.ch.characterSkillList[i - 1].isLearn)
            {
                ((LogView)viewMap[ViewID.Log]).AddLog($"이미 배운 스킬입니다.");
            }
            else
            {
                if (gameContext.ch.characterSkillList[i - 1].skillLevel <= gameContext.ch.Level)
                {
                    if (gameContext.ch.characterSkillList[i - 1].skillPoint <= gameContext.ch.Point)
                    {
                        gameContext.ch.learnSkillList.Add(gameContext.ch.characterSkillList[i - 1]);
                        ((LogView)viewMap[ViewID.Log]).AddLog($"{gameContext.ch.characterSkillList[i - 1].skillName} 획득 성공!");
                        gameContext.ch.characterSkillList[i - 1].isLearn = true;
                    }
                    else
                    {
                        ((LogView)viewMap[ViewID.Log]).AddLog($"{gameContext.ch.characterSkillList[i - 1].skillName}스킬을 배우기 위한 포인트가 {gameContext.ch.characterSkillList[i - 1].skillPoint - gameContext.ch.Point} 부족합니다.");
                    }
                }
                else
                {
                    ((LogView)viewMap[ViewID.Log]).AddLog($"{gameContext.ch.characterSkillList[i - 1].skillName}스킬을 배우기 위한 레벨이 {gameContext.ch.characterSkillList[i - 1].skillLevel - gameContext.ch.Level} 부족합니다.");
                }
            }
        }
    }
}
