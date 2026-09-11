# Changelog

What changed in Inventory Enhancer, newest first. Only changes you can actually notice are listed:
internal and build work is deliberately left out. This file is the source for the release
notes published on the Nexus Mods page.

Entries begin on 2026-08-14, when this file was created. This repository's earlier history
was intentionally cleared, so there is nothing before that to record.

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
