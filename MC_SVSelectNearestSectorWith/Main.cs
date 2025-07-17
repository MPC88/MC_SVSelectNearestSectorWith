using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MC_SVSelectNearestSectorWith
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string pluginGuid = "mc.starvalor.selectclosestsectorwithquest";
        public const string pluginName = "SV Select Closest Sector with...";
        public const string pluginVersion = "2.0.0";

        private static GameObject questButton;
        private static GameObject ravagerButton;
        private static GameObject stationButton;
        private static List<GameObject> factionButtons;
        private static GameObject marketSearchButton;

        internal static ConfigEntry<int> lastType;
        internal static ConfigEntry<int> lastSort;

        internal static ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource(pluginName);

        public void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(Main));
            LoadAssets();

            lastType = Config.Bind<int>("Memory",
                "lastType",
                0,
                "");
            lastSort = Config.Bind<int>("Memory",
                "lastSort",
                0,
                "");
        }

        private void LoadAssets()
        {
            string pluginfolder = System.IO.Path.GetDirectoryName(GetType().Assembly.Location);
            string bundleName = "mc_svselectnearestsectorwith";
            AssetBundle assets = AssetBundle.LoadFromFile($"{pluginfolder}\\{bundleName}");
            GameObject pack = assets.LoadAsset<GameObject>("Assets/mc_marketsearch.prefab");
            Assets.marketSearchPanel = pack.transform.Find("mc_marketSearchPanel").gameObject;
            Assets.marketSearchResultItem = pack.transform.Find("mc_marketSearchItem").gameObject;

            Language.Load(pluginfolder + "\\MC_SVSelectNearestSectorWithLang.txt");
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
                MarketSearch.EnterPress();
            else if (Input.GetKeyDown(KeyCode.Escape))
                MarketSearch.EscapePress();
        }

        [HarmonyPatch(typeof(GalaxyMap), nameof(GalaxyMap.ShowHideGalaxyMap))]
        [HarmonyPostfix]
        private static void GMShowHide_Pre(GalaxyMap __instance, GameObject ___BtnWarp, Slider ___sliderFactionInfluence)
        {
            if (questButton == null)
            {
                questButton = Instantiate(___BtnWarp);
                questButton.name = "BtnFindNearestQuest";
                Destroy(questButton.transform.Find("Cost").gameObject);
                questButton.SetActive(true);
                questButton.GetComponentInChildren<Text>().text = Language.NearestQuest;
                questButton.SetActive(false);
                Button.ButtonClickedEvent btnClickEvent = new Button.ButtonClickedEvent();
                btnClickEvent.AddListener(new UnityAction(QuestBtnClick));
                questButton.GetComponentInChildren<Button>().onClick = btnClickEvent;
                questButton.transform.SetParent(___sliderFactionInfluence.transform.parent);
                questButton.layer = ___sliderFactionInfluence.gameObject.layer;
                questButton.transform.localPosition = new Vector3(
                    ___sliderFactionInfluence.transform.localPosition.x - (___BtnWarp.GetComponentInChildren<RectTransform>().rect.x),
                    ___sliderFactionInfluence.transform.localPosition.y + (___BtnWarp.GetComponentInChildren<RectTransform>().rect.y * 2),
                    ___sliderFactionInfluence.transform.localPosition.z
                    );
                questButton.transform.localScale = ___sliderFactionInfluence.transform.localScale;
            }
            questButton.SetActive(__instance.gameObject.activeSelf);            

            if (ravagerButton == null)
            {
                ravagerButton = Instantiate(___BtnWarp);
                ravagerButton.name = "BtnFindNearestRavager";
                Destroy(ravagerButton.transform.Find("Cost").gameObject);
                ravagerButton.SetActive(true);
                ravagerButton.GetComponentInChildren<Text>().text = Language.NearestRavager;
                ravagerButton.SetActive(false);
                Button.ButtonClickedEvent btnClickEvent = new Button.ButtonClickedEvent();
                btnClickEvent.AddListener(new UnityAction(RavagerBtnClick));
                ravagerButton.GetComponentInChildren<Button>().onClick = btnClickEvent;
                ravagerButton.transform.SetParent(questButton.transform.parent);
                ravagerButton.layer = questButton.gameObject.layer;
                ravagerButton.transform.localPosition = new Vector3(
                    ___sliderFactionInfluence.transform.localPosition.x - (___BtnWarp.GetComponentInChildren<RectTransform>().rect.x),
                    questButton.transform.localPosition.y + (___BtnWarp.GetComponentInChildren<RectTransform>().rect.y * 1.1f),
                    questButton.transform.localPosition.z
                    );
                ravagerButton.transform.localScale = questButton.transform.localScale;
            }
            ravagerButton.SetActive(__instance.gameObject.activeSelf);

            if (stationButton == null)
            {
                stationButton = Instantiate(___BtnWarp);
                stationButton.name = "BtnFindNearestStation";
                Destroy(stationButton.transform.Find("Cost").gameObject);
                stationButton.SetActive(true);
                stationButton.GetComponentInChildren<Text>().text = Language.NearestStation;
                stationButton.SetActive(false);
                Button.ButtonClickedEvent btnClickEvent = new Button.ButtonClickedEvent();
                btnClickEvent.AddListener(new UnityAction(StationButtonClick));
                stationButton.GetComponentInChildren<Button>().onClick = btnClickEvent;
                stationButton.transform.SetParent(questButton.transform.parent);
                stationButton.layer = questButton.gameObject.layer;
                stationButton.transform.localPosition = new Vector3(
                    ___sliderFactionInfluence.transform.localPosition.x - (___BtnWarp.GetComponentInChildren<RectTransform>().rect.x),
                    ravagerButton.transform.localPosition.y + (___BtnWarp.GetComponentInChildren<RectTransform>().rect.y * 1.1f),
                    ravagerButton.transform.localPosition.z
                    );
                stationButton.transform.localScale = questButton.transform.localScale;

                factionButtons = new List<GameObject>();

                for (int i = 0; i < 7; i ++)
                {
                    GameObject btn = Instantiate(___BtnWarp);
                    btn.name = "BtnFaction" + i;
                    Destroy(btn.transform.Find("Cost").gameObject);
                    btn.SetActive(true);
                    btn.GetComponentInChildren<Text>().text = FactionDB.GetFaction(i).factionName;
                    btn.SetActive(false);
                    Button.ButtonClickedEvent btnEvent = new Button.ButtonClickedEvent();
                    int ci = i;
                    btnEvent.AddListener(() => FactionButtonClick(ci));
                    btn.GetComponentInChildren<Button>().onClick = btnEvent;
                    btn.transform.SetParent(questButton.transform.parent);
                    btn.layer = questButton.gameObject.layer;
                    btn.transform.localPosition = new Vector3(
                        stationButton.transform.localPosition.x - ((stationButton.GetComponentInChildren<RectTransform>().rect.x) * 2.1f),
                        stationButton.transform.localPosition.y + ((___BtnWarp.GetComponentInChildren<RectTransform>().rect.y * 1.1f) * i),
                        stationButton.transform.localPosition.z
                        );
                    btn.transform.localScale = questButton.transform.localScale;
                    factionButtons.Add(btn);
                }
            }
            stationButton.SetActive(__instance.gameObject.activeSelf);
            factionButtons.ForEach(x => x.SetActive(false));

            if (marketSearchButton == null)
            {
                marketSearchButton = Instantiate(___BtnWarp);
                marketSearchButton.name = "BtnMarketSearch";
                Destroy(marketSearchButton.transform.Find("Cost").gameObject);
                marketSearchButton.SetActive(true);
                marketSearchButton.GetComponentInChildren<Text>().text = Language.MarketSearch;
                marketSearchButton.SetActive(false);
                marketSearchButton.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
                Button.ButtonClickedEvent btnClickEvent = new Button.ButtonClickedEvent();
                btnClickEvent.AddListener(() => { MarketButtonClick(__instance); });
                marketSearchButton.GetComponentInChildren<Button>().onClick = btnClickEvent;
                marketSearchButton.transform.SetParent(questButton.transform.parent);
                marketSearchButton.layer = questButton.gameObject.layer;
                marketSearchButton.transform.localPosition = new Vector3(
                    ___sliderFactionInfluence.transform.localPosition.x - (___BtnWarp.GetComponentInChildren<RectTransform>().rect.x),
                    stationButton.transform.localPosition.y + (___BtnWarp.GetComponentInChildren<RectTransform>().rect.y * 1.1f),
                    stationButton.transform.localPosition.z
                    );
                marketSearchButton.transform.localScale = questButton.transform.localScale;
            }
            marketSearchButton.SetActive(__instance.gameObject.activeSelf);

            if (MarketSearch.marketSearchPanel == null)
            {
                MarketSearch.marketSearchPanel = GameObject.Instantiate(Assets.marketSearchPanel);
                MarketSearch.marketSearchPanel.transform.SetParent(questButton.transform.parent);
                MarketSearch.marketSearchPanel.layer = questButton.layer;
                MarketSearch.marketSearchPanel.transform.localPosition = new Vector3(
                    ___sliderFactionInfluence.transform.localPosition.x - (MarketSearch.marketSearchPanel.GetComponentInChildren<RectTransform>().rect.x),
                    marketSearchButton.transform.localPosition.y + MarketSearch.marketSearchPanel.GetComponentInChildren<RectTransform>().rect.y,
                    marketSearchButton.transform.localPosition.z
                    );
                MarketSearch.marketSearchPanel.transform.localScale = questButton.transform.localScale;
                MarketSearch.marketSearchPanel.SetActive(false);
            }
            if (MarketSearch.marketSearchPanel != null && MarketSearch.marketSearchPanel.activeSelf)
                MarketSearch.marketSearchPanel.SetActive(__instance.gameObject.activeSelf);
        }

        private static void MarketButtonClick(GalaxyMap galaxyMap)
        {
            MarketSearch.SetActive(true, galaxyMap);
        }

        private static void StationButtonClick()
        {
            factionButtons.ForEach(x => x.SetActive(true));
        }

        private static void FactionButtonClick(int faction)
        {
            if (GalaxyMap.instance == null)
                return;

            TSector curSector = GameData.data.GetCurrentSector();
            TSector closestSector = null;
            float distance = 0;

            for (int i = 0; i < GameData.data.sectors.Count; i++)
            {
                if (GameData.data.sectors[i].discovered &&
                    GameData.data.sectors[i] != curSector)
                {
                    Station station = null;
                    station = GameData.data.sectors[i].GetStation(faction, null, false, false);
                    if (station != null && station.discovered && !station.Destroyed)
                    {
                        float tempDist = Mathf.Abs(Vector2.Distance(curSector.realPosV2, GameData.data.sectors[i].realPosV2));
                        if (distance == 0 || tempDist < distance)
                        {
                            closestSector = GameData.data.sectors[i];
                            distance = tempDist;
                        }
                    }
                }
            }

            if (closestSector == null)
                InfoPanelControl.inst.ShowWarning(Language.NoStationFound, 1, false);
            else
                GalaxyMap.instance.MoveCameraTo(closestSector, true);

            factionButtons.ForEach(x => x.SetActive(false));
        }

        private static void RavagerBtnClick()
        {
            if (GalaxyMap.instance == null)
                return;

            TSector curSector = GameData.data.GetCurrentSector();
            TSector closestSector = null;
            float distance = 0;

            for (int i = 0; i < GameData.data.sectors.Count; i++)
            {
                if(GameData.data.sectors[i].discovered && 
                    GameData.data.sectors[i].boss != null && 
                    GameData.data.sectors[i].boss.alive &&
                    GameData.data.sectors[i] != curSector)
                {
                    float tempDist = Mathf.Abs(Vector2.Distance(curSector.realPosV2, GameData.data.sectors[i].realPosV2));
                    if(distance == 0 || tempDist < distance)
                    {
                        closestSector = GameData.data.sectors[i];
                        distance = tempDist;
                    }
                }
            }

            if (closestSector == null)
                InfoPanelControl.inst.ShowWarning(Language.NoRavagerFound, 1, false);
            else
                GalaxyMap.instance.MoveCameraTo(closestSector, true);
        }

        private static void QuestBtnClick()
        {
            if (GalaxyMap.instance == null)
                return;

            TSector curSector = GameData.data.GetCurrentSector();
            TSector closestQuestSector = null;
            float distance = 0;
            for (int i = 0; i < PChar.Char.activeQuests.Count; i++)
            {
                Quest q = PChar.Char.activeQuests[i];
                if (q.objectives.Length > 0)
                {
                    TSector questObjSector = GetQuestSector(q.objectives[0]);
                    if (questObjSector != null && questObjSector != curSector)
                    {
                        if (closestQuestSector == null)
                        {
                            closestQuestSector = questObjSector;
                            distance = Mathf.Abs(Vector2.Distance(curSector.realPosV2, questObjSector.realPosV2));
                        }
                        else
                        {
                            float tempDist = Mathf.Abs(Vector2.Distance(curSector.realPosV2, questObjSector.realPosV2));
                            if (tempDist < distance)
                            {
                                distance = tempDist;
                                closestQuestSector = questObjSector;
                            }
                        }
                    }
                }
            }

            if (closestQuestSector == null)
                InfoPanelControl.inst.ShowWarning(Language.NoQuestSectorFound, 1, false);
            else
                GalaxyMap.instance.MoveCameraTo(closestQuestSector, true);
        }

        private static TSector GetQuestSector(QuestObjective questObjective)
        {
            if (questObjective.type == QuestObjectiveType.GoToCoordenates)
                return GameData.data.sectors[questObjective.par1];
            else if (questObjective.type == QuestObjectiveType.GoToSector)
                return GameData.data.GetExistingSector(questObjective.par1, questObjective.par2);
            else if (questObjective.type == QuestObjectiveType.GoToStation)
                return GameData.data.sectors[GameData.GetStation(questObjective.par1, true).sectorIndex];
            else if (questObjective.type == QuestObjectiveType.Clear_Eliminate)
                return GameData.data.sectors[questObjective.par1];

            return null;
        }
    }
}
