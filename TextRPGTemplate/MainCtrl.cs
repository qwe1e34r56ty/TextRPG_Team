using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Text.Json;
using TextRPG.View;
using TextRPG.Scene;
using static System.Formats.Asn1.AsnWriter;
using TextRPG.Context;
using TextRPGTemplate.Animation;
using TextRPGTemplate.Managers;
using TextRPGTemplate.Scene;
using System.Diagnostics;
using System;
using System.IO;
using NAudio.Wave;

namespace TextRPG
{
    public class MainCtrl
    {
        static void Main(string[] args)
        {
            // 화면 크기 조정
            Console.SetWindowSize(183, 56);
            Console.SetBufferSize(183, 56);
            int width = Console.WindowWidth;
            int height = Console.WindowHeight;
            AudioManager.Instance().InitBgm();

            //Save 불러오기
            string? name = "";
            string? useSaveStr = "";
            bool? useSave = null;
            while (true)
            {
                if (File.Exists(JsonPath.saveDataJsonPath))
                {
                    Console.Write("이미 존재하는 세이브 데이터를 사용합니까?(Y/N) : ");
                    while (useSaveStr?.Length <= 0)
                    {
                        useSaveStr = Console.ReadLine();
                    }
                    switch (useSaveStr?[0])
                    {
                        case 'Y': useSave = true; break;
                        case 'N': useSave = false; break;
                        default: Console.Clear(); Console.SetCursorPosition(0, 1); Console.Write("잘못된 응답입니다."); Console.SetCursorPosition(0, 0); break;
                    }
                    useSaveStr = "";
                    if (useSave != null)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            string saveDataJson;
            SaveData saveData;
            if (useSave == true && File.Exists(JsonPath.saveDataJsonPath))
            {
                saveDataJson = File.ReadAllText(JsonPath.saveDataJsonPath);
                saveData = JsonSerializer.Deserialize<SaveData>(saveDataJson)!;
            }
            else
            {
                Console.Clear();
                Console.Write("사용할 이름을 입력하세요 : ");
                name = Console.ReadLine();
                var statCreater = new FirstStatsCreater(name, autoGenerate: false); // 자동 생성 X
                statCreater.GenerateStats();
                Console.Clear();
                saveDataJson = File.ReadAllText(JsonPath.defaultDataJsonPath);

                saveData = JsonSerializer.Deserialize<SaveData>(saveDataJson)!;

                saveData.name = name;
                saveData.Str = statCreater.Str;
                saveData.Int = statCreater.Int;
                saveData.Dex = statCreater.Dex;
                saveData.Luk = statCreater.Luk;
                saveData.hp = statCreater.hp;
                saveData.MaxHp = statCreater.MaxHp;
                saveData.Mp = statCreater.Mp;
                saveData.MaxMp = statCreater.MaxMp;
                saveData.gold = statCreater.Gold;
            }

            //정적 데이터 불러오기
            Dictionary<string, string?> animationPathMap = new();
            initanimationPathMap(animationPathMap);
            Dictionary<string, Animation?> animationMap = new();
            initanimationMap(animationPathMap, animationMap);
            Dictionary<string, AView> viewMap = new();
            initViewMap(viewMap);
            Dictionary<string, SceneText> sceneTextMap = new();
            initSceneTextMap(sceneTextMap);
            Dictionary<string, SceneNext> sceneNextMap = new();
            initSceneNextMap(sceneNextMap);
            Dictionary<string, AScene> sceneMap = new();
            Dictionary<string, SceneMaker> sceneFactoryMap = new();
            initSceneFactoryMap(sceneFactoryMap);

            var options = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true
            };
            //Save 외의 동적 데이터 불러오기
            var dungeonDataJson = File.ReadAllText(JsonPath.dungeonDataJsonPath);
            var dungeonData = JsonSerializer.Deserialize<List<DungeonData>>(dungeonDataJson);


            AnimationPlayer animationPlayer = new AnimationPlayer();
            // 몬스터 데이터 로드 추가
            var monsterDataJson = File.ReadAllText(JsonPath.monsterDataJsonPath);
            var monsterList = JsonSerializer.Deserialize<List<MonsterData>>(monsterDataJson);

            var battleAnimationPosJson = File.ReadAllText(JsonPath.battleAnimationPosJsonPath);
            var battleAnimationPos = JsonSerializer.Deserialize<List<BattleAnimationPos>>(battleAnimationPosJson);


            GameContext gameContext = new(saveData!, dungeonData!, monsterList!, animationPlayer!, animationMap, battleAnimationPos!);


            AScene startScene = sceneFactoryMap[SceneID.Main](gameContext,
                viewMap,
                sceneTextMap,
                sceneMap,
                sceneNextMap);

            Console.Clear();
            //실행            
            run(gameContext,
                startScene,
                viewMap,
                sceneTextMap,
                sceneMap,
                sceneFactoryMap,
                sceneNextMap);
        }

        static void initanimationPathMap(Dictionary<string, string> animationPathMap)
        {
            var animationPathJson = File.ReadAllText(JsonPath.animationPathJsonPath);
            var animationPaths = JsonSerializer.Deserialize<List<AnimationPath>>(animationPathJson);
            foreach (var animationPath in animationPaths!)
            {
                animationPathMap[animationPath.key] = animationPath.animationPath;
            }
        }
        static void initanimationMap(Dictionary<string, string?> animationPathMap, Dictionary<string, Animation?> animationMap)
        {
            string animationJson;
            foreach (var pair in animationPathMap)
            {
                if (animationPathMap[pair.Key] == null)
                {
                    animationMap[pair.Key] = null;
                    continue;
                }
                animationJson = File.ReadAllText(animationPathMap[pair.Key]!);
                animationMap[pair.Key] = JsonSerializer.Deserialize<Animation>(animationJson)!;
            }
        }

        static void run(GameContext gameContext,
            AScene startScene, Dictionary<string, AView> viewMap,
            Dictionary<string, SceneText> sceneTextMap,
            Dictionary<string, AScene> sceneMap,
            Dictionary<string, SceneMaker> sceneFactoryMap,
            Dictionary<string, SceneNext> sceneNextMap)
        {
            AScene curScene = startScene;
            string response = "";
            curScene.DrawScene();

            Animation[] animations = { gameContext.animationMap[SceneID.Main] };
            gameContext.animationPlayer.play(animations, (SpriteView)viewMap[ViewID.Sprite]);

            ((InputView)viewMap[ViewID.Input]).SetCursor();
            string? str = "";
            while (true)
            {
                str = Console.ReadLine();
                if (str?.Length <= 0)
                {
                    curScene.DrawScene();
                }
                else if (str?[0] == 'r')
                {
                    foreach (var pair in viewMap)
                    {
                        pair.Value.Update();
                        pair.Value.Render();
                    }
                }
                else if (str?[0] == 'q')
                {
                    SaveData saveData = new SaveData(gameContext);
                    string json = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(JsonPath.saveDataJsonPath, json);
                    return;
                }
                else
                {
                    if (!(str![0] >= '0' && str![0] <= '9'))
                    {
                        continue;
                    }
                    int i = str![0] - '0';
                    response = curScene.respond(i);
                    if (response == SceneID.Nothing)
                    {
                        curScene.DrawScene();
                    }
                    else
                    {
                        curScene = sceneFactoryMap[response](gameContext,
                            viewMap,
                            sceneTextMap,
                            sceneMap,
                            sceneNextMap);
                        curScene.DrawScene();
                    }
                    // todo 다른 화면 넘어가기? or 현재 화면 처리하기
                }
                str = "";
                ((InputView)viewMap[ViewID.Input]).Render();
                ((InputView)viewMap[ViewID.Input]).SetCursor();
            }
        }

        static void initViewMap(Dictionary<string, AView> viewMap)
        {
            viewMap.Add(ViewID.Script, new ScriptView());
            viewMap.Add(ViewID.Dynamic, new DynamicView());
            viewMap.Add(ViewID.Sprite, new SpriteView());
            viewMap.Add(ViewID.Log, new LogView());
            viewMap.Add(ViewID.Choice, new ChoiceView());
            viewMap.Add(ViewID.Input, new InputView());
            var viewTranJson = File.ReadAllText(JsonPath.viewTranJsonPath);
            var viewDefs = JsonSerializer.Deserialize<List<ViewTransform>>(viewTranJson);
            foreach (var def in viewDefs!)
            {
                viewMap[def.key].setTransform(def);
                viewMap[def.key].Update();
                viewMap[def.key].Render();
            }
        }

        static void initSceneTextMap(Dictionary<string, SceneText> sceneTextMap)
        {
            var sceneTextJson = File.ReadAllText(JsonPath.sceneTextJsonPath);
            var sceneTexts = JsonSerializer.Deserialize<List<SceneText>>(sceneTextJson);
            foreach (var sceneText in sceneTexts!)
            {
                sceneTextMap[sceneText.key] = sceneText;
            }
        }

        static void initSceneNextMap(Dictionary<string, SceneNext> sceneNextMap)
        {
            var sceneNextJson = File.ReadAllText(JsonPath.sceneNextJsonPath);
            var sceneNexts = JsonSerializer.Deserialize<List<SceneNext>>(sceneNextJson);
            foreach (var sceneNext in sceneNexts!)
            {
                sceneNextMap[sceneNext.key] = sceneNext;
            }
        }

        delegate void AddPair<T>(string sceneKey, Dictionary<string, SceneMaker> sceneFactoryMap);

        delegate AScene SceneMaker(GameContext gameContext,
            Dictionary<string, AView> viewMap,
            Dictionary<string, SceneText> sceneTextMap,
            Dictionary<string, AScene> sceneMap,
            Dictionary<string, SceneNext> sceneNextMap);

        static void RegisterScene<T>(Dictionary<string, SceneMaker> sceneFactoryMap, string sceneID) where T : AScene
        {
            sceneFactoryMap[sceneID] = (gameContext, viewMap, sceneTextMap, sceneMap, sceneNextMap) =>
            {
                if (!sceneMap.ContainsKey(sceneID))
                {
                    var sceneText = sceneTextMap[sceneID];
                    var sceneNext = sceneNextMap[sceneID];
                    var monsters = gameContext.currentBattleMonsters;
                    sceneMap[sceneID] = (AScene)Activator.CreateInstance(typeof(T), gameContext, viewMap, sceneText, sceneNext)!;
                }
                return sceneMap[sceneID];
            };
        }

        internal class AudioManager
        {
            private IWavePlayer waveOut;
            private AudioFileReader audioFile;

            private static AudioManager instance;
            public static AudioManager Instance()
            {
                if (instance == null)
                    instance = new AudioManager();
                return instance;
            }

            public void InitBgm()
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "bgm.wav");

                if (File.Exists(filePath))
                {
                    waveOut = new WaveOutEvent();
                    audioFile = new AudioFileReader(filePath);

                    audioFile.Volume = 0.3f;
                    // 재생이 끝날 때 이벤트를 감지하여 무한 반복
                    waveOut.PlaybackStopped += (sender, args) =>
                    {
                        audioFile.Position = 0; // 파일의 시작 위치로 되돌림
                        waveOut.Play();
                    };

                    waveOut.Init(audioFile);
                    waveOut.Play();
                }
            }
        }

                    // 새 Scene을 만들면 이 부분에 추가
                    static void initSceneFactoryMap(Dictionary<string, SceneMaker> sceneFactoryMap)
        {
            RegisterScene<MainScene>(sceneFactoryMap, SceneID.Main);
            RegisterScene<WearScene>(sceneFactoryMap, SceneID.Wear);
            RegisterScene<InventoryScene>(sceneFactoryMap, SceneID.Inventory);
            RegisterScene<StatusScene>(sceneFactoryMap, SceneID.Status);
            RegisterScene<ShopScene>(sceneFactoryMap, SceneID.Shop);
            RegisterScene<BuyScene>(sceneFactoryMap, SceneID.Buy);
            RegisterScene<SellScene>(sceneFactoryMap, SceneID.Sell);
            RegisterScene<RestScene>(sceneFactoryMap, SceneID.Rest);
            RegisterScene<DungeonSelectScene>(sceneFactoryMap, SceneID.DungeonSelect);
            RegisterScene<DungeonClearScene>(sceneFactoryMap, SceneID.DungeonClear);
            RegisterScene<DungeonFailScene>(sceneFactoryMap, SceneID.DungeonFail);
            RegisterScene<BattleScene>(sceneFactoryMap, SceneID.BattleScene);
            RegisterScene<BattleScene_SkillSelect>(sceneFactoryMap, SceneID.BattleScene_Skill);
            RegisterScene<StatUpScene>(sceneFactoryMap, SceneID.StatUp);
            RegisterScene<QuestScene>(sceneFactoryMap, SceneID.QuestScene);
            RegisterScene<NPCScene>(sceneFactoryMap, SceneID.NPCScene);
            RegisterScene<QuestClearScene>(sceneFactoryMap,SceneID.QuestClearScene);
            RegisterScene<GetJobScene>(sceneFactoryMap, SceneID.GetJob);
            RegisterScene<SkillManagerScene>(sceneFactoryMap, SceneID.SkillManager);
            RegisterScene<SkillLearnScene>(sceneFactoryMap, SceneID.SkillLearn);
            RegisterScene<SkillEquipScene>(sceneFactoryMap, SceneID.SkillEquip);
        }
    }
}

