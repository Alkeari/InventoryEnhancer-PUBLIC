# Inventory Enhancer

Sorting, filtering and search for Bannerlord's inventory screen, so managing hundreds of items stays fast. Thirteen sort categories, thirty-four filters, and a search bar that works in the loot screens where the game normally disables it.

---

## Availability

- [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3790179835)
- [Nexus Mods](https://www.nexusmods.com/mountandblade2bannerlord/mods/9757)

## What It Does

- **Sort by what actually matters** - thirteen categories under General, Combat and Defense headers, ascending or descending, applied to the left inventory, the right, or both.
- **Filter thirty-four ways** - thirty category filters under seven headers, plus four modifier filters in their own section.
- **Filters that combine the way you expect** - categories widen the results, the four modifiers narrow whatever the categories matched:
  - **High Tier** 5 and up, **Low Tier** 2 and under.
  - **High Value** 1000 and up, **Low Value** under 100.
- **Filter by where it came from** - the culture filter separates Vlandian, Sturgian, Imperial, Battanian, Khuzait and Aserai gear.
- **Set it all up, then Apply** - selections are staged, so a sort and several filters land together instead of one at a time.
- **Search everywhere** - the search bar works in the inventories the game normally disables it in, loot screens included, and matching is case-insensitive.
- **Keep your choices** - Remember Selections keeps your sort and filters active the next time you open an inventory.
- **Get out of the way** - Hide collapses the overlay, Reset clears every sort and filter at once.
- **It says when something fails** - an in-game message instead of a silent no-op, and a green loaded line at game start.

---

## Requirements

- Mount & Blade II: Bannerlord v1.3.13 through v1.5.2.
- [Harmony](https://www.nexusmods.com/mountandblade2bannerlord/mods/2006) 2.4.2, [ButterLib](https://www.nexusmods.com/mountandblade2bannerlord/mods/2018) 2.11.1, [UIExtenderEx](https://www.nexusmods.com/mountandblade2bannerlord/mods/2102) 2.13.3 and [Mod Configuration Menu](https://www.nexusmods.com/mountandblade2bannerlord/mods/612) 5.12.2 or newer.

## Configuration

Everything is in the MCM options screen under Inventory Enhancer, and nothing needs a restart.

| Setting | What it does |
|---|---|
| Enable Mod | the master toggle |
| Enable Logging | writes to `Documents\Mount and Blade II Bannerlord\Configs\ModLogs\Inventory Enhancer.log` |
| Search Everywhere | turns the search bar on in the inventories the game disables it in |
| Case-Insensitive Search | matches regardless of capitalization |
| Custom Sorting, Custom Filtering | each half of the overlay, on or off |
| Remember Selections | keeps your sort and filters between inventory opens |

### Sort categories

Name, Tier, Damage, Accuracy, Reach, Missile Speed, Hit Points, Armor Total, Armor Highest, Speed, Handling, Weight, and Value per Weight.

### Filter headers

Melee One-Handed, Melee Two-Handed, Shields and Ranged, Armor, Mounts, Misc, and Culture, with the four modifier filters in their own section below.

## Compatibility

- **Save games.** Nothing is written to your save, so it can be added to or removed from a campaign at any time.
- **Game versions.** This release supports v1.3.13 through v1.5.2.
- Its changes run after other mods' changes to the same inventory screens, so it does not undo a filter another mod applied.
- **One copy only.** A `Modules\` copy overrides a Workshop copy silently, so if you have ever installed this mod the other way, delete `...\Modules\Inventory Enhancer` before using the store copy.

## Support

- Include the mod version, your Bannerlord version, your launcher's mod list in load order, and `Documents\Mount and Blade II Bannerlord\Configs\ModLogs\Inventory Enhancer.log` with logging turned on.

## License

License terms are in the [Alkeari License Agreement](https://gist.github.com/Alkeari/2c6ec0cdf3dafee375b1a00b28ca190a).
