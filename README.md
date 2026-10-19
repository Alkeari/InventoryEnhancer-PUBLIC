# Inventory Enhancer

Sorting, filtering and search for Bannerlord's inventory screen, so managing hundreds of items stays fast. Thirteen sort categories, thirty-seven filters, and a search bar that works in the loot screens where the game normally disables it.

---

## Availability

- [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3790179835)
- [Nexus Mods](https://www.nexusmods.com/mountandblade2bannerlord/mods/9757)

## What It Does

- **One menu per side** - a Sort & Filter button beside each arrow around the character name opens that side's menu, so the two lists sort and filter independently.
- **See what is applied** - a gold dot on a side's button shows that side has a sort or filter applied.
- **Or both at once** - tick Both Sides and that side's sort and filter apply to both inventories, with the other side's button locked until you untick it.
- **Sort by what actually matters** - thirteen categories under General, Combat and Defense headers, ascending or descending.
- **Filter thirty-seven ways** - thirty-three category filters under nine headers, plus four modifier filters in their own section.
- **Weapons by how they are used** - Lance shows only couchable polearms and Spear every other polearm, Mace includes picks, and Two-Handed Axe includes mauls.
- **Every weapon type** - Dagger, One-Handed Sword, One-Handed Axe, Two-Handed Sword, Throwing Axe, Javelin, Throwing Knife, Stones, Bow, Crossbow, Arrows, Bolts and Shield sit beside them.
- **Filters that combine the way you expect** - categories widen the results, the four modifiers narrow whatever the categories matched:
  - **High Tier** 5 and up, **Low Tier** 2 and under.
  - **High Value** 1000 and up, **Low Value** under 100.
- **Filter by where it came from** - the culture filter separates Vlandian, Sturgian, Imperial, Battanian, Khuzait and Aserai gear.
- **Set it all up, then Apply** - selections are staged, so a sort and several filters land together instead of one at a time, and Reset clears a side's sort and filter at once.
- **Search everywhere** - the search bar works in the inventories the game normally disables it in, loot screens included, and matching is case-insensitive.
- **Keep your choices** - Remember Selections keeps your sort and filters active the next time you open an inventory.
- **Built into the screen** - the buttons sit in the inventory screen's own name bar, and the menu opens in the middle of the screen and closes from its Close button or a click outside it.
- **It says whether it loaded** - a green loaded line at game start, or a red line if it could not start.

---

## Requirements

- Mount & Blade II: Bannerlord v1.2.6 through v1.5.2.
- [Harmony](https://www.nexusmods.com/mountandblade2bannerlord/mods/2006) 2.4.2, [ButterLib](https://www.nexusmods.com/mountandblade2bannerlord/mods/2018) 2.11.1, [UIExtenderEx](https://www.nexusmods.com/mountandblade2bannerlord/mods/2102) 2.13.3 and [Mod Configuration Menu](https://www.nexusmods.com/mountandblade2bannerlord/mods/612) 5.12.2 or newer.

## Configuration

Everything is in the MCM options screen under Inventory Enhancer, and nothing needs a restart.

| Setting | What it does |
|---|---|
| Mod Enabled | the master toggle |
| Enable Logging | writes a daily log, `InventoryEnhancer<date>.log`, to `Documents\Mount and Blade II Bannerlord\Configs\ModLogs\` |
| Enable Search in All Inventories | turns the search bar on in the inventories the game disables it in |
| Case-Insensitive Search | matches regardless of capitalization |
| Enable Custom Sorting, Enable Custom Filtering | the sort half and the filter half of each side's menu, on or off |
| Remember Selections | keeps your sort and filters between inventory opens |

### Sort categories

Name, Tier, Damage, Accuracy, Reach, Missile Speed, Hit Points, Armor Total, Armor Highest, Speed, Handling, Weight, and Value per Weight.

### Filter headers

Melee One-Handed, Melee Two-Handed, Polearms, Thrown, Shields and Ranged, Armor, Mounts, Misc, and Culture, with the four modifier filters in their own section below.

## Compatibility

- **Save games.** Nothing is written to your save, so it can be added to or removed from a campaign at any time.
- **Game versions.** This release supports v1.2.6 through v1.5.2.
- Its changes run after other mods' changes to the same inventory screens, so it does not undo a filter another mod applied.
- **One copy only.** A `Modules\` copy overrides a Workshop copy silently, so if you have ever installed this mod the other way, delete `...\Modules\Inventory Enhancer` before using the store copy.

## Support

- Include the mod version, your Bannerlord version, your launcher's mod list in load order, and that day's `InventoryEnhancer<date>.log` from `Documents\Mount and Blade II Bannerlord\Configs\ModLogs\` with logging turned on.

## License

License terms are in the [Alkeari License Agreement](https://gist.github.com/Alkeari/2c6ec0cdf3dafee375b1a00b28ca190a).
