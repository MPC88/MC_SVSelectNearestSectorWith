using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.UI.Button;
using static UnityEngine.UI.Dropdown;

namespace MC_SVSelectNearestSectorWith
{
    internal class MarketSearch
    {
        private enum ItemType { none, weapon, equipment, generic, ship };
        private enum SortMethod { price, distance, rarity };

        private const int listItemSpacing = 14;
        private const int blueprintID = 54;

        internal static GameObject marketSearchPanel;

        private static Dropdown itemTypeDropdown;
        private static Dropdown sortMethodDropdown;
        private static InputField inputField;
        private static GameObject resultList;
        private static Button searchBtn;
        private static Button closeBtn;

        private static GalaxyMap galaxyMapInstance;
        private static ItemType selectedItemType = ItemType.weapon;
        private static SortMethod selectedSortMethod = SortMethod.price;
        private static GameObject goCurrentHighlight = null;
        private static List<ResultItem> results = new List<ResultItem>();

        internal static void SetActive(bool active, GalaxyMap galaxyMap)
        {
            if (marketSearchPanel == null)
            {
                Main.log.LogError("Null market serach panel.");
                return;
            }

            if (!itemTypeDropdown || !sortMethodDropdown || !inputField ||
                !resultList || !searchBtn || !closeBtn)
                InitPanel();

            if(galaxyMap != null)
                galaxyMapInstance = galaxyMap;
            marketSearchPanel.SetActive(active);

            if (active)
            {
                if (galaxyMapInstance == null)
                {
                    Main.log.LogError("Null galaxy map.");
                    return;
                }
                AccessTools.FieldRefAccess<bool>(typeof(GalaxyMap), "dragging")(galaxyMapInstance) = false;
                GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>().blockKeyboard = true;
                inputField.Select();
                if (results.Count > 0)
                    DisplayResults();
            }
            else
            {
                GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>().blockKeyboard = false;
            }
        }

        private static void InitPanel()
        {
            itemTypeDropdown = marketSearchPanel.transform.Find("mc_marketSearchType").GetComponent<Dropdown>();
            sortMethodDropdown = marketSearchPanel.transform.Find("mc_marketSearchSort").GetComponent<Dropdown>();
            inputField = marketSearchPanel.transform.Find("mc_marketSearchInput").GetComponent<InputField>();
            resultList = marketSearchPanel.transform.Find("mc_marketSearchResultList").GetChild(0).GetChild(0).gameObject;
            searchBtn = marketSearchPanel.transform.Find("mc_marketSearchSearch").GetComponent<Button>();
            searchBtn.GetComponentInChildren<Text>().text = Language.Search;
            closeBtn = marketSearchPanel.transform.Find("mc_marketSearchClose").GetComponent<Button>();
            closeBtn.GetComponentInChildren<Text>().text = Language.Close;

            DropdownEvent itemTypeSelectEvent = new DropdownEvent();
            itemTypeSelectEvent.AddListener(ItemTypeSelect);
            itemTypeDropdown.onValueChanged = itemTypeSelectEvent;
            itemTypeDropdown.options.Clear();
            itemTypeDropdown.options.Add(new Dropdown.OptionData(Language.Weapon));
            itemTypeDropdown.options.Add(new Dropdown.OptionData(Language.Equipment));
            itemTypeDropdown.options.Add(new Dropdown.OptionData(Language.TradeGood));
            itemTypeDropdown.options.Add(new Dropdown.OptionData(Language.Ship));
            itemTypeDropdown.value = Main.lastType.Value;

            DropdownEvent sortMethodSelectEvent = new DropdownEvent();
            sortMethodSelectEvent.AddListener(SortMethodSelect);
            sortMethodDropdown.onValueChanged = sortMethodSelectEvent;
            sortMethodDropdown.options.Clear();
            sortMethodDropdown.options.Add(new Dropdown.OptionData(Language.Price));
            sortMethodDropdown.options.Add(new Dropdown.OptionData(Language.Distance));
            sortMethodDropdown.options.Add(new Dropdown.OptionData(Language.Rarity));
            sortMethodDropdown.value = Main.lastSort.Value;

            ButtonClickedEvent searchButtonEvent = new ButtonClickedEvent();
            searchButtonEvent.AddListener(SearchButtonClick);
            searchBtn.onClick = searchButtonEvent;

            ButtonClickedEvent closeButtonEvent = new ButtonClickedEvent();
            closeButtonEvent.AddListener(CloseButtonClick);
            closeBtn.onClick = closeButtonEvent;
        }

        private static void CloseButtonClick()
        {
            SetActive(false, galaxyMapInstance);
        }

        private static void ItemTypeSelect(int value)
        {
            selectedItemType = (ItemType)value + 1;
        }

        private static void SortMethodSelect(int value)
        {
            selectedSortMethod = (SortMethod)value;
        }

        internal static void EnterPress()
        {
            if (marketSearchPanel != null &&
                marketSearchPanel.activeSelf &&
                EventSystem.current.currentSelectedGameObject == inputField.gameObject)
                SearchButtonClick();
        }

        internal static void EscapePress()
        {
            if (marketSearchPanel != null &&
                marketSearchPanel.activeSelf)
                CloseButtonClick();
        }

        private static void SearchButtonClick()
        {
            // Clear
            results.Clear();

            // Get target item(s)
            List<int> ids = GetItemIDs();
            if (ids == null || ids.Count == 0)
            {
                InfoPanelControl.inst.ShowWarning(Language.InvalidSearchCriteria, 1, false);
                return;
            }

            // Get results
            results = GetResults(ids);
            if (results == null || results.Count == 0)
            {
                InfoPanelControl.inst.ShowWarning(Language.NoResultsFound, 1, false);
                return;
            }

            // Sort results
            switch(selectedSortMethod)
            {
                case SortMethod.price:
                    results.Sort(ResultItem.ComparePrice);
                    break;
                case SortMethod.distance:
                    results.Sort(ResultItem.CompareDistance);
                    break;
                case SortMethod.rarity:
                    results.Sort(ResultItem.ComparRarity);
                    break;
            }

            // Save settings used
            Main.lastSort.Value = sortMethodDropdown.value;
            Main.lastType.Value = itemTypeDropdown.value;

            // Display results
            DisplayResults();
        }

        private static void DisplayResults()
        {
            DestroyAllChildren(resultList.transform);
            resultList.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, listItemSpacing * (results.Count + 2));
            for (int i = -1; i < results.Count; i++)
            {
                GameObject listItem = null;
                if (i == -1)
                    listItem = CreateResultListItem(null);
                else
                    listItem = CreateResultListItem(results[i]);

                listItem.transform.localPosition = new Vector3(
                    listItem.transform.localPosition.x + 5f,
                    -listItemSpacing * (i + 1),
                    listItem.transform.localPosition.z);
            }
            resultList.GetComponentInParent<ScrollRect>().verticalScrollbar.value = 1;
        }

        private static void ResultItemClick(PointerEventData pointerEventData, ResultItem item)
        {
            // Highlight toggle
            GameObject listItem = pointerEventData.pointerCurrentRaycast.gameObject.transform.parent.gameObject;
            if (goCurrentHighlight != null)
                goCurrentHighlight.SetActive(false);
            goCurrentHighlight = listItem.transform.Find("Highlight").gameObject;
            goCurrentHighlight.SetActive(true);

            // Move galaxy map
            GalaxyMap.instance.MoveCameraTo(GameData.data.sectors[item.sectorID], true);
        }

        private static void DestroyAllChildren(Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++)
                GameObject.Destroy(transform.GetChild(i).gameObject);
        }

        private static GameObject CreateResultListItem(ResultItem resultItem)
        {   
            // Instantiate
            GameObject resultListItem = GameObject.Instantiate(Assets.marketSearchResultItem);
            resultListItem.transform.SetParent(resultList.transform, false);
            resultListItem.transform.localScale = resultList.transform.localScale;
            resultListItem.layer = resultList.layer;
            resultListItem.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, resultList.GetComponent<RectTransform>().rect.width - 0.1f);

            // Populate
            if (resultItem == null)
            {
                resultListItem.transform.Find("ItemName").GetComponent<Text>().text = "<b>" + Language.ItemName + "</b>";
                resultListItem.transform.Find("Station").GetComponent<Text>().text = "<b>" + Language.Station + "</b>";
                resultListItem.transform.Find("Sector").GetComponent<Text>().text = "<b>" + Language.Sector + "</b>";
                resultListItem.transform.Find("Price").GetComponent<Text>().text = "<b>" + Language.Price + "</b>";
                resultListItem.transform.Find("Distance").GetComponent<Text>().text = "<b>" + Language.Dist + "</b>";
            }
            else
            {
                // ItemName
                resultListItem.transform.Find("ItemName").GetComponent<Text>().text = "";
                if (resultItem.blueprint)
                {
                    resultListItem.transform.Find("ItemName").GetComponent<Text>().text += ItemDB.GetRarityColor(resultItem.rarity) + "*</color>";
                }

                switch (selectedItemType)
                {
                    case ItemType.weapon:
                        resultListItem.transform.Find("ItemName").GetComponent<Text>().text += GameData.data.weaponList[resultItem.itemID].GetNameModified(resultItem.rarity, 0);
                        break;
                    case ItemType.equipment:
                        resultListItem.transform.Find("ItemName").GetComponent<Text>().text += EquipmentDB.GetEquipment(resultItem.itemID).GetNameModified(resultItem.rarity, 0);
                        break;
                    case ItemType.generic:
                        resultListItem.transform.Find("ItemName").GetComponent<Text>().text += ItemDB.GetItemNameModified(resultItem.itemID, 0);
                        break;
                    case ItemType.ship:
                        resultListItem.transform.Find("ItemName").GetComponent<Text>().text += ShipDB.GetModel(resultItem.itemID).GetModelNameModified(0).Replace("<size=14>", "").Replace("</size>", "");
                        break;
                }

                //Station
                resultListItem.transform.Find("Station").GetComponent<Text>().text = resultItem.station;
                //Sector
                resultListItem.transform.Find("Sector").GetComponent<Text>().text = GameData.data.sectors[resultItem.sectorID].coords;
                //Price
                resultListItem.transform.Find("Price").GetComponent<Text>().text = GetPriceString(resultItem.price, (int)selectedItemType, resultItem.itemID);
                //Distance
                resultListItem.transform.Find("Distance").GetComponent<Text>().text = resultItem.distance.ToString("N2");

                // Click Event
                EventTrigger.Entry newItemTrig = new EventTrigger.Entry();
                newItemTrig.eventID = EventTriggerType.PointerDown;
                newItemTrig.callback.AddListener((data) => { ResultItemClick((PointerEventData)data, resultItem); });
                resultListItem.GetComponent<EventTrigger>().triggers.Add(newItemTrig);
            }
            
            return resultListItem;
        }

        private static List<int> GetItemIDs()
        {
            string searchText = inputField.text.ToLower().Trim();

            if (searchText.Length == 0)
                return null;

            List<int> ids = new List<int>();

            switch (selectedItemType)
            {
                case ItemType.weapon:
                    foreach (TWeapon weapon in GameData.data.weaponList)
                        if (weapon.name.ToLower().Trim().Contains(searchText))
                            ids.Add(weapon.index);
                    break;
                case ItemType.equipment:
                    foreach (Equipment equipment in AccessTools.StaticFieldRefAccess<List<Equipment>>(typeof(EquipmentDB), "equipments"))
                        if (equipment.equipName.ToLower().Trim().Contains(searchText))
                            ids.Add(equipment.id);
                    break;
                case ItemType.generic:
                    foreach (Item item in AccessTools.StaticFieldRefAccess<List<Item>>(typeof(ItemDB), "items"))
                        if (item.itemName.ToLower().Trim().Contains(searchText))
                            ids.Add(item.id);
                    break;
                case ItemType.ship:
                    foreach (ShipModelData ship in AccessTools.StaticFieldRefAccess<List<ShipModelData>>(typeof(ShipDB), "shipModels"))
                        if (ship.modelName.ToLower().Trim().Contains(searchText))
                            ids.Add(ship.id);
                    break;
            }

            return ids;
        }

        private static List<ResultItem> GetResults(List<int> ids)
        {
            List<ResultItem> results = new List<ResultItem>();
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
                return null;
            
            SpaceShip playerSS = player.GetComponent<SpaceShip>();

            foreach(Station station in GameData.data.stationList)
            {
                if (station.discovered && !station.Destroyed && station.GetMarketList != null)
                {
                    foreach (MarketItem marketItem in station.GetMarketList)
                    {
                        if (marketItem.InStock)
                        {
                            if(marketItem.itemType == (int)selectedItemType &&
                             ids.Contains(marketItem.itemID))
                            {
                                results.Add(new ResultItem()
                                {
                                    itemID = marketItem.itemID,
                                    station = station.stationName(false),
                                    price = GetPrice(station, marketItem, playerSS),
                                    rarity = marketItem.rarity,
                                    sectorID = station.sectorIndex,
                                    distance = Mathf.Abs(Vector2.Distance(GameData.data.sectors[GameData.data.currentSectorIndex].realPosV2, GameData.data.sectors[station.sectorIndex].realPosV2)),
                                    blueprint = false
                                });
                            }
                            else if (marketItem.itemType == (int)ItemType.generic &&
                              marketItem.itemID == 54 &&
                              marketItem.extraData is CI_Data_Blueprint &&
                              marketItem.extraData.genItem.itemType == (int)selectedItemType &&
                              ids.Contains(marketItem.extraData.genItem.itemID))
                            {
                                results.Add(new ResultItem()
                                {
                                    itemID = marketItem.extraData.genItem.itemID,
                                    station = station.stationName(false),
                                    price = GetPrice(station, marketItem, playerSS),
                                    rarity = marketItem.extraData.genItem.rarity,
                                    sectorID = station.sectorIndex,
                                    distance = Mathf.Abs(Vector2.Distance(GameData.data.sectors[GameData.data.currentSectorIndex].realPosV2, GameData.data.sectors[station.sectorIndex].realPosV2)),
                                    blueprint = true
                                });
                            }
                        }
                    }
                }
            }

            return results;
        }

        private static float GetPrice(Station station, MarketItem marketItem, SpaceShip playerSS)
        {
            GenericCargoItem genericCargoItem = new GenericCargoItem(marketItem.itemType, marketItem.itemID, marketItem.rarity, station, null, null, marketItem.extraData);
            return MarketSystem.GetTradeModifier(genericCargoItem.unitPrice, marketItem.itemType, marketItem.itemID, false, station.factionIndex, playerSS);
        }

        private static string GetPriceString(float price, int itemType, int itemID)
        {
            if (itemType == (int)ItemType.generic)
                return MarketSystem.GetSimplePriceString2(price, itemID);
            else if (itemType == (int)ItemType.ship)
                return MarketSystem.GetSimplePriceString(price, true);
            else
                return MarketSystem.GetSimplePriceString(price, false);
        }

        private class ResultItem
        {
            internal int itemID;
            internal string station;
            internal float price;
            internal int rarity;
            internal int sectorID;
            internal float distance;
            internal bool blueprint;

            internal static int ComparePrice(ResultItem x, ResultItem y)
            {
                if (x.price > y.price)
                    return 1;
                else if (x.price < y.price)
                    return -1;
                else
                    return 0;
            }

            internal static int CompareDistance(ResultItem x, ResultItem y)
            {
                if (x.distance > y.distance)
                    return 1;
                else if (x.distance < y.distance)
                    return -1;
                else
                    return 0;
            }

            internal static int ComparRarity(ResultItem x, ResultItem y)
            {
                if (x.rarity < y.rarity)
                    return 1;
                else if (x.rarity > y.rarity)
                    return -1;
                else
                    return 0;
            }
        }
    }
}
