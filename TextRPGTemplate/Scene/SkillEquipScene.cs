using System;
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
    public class SkillEquipScene : AScene
    {
        public SkillEquipScene(GameContext gameContext, Dictionary<string, AView> viewMap, SceneText sceneText, SceneNext sceneNext) : base(gameContext, viewMap, sceneText, sceneNext)
        {

        }


        public override void DrawScene()
        {
            ClearScene();
            List<string> dynamicText = new();

            dynamicText.Add("[스킬 목록]");
            dynamicText.Add("");

            if (gameContext.ch.learnSkillList == null || gameContext.ch.learnSkillList.Count == 0)
            {
                dynamicText.Add("장착할 수 있는 스킬이 없습니다.");
            }
            else
            {
                for (int i = 0; i < gameContext.ch.learnSkillList.Count; i++)
                {
                    Skill learnSkill = gameContext.ch.learnSkillList[i];

                    string skillType = "";
                    switch (learnSkill.skillType)
                    {
                        case SkillType.Attack: skillType = "공격스킬"; break;
                        case SkillType.Defence: skillType = "방어스킬"; break;
                        case SkillType.Utility: skillType = "보조스킬"; break;
                    }

                    string statType = "";
                    switch (learnSkill.statType)
                    {
                        case StatType.None: statType = "없음"; break;
                        case StatType.Str: statType = "힘"; break;
                        case StatType.Dex: statType = "민첩"; break;
                        case StatType.Int: statType = "지능"; break;
                        case StatType.Luk: statType = "운"; break;
                    }

                    dynamicText.Add($"{i + 1}.{(learnSkill.isEquip ? "[E]" : "")} {learnSkill.skillName} | {skillType} | {statType} | 마나 : {learnSkill.costMana} | 횟수 : {learnSkill.maxUseCount} |");
                    dynamicText.Add($"\t쿨타임 : {learnSkill.coolTime}턴 | {(learnSkill.duration[0] == 0 ? "즉발" : $"{learnSkill.duration[0]}턴")}");
                    dynamicText.Add("");
                }
            }

            ((DynamicView)viewMap[ViewID.Dynamic]).SetText(dynamicText.ToArray());
            Render();
        }

        //기능
        public override string respond(int i)
        {
            if (i > 0 && i <= (gameContext.ch.learnSkillList?.Count ?? 0))
            {
                EquipSKill(i);
            }
            else if (i > (gameContext.ch.learnSkillList?.Count ?? 0) || i < 0)
            {
                ((LogView)viewMap[ViewID.Log]).AddLog($"잘못된 입력입니다.");
            }
            if (sceneNext.next![i] != SceneID.SkillManager)
            {
                convertSceneAnimationPlay(sceneNext.next![i]);
            }
            return sceneNext.next![i];
        }

        public void EquipSKill(int i)
        {
            if (!UseableSkillSlot())
            {
                ((LogView)viewMap[ViewID.Log]).AddLog($"더 이상 스킬을 장착할 수 없습니다");
                ((LogView)viewMap[ViewID.Log]).AddLog($"등록된 스킬을 해제하거나 사용가능 슬롯을 늘려주세요");
            }
            else
            {
                Skill selectSkill = gameContext.ch.learnSkillList[i - 1];

                if (selectSkill.isEquip)
                {
                    selectSkill.isEquip = false;
                    gameContext.ch.equipSkillList[selectSkill.equipSlot] = null;
                    selectSkill.equipSlot = -1;                 
                    ((LogView)viewMap[ViewID.Log]).AddLog($"{selectSkill.skillName} 스킬 등록을 해제합니다.");
                }
                else
                {
                    selectSkill.isEquip = true;
                    for (int j = 0; j < gameContext.ch.equipSkillList.Length; j++)
                    { 
                        if(gameContext.ch.equipSkillList[j] == null)
                        {
                            gameContext.ch.equipSkillList[j] = selectSkill;
                            selectSkill.equipSlot = j;
                            break;
                        }
                    }
                    ((LogView)viewMap[ViewID.Log]).AddLog($"{selectSkill.skillName} 스킬을 등록 합니다.");
                }
            }
        }

        public bool UseableSkillSlot()
        {
            if (gameContext.ch.learnSkillList.Count < gameContext.ch.useableSlot)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
