# Changelog

What changed in Inventory Enhancer, newest first. Only changes you can actually notice are listed:
internal and build work is deliberately left out. This file is the source for the release
notes published on the Nexus Mods page.

Entries begin on 2026-08-14, when this file was created. This repository's earlier history
was intentionally cleared, so there is nothing before that to record.

## v4.0.0 - 2026-09-14

- Added: A Sort & Filter button beside each arrow around the character name opens a sort and filter menu for that side's inventory alone.
- Added: A Both Sides option applies one side's sort and filter to both inventories and locks the other side's button until you untick it.
- Added: A gold dot on each side's button shows when that side has a sort or filter applied.
- Added: A Close button, and clicking outside the menu, close it without applying your changes.
- Changed: The menu is redesigned with a dark panel, gold headings, highlighted selections and an Ascending and Descending toggle, and opens in the middle of the screen.
- Changed: Weapon filters now go by how a weapon is used: Lance shows only couchable polearms, Spear every other polearm, Mace includes picks and Two-Handed Axe includes mauls.
- Changed: Throwing Weapons and Ammunition are split into Throwing Axe, Javelin, Throwing Knife, Stones, Arrows and Bolts.
- Changed: Reset clears only the side whose menu it is in.
- Changed: Supported game versions are now v1.2.6 through v1.5.2, and both downloads run every one of them.
- Removed: The floating Sort, Filter and Hide buttons, replaced by the per-side buttons.

## v3.1.0 - 2026-08-28

- Added: Support for the game's Beta branch v1.5.2, as a separate BETA download on Nexus Mods.
- Added: Support for game versions v1.3.13 through v1.3.15. The supported range is now v1.3.13
  through v1.4.8 (plus Beta v1.5.2), verified by real compilation and API checks against each
  game version rather than assumption.
- Changed: The main Nexus Mods download is now the file group named simply "Inventory Enhancer"
  (previously "Inventory Enhancer - v1.4.X"); it always carries the newest release for the game's
  current Stable version range.

## v3.0.0 - 2026-08-25

- Fixed: Sorting, filtering, and search had stopped working entirely on game version 1.4.8; all three work again.
- Fixed: Selection checkboxes on sort and filter options were invisible; they now show correctly.
- Fixed: The Filter button shifted out of place when custom sorting was disabled.
- Fixed: Sort and filter failures now show an in-game message instead of failing silently.
- Added: A culture filter (Vlandian, Sturgian, Imperial, Battanian, Khuzait, Aserai).
- Changed: Sort options are now grouped (General, Combat, Defense) instead of one long list.
- Changed: Filter options are now grouped under icon headers, matching the game's own filter tabs.
- Changed: Filter labels show their numeric thresholds directly (e.g. "High Tier (5+)").
