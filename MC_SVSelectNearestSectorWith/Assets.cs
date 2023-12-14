using System.IO;
using UnityEngine;

namespace MC_SVSelectNearestSectorWith
{
    internal class Assets
    {
        internal static GameObject marketSearchPanel;
        internal static GameObject marketSearchResultItem;
    }

    internal class Language
    {
        internal static string NearestQuest = "Nearest Quest";
        internal static string NearestRavager = "Nearest Ravager";
        internal static string NearestStation = "Nearest Station";
        internal static string MarketSearch = "Market Search";
        internal static string ItemType = "Item Type:";
        internal static string Weapon = "Weapon";
        internal static string Equipment = "Equipment";
        internal static string TradeGood = "Trade Good";
        internal static string Ship = "Ship";
        internal static string SortBy = "Sort By:";
        internal static string Distance = "Distance";
        internal static string Rarity = "Rarity";
        internal static string Search = "Search";
        internal static string ItemName = "Item Name";
        internal static string Station = "Station";
        internal static string Sector = "Sector";
        internal static string Price = "Price";
        internal static string Dist = "Dist.";
        internal static string Close = "Close";
        internal static string NoStationFound = "No station found.";
        internal static string NoRavagerFound = "No ravager found.";
        internal static string NoQuestSectorFound = "No quest sector found.";
        internal static string InvalidSearchCriteria = "Invalid search criteria.";
        internal static string NoResultsFound = "No results found.";

        internal static void Load(string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    StreamReader sr = new StreamReader(file);
                    NearestQuest = sr.ReadLine();
                    NearestRavager = sr.ReadLine();
                    NearestStation = sr.ReadLine();
                    MarketSearch = sr.ReadLine();
                    ItemType = sr.ReadLine();
                    Weapon = sr.ReadLine();
                    Equipment = sr.ReadLine();
                    TradeGood = sr.ReadLine();
                    Ship = sr.ReadLine();
                    SortBy = sr.ReadLine();
                    Price = sr.ReadLine();
                    Distance = sr.ReadLine();
                    Rarity = sr.ReadLine();
                    Search = sr.ReadLine();
                    ItemName = sr.ReadLine();
                    Station = sr.ReadLine();
                    Sector = sr.ReadLine();
                    Dist = sr.ReadLine();
                    Close = sr.ReadLine();
                    NoStationFound = sr.ReadLine();
                    NoRavagerFound = sr.ReadLine();
                    NoQuestSectorFound = sr.ReadLine();
                    InvalidSearchCriteria = sr.ReadLine();
                    NoResultsFound = sr.ReadLine();
                }
            }
            catch
            {
                Main.log.LogError("Language load failed");
            }
        }
    }
}
