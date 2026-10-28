# Settlement Trade Overview architecture

Settlement Trade Overview reads settlement stock into immutable snapshots and presents it in a global overview and a settlement-specific window. This document describes the current source, which targets RimWorld 1.7.

User instructions and development commands are in [README](../README.md). Test procedures and acceptance scenarios are in [Testing policy](testing.md).

## Runtime and package

The development target is RimWorld 1.7 on the Waylith engine. The mod assembly targets `net10.0`, compiled against the installed `Assembly-CSharp.dll` and `UnityStubs.dll`. The latter provides the Unity-named compatibility types used by the mod and its shared UI source. Automated tests run on the standard .NET 10 runtime; compatibility with the game's Waylith runtime requires in-game acceptance.

The repository's `mod/` directory contains shared content and preserved historical DLLs, including `1.6/Assemblies/SettlementTradeOverview.dll`. Current sources build the RimWorld 1.7 DLL into `bin/`. Explicit deployment assembles the complete installed mod with the current DLL under `Current/Assemblies/`; no current binary is staged in the repository. `LoadFolders.xml` selects the shared root plus `1.6` on RimWorld 1.6, or the shared root plus `Current` on 1.7.

[Deployment targets](../build/ModDeployment.targets) run only with `DeployToGame=true`. They validate the destination, package identity, load mapping and historical checksums before replacing managed content directories. Shared metadata, Defs, translations and textures are deployed with the assemblies, and obsolete unversioned/current-numbered assembly layouts are removed. Files outside managed directories and a source-absent local `About/PublishedFileId.txt` are preserved. Ordinary builds and tests leave the source package and installation unchanged. The deployed directory is the complete package; deployment to a clean external staging directory prepares a release archive. See README for commands and the cleanup boundary.

The package ID is `escarval.settlementtradeoverview`; the assembly and root namespace are `SettlementTradeOverview`. No Harmony patches or required runtime framework mod are used. Royalty and Biotech are optional content boundaries, and Xenogerm Planner is an optional integration.

The production project imports `Escarval.RimWorld.UI.Source.projitems` from a local copy of the shared UI source, obtained from a release archive or Git checkout. The import defines the source files, excludes the library's `bin/` and `obj/` directories, and displays external sources under `SharedUI` in the project view. STO compiles those files into its own assembly using its game references and compiler settings. The library's standalone project and tests remain separate: STO has no production project reference or extra UI DLL dependency. The required release and source setup are documented in [README](../README.md#shared-ui-source). The build uses the configured local files without downloading or checking their release version.

## Implementation map

- [Mod entry point](../src/SettlementTradeOverview/SettlementTradeOverviewMod.cs), [snapshot service](../src/SettlementTradeOverview/Application/Snapshots/TradeInventorySnapshotService.cs) and [game lifecycle component](../src/SettlementTradeOverview/Integration/Lifecycle/GameComponent_SettlementTradeOverview.cs) own initialization and application state.
- [Mod metadata](../mod/About/About.xml), [load-folder mapping](../mod/LoadFolders.xml), [main-button Def](../mod/Defs/MainButtonDefs/STO_GlobalOverview.xml) and [settlement patch](../mod/Patches/STO_SettlementTradeOverview.xml) define declared support, version selection and the packaged entry points.
- [Production project](../src/SettlementTradeOverview/SettlementTradeOverview.csproj) and [test project](../tests/SettlementTradeOverview.Tests/SettlementTradeOverview.Tests.csproj) define compilation, references and build actions.
- [Planner API binding](../src/SettlementTradeOverview/Integration/Planner/XenogermPlannerApiV1Binding.cs) defines the exact optional public API shape accepted by STO. Planner remains the owner of relevance semantics.

## Runtime flow

1. `SettlementTradeOverviewMod` loads and normalizes settings, applies the settings service and schedules main-button visibility after Def loading.
2. The main-button Def opens `MainTabWindow_SettlementTradeOverview`. Alternatively, the settlement XML patch adds a stateless `WorldObjectComp`, whose native gizmo delegates to the shared eligibility and command policy before opening `Dialog_SettlementStock`.
3. A window first asks the snapshot service for a compatible loaded snapshot. Otherwise it displays loading state, waits for a repaint and performs the requested synchronous capture from `WindowUpdate`.
4. The context adapter takes the current map as origin, retains its full `PlanetTile`, collects its free colonists, and records powered-console availability and whether the Royalty DLC is active. No active map produces an unavailable context.
5. Discovery applies settlement eligibility. The trader adapter reads public stock once per captured trader, groups goods through the vanilla trade boundary and immediately converts them to project-owned values.
6. The service owns snapshot/cache state. Query code produces the search/category/sort projection; the common `TradeListView` renders both windows.
7. Optional Planner relevance is projected from copied genepack compositions. Reopening a compatible window recomputes relevance without requiring a new stock read.
8. Manual refresh, relevant settings changes and new-game/load callbacks invalidate derived state and transient item targets.

## Settlement eligibility and commands

[Eligibility adapters](../src/SettlementTradeOverview/Integration/Eligibility/SettlementEligibilityFactsAdapter.cs) collect facts; [SettlementEligibilityPolicy](../src/SettlementTradeOverview/Domain/Eligibility/SettlementEligibilityPolicy.cs) decides eligibility. The same policy serves discovery and the settlement command.

Filters cover trade availability, faction hostility, faction technology, powered communications access, maximum distance, ground reachability and optional Royalty trade permission. A powered console on the current map or another `IsPlayerHome` map satisfies communications access; an arbitrary non-home map does not.

[WorldObjectComp_SettlementTradeOverview](../src/SettlementTradeOverview/Integration/Settlements/WorldObjectComp_SettlementTradeOverview.cs) is stateless. It returns an enabled command for an eligible settlement, a disabled command with a reason for a supported potential trader that fails eligibility, or no command for unsupported/player-owned objects. It does not read stock or own cache state.

The XML patch targets the existing `comps` list of the vanilla `Settlement` Def. XML descendants that retain that list inherit the component. Independently defined settlement-like objects receive the command only if their Def includes the STO component; deriving their runtime class from `Settlement` does not add it.

## Stock access and snapshots

RimWorld owns settlement stock through `Settlement_TraderTracker`. Its public `ITrader.Goods` access can lazily generate stock when the container is absent or empty. Expired stock is destroyed by the tracker; a later read generates replacement stock. `CanTradeNow` can be true before stock is generated.

[TraderGoodsCaptureAdapter](../src/SettlementTradeOverview/Integration/Snapshots/TraderGoodsCaptureAdapter.cs) performs one public capture per trader snapshot build. It groups trader-side goods using the applicable `TransferAsOne` mode and converts them synchronously through [TradeEntrySnapshotAdapter](../src/SettlementTradeOverview/Integration/Snapshots/TradeEntrySnapshotAdapter.cs).

Snapshots retain identities, labels, counts, categories, price state, pawn details, restock values and optional gene composition. They do not retain live `Thing`, `Pawn`, `Tradeable`, `GeneDef`, `GeneSet` or vanilla collections.

For supported genepack rows, grouping compares each candidate against `Tradeable.AnyThing`; matching gene sets and labels are required in addition to generic stacking rules. One representative therefore supplies the row's composition. `GenepackCompositionSnapshot` stores distinct non-empty gene definition names in ordinal order. Equivalent compositions may remain separate rows. Unavailable or empty composition metadata leaves the ordinary row usable without Planner relevance.

The public stock read can change vanilla state through lazy generation. STO's read-only boundary means no purchases, direct stock mutation, private tracker reflection, forced regeneration, schedule changes or lifecycle patches. Manual refresh repeats public capture; it is not immediate restocking. Initial capture remains synchronous and can cause temporary generation hitches.

## Cache and runtime targets

[TradeInventorySnapshotCache](../src/SettlementTradeOverview/Cache/TradeInventorySnapshotCache.cs) exposes `NotLoaded`, `Loading`, `Available`, `Empty`, `Unavailable`, `Partial` and `Failed` states. Per-trader failures must not discard successfully captured traders.

The lightweight reuse key includes context availability, active map, origin tile and layer, selected negotiator, and eligibility criteria. It also includes powered-console availability when the communications filter is enabled, and `IsRoyaltyActive` when the Royalty trade-permission filter is enabled. `IsRoyaltyActive` records whether the DLC is active, not whether a colonist currently has a qualifying title. The full key additionally includes ordered trader identities and discovery failure state. Planner state does not participate in either key.

The reuse key does not include the full colonist roster or their royal titles. Gaining or losing a qualifying title, or a qualifying colonist arriving at or leaving the active map, does not by itself prevent reuse if the selected negotiator and the other key values remain unchanged. Reopening can therefore retain the previous settlement list. Use Refresh to rerun discovery and evaluate Royalty trade permission against the current free colonists and their titles on the active map.

Before reopen reuse, the service verifies that cached settlements can still be resolved. A settlement-specific view can reuse the shared snapshot when its target is present; otherwise it follows full discovery. Reuse keeps the captured stock values; Refresh reads current stock and rebuilds the display.

[TradeEntryRuntimeTargetCache](../src/SettlementTradeOverview/Integration/Runtime/TradeEntryRuntimeTargetCache.cs) retains representative live items only inside the integration boundary for the lifetime of their snapshot. This supports instance icons, native tooltips and info cards. Missing/destroyed targets degrade to a safe definition-based presentation. Rebuild/invalidation clears the targets; neither immutable snapshots nor row presentation own them.

## Pricing, distance and restock

[TradeNegotiatorAdapter](../src/SettlementTradeOverview/Integration/Snapshots/TradeNegotiatorAdapter.cs) chooses an eligible free colonist on the active map by `TradePriceImprovement`, with a deterministic pawn-ID tie break. Dead, destroyed, non-colonist and Social-disabled candidates are excluded.

[TradePriceAdapter](../src/SettlementTradeOverview/Integration/Snapshots/TradePriceAdapter.cs) obtains the estimated player-buy price using a bounded trade-session scope and restores the previous negotiator, trader and gift mode. Without a negotiator, it uses valid instance or definition market value. Failed negotiated calculations and invalid values produce unavailable price state rather than a fabricated estimate.

Distance and routes use complete `PlanetTile` endpoints. On one layer, approximate distance and traversable ground-route state are distinct: a route failure can coexist with a numeric distance. Between layers, STO has no comparable approximate-distance result and reports no ground route. It does not reinterpret a layer-local tile ID as a surface tile. An unavailable distance fails an enabled maximum-distance filter; an unavailable/unreachable route fails an enabled reachability filter.

Origin layer identity is validated once and copied as a scalar into runtime context, snapshots and both cache keys. Failure handling uses that copied identity rather than resolving a possibly unavailable live layer again. Complete `PlanetTile` values remain available to distance and restock adapters. Changing layer cannot reuse stock captured for a different origin merely because its numeric tile ID matches.

[TradeRestockAdapter](../src/SettlementTradeOverview/Integration/Snapshots/TradeRestockAdapter.cs) copies public restock metadata after stock capture. Scheduled moments retain game ticks, converted absolute ticks and coordinates of the active origin for local date/time presentation. Values are fixed at capture; the UI does not run a continuously updating restock timer.

## Optional Planner integration

[PlannerGenepackRelevanceAdapter](../src/SettlementTradeOverview/Integration/Planner/PlannerGenepackRelevanceAdapter.cs) soft-binds to the public facade `XenogermPlanner.Api.XenogermPlannerApi, XenogermPlanner`. Only [API version `1`](https://github.com/EscarvalOokt/XenogermPlanner/blob/v1.2.0/docs/integration-api.md) is accepted, followed by exact public request/result/member validation. The API version is independent from the Planner mod version. A newer API version is not assumed compatible.

STO supplies runtime-free gene names and copies returned stable plan IDs and display names. Planner owns coverage, exact-payload, readiness and conflict semantics; STO does not recreate them or inspect private Planner state. Expected absence, unsupported versions and unavailable responses produce neutral relevance while ordinary stock remains available.

[PlannerTradeRelevanceProjectionBuilder](../src/SettlementTradeOverview/Integration/Planner/PlannerTradeRelevanceProjectionBuilder.cs) owns transient relevance. It is not saved, does not enter stock snapshots or cache keys, and has no cross-mod event subscription. An already open window is not automatically updated when plans change. Reopen recomputes relevance over a reusable stock snapshot.

The common Details column combines Planner indicators and pawn trade details. Its query-owned [TradeDetailsSortKey](../src/SettlementTradeOverview/Query/TradeDetailsSortKey.cs) orders Planner match counts, colonist/slave purchase outcomes, rideable speed factors and neutral rows through one shared contract. Relevant indicators use the STO-owned `PlannerRelevance` texture and tooltips, with no remote acquisition or change to plan readiness.

## UI and localization

Both windows use [TradeListView](../src/SettlementTradeOverview/UI/TradeList/TradeListView.cs), query rules and shared UI controls. Each window owns transient search, category, sort, scroll and presentation state. Category availability is calculated from the search-matching projection before applying the selected category.

Projection metadata and row presentation are cached when their inputs change. List virtualization limits drawing to visible content; runtime targets are resolved through integration adapters. UI code does not discover traders or call `ITrader.Goods` directly.

Shared UI owns generic IMGUI controls, layout, scrolling, tooltips and state restoration. STO owns trade semantics and localization. Code-rendered monochrome icons use white textures with semantic tint; multicolor and game-owned icons preserve their intended rendering paths.

The supported languages and translation credits are listed in [README](../README.md#languages). Keyed entries use the `STO.` prefix, and main-button translations are placed beneath `DefInjected/MainButtonDef/`.

## Configuration

[SettlementTradeOverviewSettings](../src/SettlementTradeOverview/Settings/SettlementTradeOverviewSettings.cs) owns persisted mod configuration. [The settings policy](../src/SettlementTradeOverview/Settings/SettlementTradeOverviewSettingsPolicy.cs) normalizes values and produces eligibility criteria; windows do not duplicate these rules. User-facing defaults and controls are documented in README.

`WriteSettings` normalizes and persists the values, applies the settings service, invalidates stock when eligibility changes, and updates main-button visibility independently. Hiding the global button does not disable settlement-specific access.

## Persistence and save lifecycle

Mod settings use `ModSettings` and `Scribe_Values`. Stock snapshots, cache keys, runtime target references, query state and Planner relevance are derived in-memory data and are not serialized as an alternative stock store.

`GameComponent_SettlementTradeOverview` invalidates derived state on `StartedNewGame` and `LoadedGame`. It stores no fields, but RimWorld serializes its type as an empty entry in the game's component list. This entry is distinct from the in-memory stock snapshots. The settlement component stores no independent settlement reference or project data; its parent is provided by RimWorld.

When a save is loaded without STO, Scribe cannot resolve the saved component type and logs an additional error when it attempts the abstract `Verse.GameComponent` fallback. `Game.FillComponents` removes the failed null entry. Saving without STO therefore omits that entry; reloading the new save no longer produces these errors. Vanilla continues to own stock serialization and pawn references. Save compatibility and test scenarios are in [Testing policy](testing.md#save-compatibility).

## Project boundaries

STO owns discovery policy, immutable snapshots, cache compatibility, presentation and optional integration handling. RimWorld owns trading, stock generation/destruction, world layers/routes, Def initialization and saves. Planner owns its public API and relevance decisions; the shared UI supplier owns generic controls and their tests.

The product does not include remote trading, forced stock regeneration, private stock lifecycle manipulation, automatic dirty-cache restocking, trader-specific partial refresh or continuous restock countdowns.

Only STO's DLL belongs in the mod's assembly package. Game/reference DLLs, a separate shared UI assembly and test assemblies are not distributed. Compilation produces the current DLL; explicit deployment prepares the complete installation. Runtime acceptance and publication are separate activities.