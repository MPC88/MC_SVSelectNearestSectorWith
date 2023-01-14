using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MC_SVSelectClosestSectorWith
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string pluginGuid = "mc.starvalor.selectclosestsectorwithquest";
        public const string pluginName = "SV Select Closest Sector with...";
        public const string pluginVersion = "1.0.2";

        private static GameObject questButton;
        private static GameObject ravagerButton;
        private static GameObject cotButton;

        public void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(Main));
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
                questButton.GetComponentInChildren<Text>().text = "Nearest Quest";
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
                ravagerButton.GetComponentInChildren<Text>().text = "Nearest Ravager";
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

            if (cotButton == null)
            {
                cotButton = Instantiate(___BtnWarp);
                cotButton.name = "BtnFindNearestCoT";
                Destroy(cotButton.transform.Find("Cost").gameObject);
                cotButton.SetActive(true);
                cotButton.GetComponentInChildren<Text>().text = "Nearest CoT Station";
                cotButton.SetActive(false);
                Button.ButtonClickedEvent btnClickEvent = new Button.ButtonClickedEvent();
                btnClickEvent.AddListener(new UnityAction(CotButtonClick));
                cotButton.GetComponentInChildren<Button>().onClick = btnClickEvent;
                cotButton.transform.SetParent(questButton.transform.parent);
                cotButton.layer = questButton.gameObject.layer;
                cotButton.transform.localPosition = new Vector3(
                    ___sliderFactionInfluence.transform.localPosition.x - (___BtnWarp.GetComponentInChildren<RectTransform>().rect.x),
                    ravagerButton.transform.localPosition.y + (___BtnWarp.GetComponentInChildren<RectTransform>().rect.y * 1.1f),
                    ravagerButton.transform.localPosition.z
                    );
                cotButton.transform.localScale = questButton.transform.localScale;
            }
            cotButton.SetActive(__instance.gameObject.activeSelf);
        }

        private static void CotButtonClick()
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
                    station = GameData.data.sectors[i].GetStation((int)TFaction.Rebels, null);
                    if (station != null && station.discovered && !station.destroyed)
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
                InfoPanelControl.inst.ShowWarning("No CoT station found.", 1, false);
            else
                GalaxyMap.instance.MoveCameraTo(closestSector, true);
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
                InfoPanelControl.inst.ShowWarning("No ravager found.", 1, false);
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
                InfoPanelControl.inst.ShowWarning("No quest sector found.", 1, false);
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
                return GameData.data.sectors[StationSystem.GetStation(questObjective.par1, true).sectorIndex];
            else if (questObjective.type == QuestObjectiveType.Clear_Eliminate)
                return GameData.data.sectors[questObjective.par1];

            return null;
        }
    }
}
