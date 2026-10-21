# Settlement Trade Overview

*Know what settlements are selling before you make the trip.*

Settlement Trade Overview lets you inspect the current trade stock offered by world settlements before deciding where to send your caravan. Browse settlements together in one overview or inspect a selected settlement on the world map.

Purchases still use RimWorld's normal caravan and trading systems. The package includes separate assemblies for RimWorld 1.6 and 1.7; current development targets 1.7. The main manual gameplay checks on 1.7 passed without errors; checks requiring a diagnostic environment remain unverified.

## Features

- Search offers by item, pawn or settlement and filter them by category.
- Sort offers by item, details, count, settlement, price, distance or restock information.
- See estimated purchase prices using an available negotiator, or market value when no negotiator is available.
- Check distance separately from ground-route availability.
- Inspect pawn trade details and restock information.
- Choose which settlements appear and refresh their displayed trade data.
- Optionally highlight genepacks relevant to unfinished Xenogerm Planner plans.

## Usage

1. Load a colony and open **trade overview** from the main tabs. For a single settlement, select it on the world map and choose **View trade stock**.
2. Use search, categories and column headers to narrow down the offers.
3. Check the price, distance and restock information, then visit the settlement through the normal caravan system.
4. Use **Refresh** to read the current settlement stock again.

The active colony map is the origin for distance and negotiator selection. If no offers appear, check the settlement filters below. With the default settings, a powered communications console is required.

## Trade data and refresh behavior

Opening the overview reads actual settlement stock. RimWorld may generate stock that has not been generated yet as part of that read. Initial loading or refreshing many settlements can therefore cause a temporary pause.

**Refresh** rebuilds the displayed information. It does not force a restock or change RimWorld's restock schedule. Reopening a compatible overview can reuse its previous stock snapshot; use Refresh when you need new data. Restock times remain relative to the last refresh rather than counting down continuously.

Distances are approximate world-tile distances from the active map. A settlement can have a distance even when no ground route exists. On 1.7, between different world layers, distance is unavailable and no ground route is offered; the distance and reachable-route filters exclude these settlements when enabled.

## Xenogerm Planner integration

With [Xenogerm Planner](https://steamcommunity.com/sharedfiles/filedetails/?id=3781523927) and a compatible Planner API, genepacks can show a relevance indicator in the **Details** column. Hover it to see matching unfinished plans.

Offers remain suggestions and do not count toward plan readiness until acquired. Reopen the overview after changing a plan to update relevance. The integration is optional; unavailable or incompatible Planner versions leave ordinary stock browsing available.

## Settings

Open RimWorld's mod settings and select **Settlement Trade Overview**.

| Setting | Default and effect |
| --- | --- |
| Show global trade overview tab | On. Hiding it keeps settlement-specific commands available. |
| Require a powered communications console | On. Accepts a powered console on the current map or another player home map. |
| Require Industrial technology or higher | On. Filters by the settlement faction's technology level. |
| Limit maximum distance | On, with a limit of 40 world tiles. The limit can be set up to 3000. |
| Require a reachable route | On. Requires a traversable ground route from the active map. |
| Require Royalty trade permission | Off. When enabled with Royalty, requires an available colonist to meet the settlement's royal title requirement. |

Settings are saved in RimWorld's mod configuration. Applying eligibility changes invalidates the displayed stock cache. The Royalty setting has no effect when that DLC is inactive.

## Requirements

- RimWorld 1.6 or 1.7. The package selects the assembly for the running game version.
- No DLC or Harmony required.

Royalty-specific filtering and Biotech genepack information are available when their content is active. Xenogerm Planner is optional.

## Installation

The published mod is available through [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3781528628). Check the supported game version on the item before installing: preparing this 1.7 checkout does not publish an update to Workshop.

Subscribe to the appropriate version, let Steam download it, then enable **Settlement Trade Overview** in RimWorld's Mods menu and restart as requested. Enable optional DLC or Planner only when using their features.

For a development installation, prepare the complete `mod/` folder with the `PackageMod` command under [Build](#build), then install that folder's contents together. It includes `LoadFolders.xml`, shared content and a separate DLL under each of `1.6/Assemblies/` and `1.7/Assemblies/`. Do not install the source checkout itself or copy the game's reference DLLs into the mod.

To update a local installation, close RimWorld and replace the previous package with the complete new package in a clean mod directory. Old DLLs under the package root's `Assemblies/` directory, including `Assemblies/Current/`, must not remain alongside the versioned assemblies. Workshop manages updates to its installed copy. To remove the mod, disable it in the Mods menu, restart, and remove or unsubscribe from that copy. See the save-related limits below before changing an existing game.

## Compatibility

The release package supports RimWorld 1.6 and 1.7 through separate version-specific assemblies. The historical 1.6 DLL is preserved unchanged inside the same package; current sources and builds target 1.7. `LoadFolders.xml` selects shared content and the matching version directory. Updates to the 1.7 implementation do not rebuild or replace the 1.6 artifact.

The 1.7 manual test result applies to the tested environment. It does not separately establish every DLC, mod or save configuration; see [Testing policy](docs/testing.md) for the evidence boundary and outstanding checks. Adding the mod to an existing 1.7 save and removing it from a save remain separately unverified. The mod saves its settings separately and does not persist its stock snapshots, but those implementation facts alone do not establish save compatibility. No general guarantee is made for migrating a 1.6 save to the new engine or for multiplayer.

Ordinary goods and genes are handled through RimWorld's trading systems. Mods that replace settlement definitions, stock handling or trade grouping may need separate compatibility work. Independently defined settlement-like world objects may not receive **View trade stock** automatically.

## Languages

- English
- Russian
- Ukrainian
- Spanish — provided by Annabelesca
- Latin American Spanish — provided by Annabelesca

All five translations are included. The main-scenario runtime report does not record separate layout and fallback results for each language on 1.7.

## Development

### Prerequisites

- A local RimWorld 1.7 installation with `Assembly-CSharp.dll` and `UnityStubs.dll`.
- A .NET SDK/MSBuild installation and reference assemblies capable of targeting .NET Framework 4.7.2 (`net472`).
- The `Escarval.RimWorld.UI` source checkout.

Shared UI source is compiled into `SettlementTradeOverview.dll`. Players do not install a separate UI framework DLL or mod. STO does not build the supplier's standalone project as a prerequisite.

### Local configuration

Copy [the configuration example](docs/SettlementTradeOverview.Local.props.example) to `src/SettlementTradeOverview.Local.props`, which is ignored by Git.

- Set `RimWorldManagedDir` to the 1.7 installation directory containing `Assembly-CSharp.dll` and `UnityStubs.dll`. The property name is retained; the old `RimWorldWin64_Data/Managed` directory is not the 1.7 location.
- Set `EscarvalRimWorldUiProjectDir` to the supplier directory containing its project and `Source/` if it is not found automatically. The build checks a sibling checkout and the shared checkout at the RimWorld workspace level.
- Leave `DeployToGame` false for ordinary development. Configure `RimWorldModAssembliesDir` only when opting into deployment; it must name the `1.7/Assemblies/` directory of the intended development installation.

### Build

From the repository root:

```text
dotnet build src/SettlementTradeOverview.sln -c Release
```

The current production DLL is written under `src/SettlementTradeOverview/bin/Release/`. Ordinary builds do not prepare the package or copy files into the game.

To prepare the complete package from the production project:

```text
dotnet build src/SettlementTradeOverview/SettlementTradeOverview.csproj -c Release -t:PackageMod
```

This builds the current 1.7 DLL and copies it to `mod/1.7/Assemblies/`. The target verifies the preserved 1.6 DLL's checksum and rejects unversioned DLLs under `mod/Assemblies/`. The resulting `mod/` folder contains the complete package for both game versions; it is not installed or published by this command with the default deployment setting.

```text
mod/
  About/
  LoadFolders.xml
  Defs/, Patches/, Languages/, Textures/       Shared content
  1.6/Assemblies/SettlementTradeOverview.dll   Preserved historical artifact
  1.7/Assemblies/SettlementTradeOverview.dll   Current build
```

The historical DLL is versioned in Git and must not be rebuilt from current sources. The generated 1.7 directory is ignored by Git and populated by `PackageMod`.

After installing the complete package in a clean directory, deploy subsequent 1.7 DLL builds to the configured development installation explicitly:

```text
dotnet build src/SettlementTradeOverview.sln -c Release -p:DeployToGame=true
```

This copies only the current `SettlementTradeOverview.dll` to `RimWorldModAssembliesDir`. The destination must end in `1.7/Assemblies/`; deployment also rejects installations that still contain unversioned DLLs under the package root's `Assemblies/` directory. It does not synchronize metadata, shared content or the historical DLL, prepare a complete package, or publish a release.

### Tests

```text
dotnet test tests/SettlementTradeOverview.Tests/SettlementTradeOverview.Tests.csproj -c Release
```

The test command builds its production dependency. With the default `DeployToGame=false`, neither command deploys files to the game; enabling that property also enables deployment through dependent builds.

The NUnit suite protects project-owned query, snapshot, eligibility, cache, settings, presentation and optional-integration contracts. Generic shared UI tests belong to the supplier's separate suite. See [Testing policy](docs/testing.md) for test boundaries, the reported runtime results and outstanding checks.

## Repository structure

```text
assets/   Source visual assets
docs/     Architecture, test policy and public configuration example
mod/      RimWorld package content
src/      Production source and solution
tests/    NUnit test project
```

## Technical documentation

- [Architecture](docs/architecture.md)
- [Testing policy](docs/testing.md)

## Links

- [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3781528628)
- [Source repository](https://github.com/EscarvalOokt/SettlementTradeOverview)