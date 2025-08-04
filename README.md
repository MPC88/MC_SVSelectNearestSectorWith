# MC_SVSelectNearestSectorWith  
  
Backup your save before using any mods.  
  
Uninstall any mods and attempt to replicate issues before reporting any suspected base game bugs on official channels.  
  
Install  
=======  
1. Install BepInEx - https://docs.bepinex.dev/articles/user_guide/installation/index.html Stable version 5.4.21 x86.  
2. Run the game at least once to initialise BepInEx and quit.  
3. Download latest mod release .zip archive.  
4. Extract all files (MC_SVSelectNearestSectorWith.dll, mc_svselectnearestsectorwith and MC_SVSelectNearestSectorWith.txt) into .\SteamLibrary\steamapps\common\Star Valor\BepInEx\plugins\  
  
Use / Configuration  
=====
New buttons at top-left of galaxy map.  All buttons ignore current sector.  "Nearest" is as the crow flies, not necessarilly number of warps required.  
  
- Nearest quest selects nearest sector with active quest.  
- Nearest ravager selects nearest sector with discovered and unkilled ravager.  
- Nearest station provides a new set of buttons to pick a faction and then selects nearest sector with discovered station of that faction.  
- Market search opens dialog to search for item types in all visited stations which have not been destroyed.  Results can be sorted by price or distance from current location.

After first run, a configuration file mc.starvalor.selectnearestsectorwith.cfg will be created in .\Star Valor\BepInEx\Config\.  This file has only one setting to modify:  
Partial max %  
This sets the maximum % explored a sector can be to be considered "partially" explored.  E.g. if set to 75, a sector 80% explored will be ignored.  
  
Language  
=====  
Language file is just line separated .txt.  Change to whatever you wish.  The only item you can't change is the market search window title.  
  
Language files can be found: https://github.com/MPC88/MC_SVSelectNearestSectorWith/tree/master/LanguageFiles  
  
Download the file (or copy paste content).  Make sure file is renamed to remove "LANGUAGE_" from the filename e.g: "ENGLISH_MC_SVSelectNearestSectorWithLang.txt" becomes "MC_SVSelectNearestSectorWithLang.txt".  
  
Russian translation by KelTuze.  
Spanish translation by KhalMika.  
