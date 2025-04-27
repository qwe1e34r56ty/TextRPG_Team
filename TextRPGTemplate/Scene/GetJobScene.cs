using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextRPG.Context;
using TextRPG.View;

namespace TextRPG.Scene
{
    public class GetJobScene : AScene
    {
        private List<JobRequirement> eligibleJobs = new();

        public GetJobScene(GameContext gameContext, Dictionary<string, AView> viewMap, SceneText sceneText, SceneNext sceneNext) : base(gameContext, viewMap, sceneText, sceneNext)
        {

        }
        public override void DrawScene()
        {
            ClearScene();

            List<string> dynamicText = new();

            Character ch = gameContext.ch;

            dynamicText.Add($" Lv. {ch.Level}\n");
            dynamicText.Add($" 힘 : {ch.Str}\n");
            dynamicText.Add($" 지 능 : {ch.Int}\n");
            dynamicText.Add($" 민 첩 : {ch.Dex}\n");
            dynamicText.Add($" 운 : {ch.Luk}\n");
            dynamicText.Add($"체 력 : {ch.hp} / {ch.MaxHp}");
            dynamicText.Add($"Gold : {ch.gold}G");
            dynamicText.Add("원하는 직업을 선택해 주세요.");
            List<JobRequirement> jobRequirements = new()
            {
                new JobRequirement { JobName = "파이터", MinLevel = 0, MinStr = 0 },
                new JobRequirement { JobName = "레인저", MinLevel = 0, MinDex = 0 },
                new JobRequirement { JobName = "위자드", MinLevel = 0, MinInt = 0 },
                new JobRequirement { JobName = "시프", MinLevel = 0, MinLuk = 0 },
            };
            eligibleJobs = jobRequirements
                .Where(req => req.IsEligible(ch))
                .ToList();

            for (int i = 0; i < eligibleJobs.Count; i++)
            {
                dynamicText.Add($"{i + 1}. {eligibleJobs[i].Message}");
            }
            ((DynamicView)viewMap[ViewID.Dynamic]).SetText(dynamicText.ToArray());
            Render();
        }

        public override string respond(int i)
        {
            if (i < 0 || i > eligibleJobs.Count)
            {
                return "None";
            }
            else if (i > 0 && i <= eligibleJobs.Count && gameContext.ch.job == "")
            {
                var selectedJob = eligibleJobs[i - 1];
                gameContext.ch.job = selectedJob.JobName;   // 클래스 직업변경
                ((LogView)viewMap[ViewID.Log]).AddLog($"{selectedJob.JobName}로 전직했습니다.");
                gameContext.ch.AddJobStat(gameContext.afterJobStat![i-1]);
                SetSkillList(gameContext.afterJobStat[i - 1].jobSkills!);
            }
            if (sceneNext.next![i] != SceneID.Status)
            {
                convertSceneAnimationPlay(SceneID.Status);
            }
            //convertSceneAnimationPlay(sceneNext.next![i]);
            return sceneNext.next![i];
        }

        public void SetSkillList(List<string> skillList)
        {
            for (int i = 0; i < skillList.Count; i++)
            {
                for (int j = 0; j < gameContext.skillList.Length; j++)
                {
                    if (skillList[i] == gameContext.skillList[j].key)
                    {
                        gameContext.ch.characterSkillList.Add(gameContext.skillList[j]);
                        if(gameContext.skillList[j].isLearn == true)
                        {
                            gameContext.ch.learnSkillList.Add(gameContext.skillList[j]);
                            if(gameContext.skillList[j].isEquip == true)
                            {
                                gameContext.ch.equipSkillList[gameContext.skillList[j].equipSlot] = gameContext.skillList[j];
                            }
                        }
                    }
                }
            }
        }
    }    
}
