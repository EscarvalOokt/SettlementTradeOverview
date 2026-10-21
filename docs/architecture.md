# Settlement Trade Overview architecture

Settlement Trade Overview reads settlement stock into immutable snapshots and presents it in a global overview and a settlement-specific window. The current source targets RimWorld 1.7. The main manual runtime scenarios passed without errors on the new engine; checks requiring a diagnostic environment remain unverified.

User instructions and development commands are in [README](../README.md). Verification procedures and the limits of available evidence are in [Testing policy](testing.md).

## Sources of truth

- [Mod entry point](../src/SettlementTradeOverview/SettlementTradeOverviewMod.cs), [snapshot service](../src/SettlementTradeOverview/Application/Snapshots/TradeInventorySnapshotService.cs) and [game lifecycle component](../src/SettlementTradeOverview/Integration/Lifecycle/GameComponent_SettlementTradeOverview.cs) own initialization and application state.
- [Mod metadata](../mod/About/About.xml), [load-folder mapping](../mod/LoadFolders.xml), [main-button Def](../mod/Defs/MainButtonDefs/STO_GlobalOverview.xml) and [settlement patch](../mod/Patches/STO_SettlementTradeOverview.xml) define declared support, version selection and the packaged entry points.
- [Production project](../src/SettlementTradeOverview/SettlementTradeOverview.csproj) and [test project](../tests/SettlementTradeOverview.Tests/SettlementTradeOverview.Tests.csproj) define compilation, references and build actions.
- RimWorld's supplied implementation defines the external trade, world, persistence and UI contracts. The 1.7 source review used game version `1.7.4984 rev1242`, with `Assembly-CSharp` version `1.7.9789.37269`.
- [Planner API binding](../src/SettlementTradeOverview/Integration/Planner/XenogermPlannerApiV1Binding.cs) defines the exact optional public API shape accepted by STO. Planner remains the owner of relevance semantics.

Code establishes implemented behavior; metadata declares the supported target. Neither metadata nor source inspection establishes that a package has passed in-game checks. The separate maintainer-reported 1.7 runtime result is limited to the main manual scenarios and does not identify an exact tested package or all optional configurations. Historical 1.6 acceptance does not extend automatically to 1.7.

## Runtime baseline

The current development target is RimWorld 1.7 on the Waylith engine. Its mod assembly remains `net472`, compiled against the installed `Assembly-CSharp.dll` and `UnityStubs.dll`. The latter provides the Unity-named compatibility types used by the mod and its shared UI source. The compiler target is distinct from the game's own runtime.

The release package contains both `1.6/Assemblies/SettlementTradeOverview.dll` and `1.7/Assemblies/SettlementTradeOverview.dll`. The former is the preserved historical binary, compiled against the 1.6 game and Unity assemblies. Only the latter is built from current sources. `LoadFolders.xml` selects the package root for shared content plus the matching version directory, so each supported game version sees one STO DLL. There is no unversioned DLL under the package root's `Assemblies/` directory. The implementation described below belongs to the current 1.7 source; it does not imply backports into the frozen 1.6 binary.

The production project's explicit `PackageMod` target checks the historical DLL's SHA-256 before staging the current DLL. Ordinary builds and tests leave package preparation separate. Opt-in deployment copies only the 1.7 assembly and rejects a destination for another version or one retaining unversioned DLLs. Package preparation and installation commands are in README.

The package ID is `escarval.settlementtradeoverview`; the assembly and root namespace are `SettlementTradeOverview`. No Harmony patches or required runtime framework mod are used. Royalty and Biotech are optional content boundaries, and Xenogerm Planner is an optional integration.

`Escarval.RimWorld.UI` is included as source in the STO assembly. The supplier's standalone project and tests are separate: no production project reference or extra UI DLL is needed to load STO. Build-time source discovery and opt-in deployment are documented in README.

## Runtime flow

1. `SettlementTradeOverviewMod` loads and normalizes settings, applies the settings service and schedules main-button visibility after Def loading.
2. The main-button Def opens `MainTabWindow_SettlementTradeOverview`. Alternatively, the settlement XML patch adds a stateless `WorldObjectComp`, whose native gizmo delegates to the shared eligibility and command policy before opening `Dialog_SettlementStock`.
3. A window first asks the snapshot service for a compatible loaded snapshot. Otherwise it displays loading state, waits for a repaint and performs the requested synchronous capture from `WindowUpdate`.
4. The context adapter takes the current map as origin, retains its full `PlanetTile`, collects its free colonists, and determines powered-console and Royalty context. No active map produces an unavailable context.
5. Discovery applies settlement eligibility. The trader adapter reads public stock once per captured trader, groups goods through the vanilla trade boundary and immediately converts them to project-owned values.
6. The service owns snapshot/cache state. Query code produces the search/category/sort projection; the common `TradeListView` renders both windows.
7. Optional Planner relevance is projected from copied genepack compositions. Reopening a compatible window recomputes relevance without requiring a new stock read.
8. Manual refresh, relevant settings changes and new-game/load callbacks invalidate derived state and transient item targets.

The 1.7 implementation retains the native component/gizmo and UI update/repaint paths used here. The compatibility layer queues window callbacks differently from the old engine. The reported manual run found no runtime errors; internal execution and failure-path checks requiring diagnostic instrumentation remain unverified.

## Settlement eligibility and commands

[Eligibility adapters](../src/SettlementTradeOverview/Integration/Eligibility/SettlementEligibilityFactsAdapter.cs) collect facts; [SettlementEligibilityPolicy](../src/SettlementTradeOverview/Domain/Eligibility/SettlementEligibilityPolicy.cs) decides eligibility. The same policy serves discovery and the settlement command.

Filters cover trade availability, faction hostility, faction technology, powered communications access, maximum distance, ground reachability and optional Royalty trade permission. A powered console on the current map or another `IsPlayerHome` map satisfies communications access; an arbitrary non-home map does not.

[WorldObjectComp_SettlementTradeOverview](../src/SettlementTradeOverview/Integration/Settlements/WorldObjectComp_SettlementTradeOverview.cs) remains thin and stateless. It returns an enabled command for an eligible settlement, a disabled command with a reason for a supported potential trader that fails eligibility, or no command for unsupported/player-owned objects. It does not read stock or own cache state.

The XML patch targets the existing `comps` list of the vanilla `Settlement` Def. XML descendants that retain that list inherit the component. A separately defined settlement-like object is not guaranteed to receive it merely because its runtime class derives from `Settlement`. No Harmony fallback is added for that boundary.

## Stock access and snapshots

RimWorld owns settlement stock through `Settlement_TraderTracker`. Its public `ITrader.Goods` access can lazily generate stock when the container is absent or empty. Expired stock is destroyed by the tracker; a later read generates replacement stock. `CanTradeNow` does not prove that stock already exists.

[TraderGoodsCaptureAdapter](../src/SettlementTradeOverview/Integration/Snapshots/TraderGoodsCaptureAdapter.cs) performs one public capture per trader snapshot build. It groups trader-side goods using the applicable `TransferAsOne` mode and converts them synchronously through [TradeEntrySnapshotAdapter](../src/SettlementTradeOverview/Integration/Snapshots/TradeEntrySnapshotAdapter.cs).

Snapshots retain identities, labels, counts, categories, price state, pawn details, restock values and optional gene composition. They do not retain live `Thing`, `Pawn`, `Tradeable`, `GeneDef`, `GeneSet` or vanilla collections.

For supported genepack rows, grouping compares each candidate against `Tradeable.AnyThing`; matching gene sets and labels are required in addition to generic stacking rules. One representative therefore supplies the row's composition. `GenepackCompositionSnapshot` stores distinct non-empty gene definition names in ordinal order. Equivalent compositions may remain separate rows. Unavailable or empty composition metadata leaves the ordinary row usable without Planner relevance.

The public stock read can change vanilla state through lazy generation. STO's read-only boundary means no purchases, direct stock mutation, private tracker reflection, forced regeneration, schedule changes or lifecycle patches. Manual refresh repeats public capture; it is not immediate restocking. Initial capture remains synchronous and can cause temporary generation hitches.

## Cache and runtime targets

[TradeInventorySnapshotCache](../src/SettlementTradeOverview/Cache/TradeInventorySnapshotCache.cs) exposes `NotLoaded`, `Loading`, `Available`, `Empty`, `Unavailable`, `Partial` and `Failed` states. Per-trader failures must not discard successfully captured traders.

The lightweight reuse key includes context availability, active map, origin tile and layer, selected negotiator, eligibility criteria, and the effective communications/Royalty facts when their filters apply. The full key additionally includes ordered trader identities and discovery failure state. Planner state does not participate in either key.

Before reopen reuse, the service verifies that cached settlements can still be resolved. A settlement-specific view can reuse the shared snapshot when its target is present; otherwise it follows full discovery. Reuse does not imply that stock has been reread or that every external change has been detected. Refresh is the explicit way to rebuild the display.

[TradeEntryRuntimeTargetCache](../src/SettlementTradeOverview/Integration/Runtime/TradeEntryRuntimeTargetCache.cs) retains representative live items only inside the integration boundary for the lifetime of their snapshot. This supports instance icons, native tooltips and info cards. Missing/destroyed targets degrade to a safe definition-based presentation. Rebuild/invalidation clears the targets; neither immutable snapshots nor row presentation own them.

## Pricing, distance and restock

[TradeNegotiatorAdapter](../src/SettlementTradeOverview/Integration/Snapshots/TradeNegotiatorAdapter.cs) chooses an eligible free colonist on the active map by `TradePriceImprovement`, with a deterministic pawn-ID tie break. Dead, destroyed, non-colonist and Social-disabled candidates are excluded.

[TradePriceAdapter](../src/SettlementTradeOverview/Integration/Snapshots/TradePriceAdapter.cs) obtains the estimated player-buy price using a bounded trade-session scope and restores the previous negotiator, trader and gift mode. Without a negotiator, it uses valid instance or definition market value. Failed negotiated calculations and invalid values produce unavailable price state rather than a fabricated estimate.

Distance and routes use complete `PlanetTile` endpoints. On one layer, approximate distance and traversable ground-route state are distinct: a route failure can coexist with a numeric distance. Between layers, STO has no comparable approximate-distance result and reports no ground route. It does not reinterpret a layer-local tile ID as a surface tile. An unavailable distance fails an enabled maximum-distance filter; an unavailable/unreachable route fails an enabled reachability filter.

Origin layer identity is validated once and copied as a scalar into runtime context, snapshots and both cache keys. Failure handling uses that copied identity rather than resolving a possibly unavailable live layer again. Complete `PlanetTile` values remain available to distance and restock adapters. Changing layer cannot reuse stock captured for a different origin merely because its numeric tile ID matches.

[TradeRestockAdapter](../src/SettlementTradeOverview/Integration/Snapshots/TradeRestockAdapter.cs) copies public restock metadata after stock capture. Scheduled moments retain game ticks, converted absolute ticks and coordinates of the active origin for local date/time presentation. Values are fixed at capture; the UI does not run a continuously updating restock timer.

## Optional Planner integration

[PlannerGenepackRelevanceAdapter](../src/SettlementTradeOverview/Integration/Planner/PlannerGenepackRelevanceAdapter.cs) soft-binds to the public facade `XenogermPlanner.Api.XenogermPlannerApi, XenogermPlanner`. Only API version `1` is accepted, followed by exact public request/result/member validation. A newer version is not assumed compatible.

STO supplies runtime-free gene names and copies returned stable plan IDs and display names. Planner owns coverage, exact-payload, readiness and conflict semantics; STO does not recreate them or inspect private Planner state. Expected absence, unsupported versions and unavailable responses produce neutral relevance while ordinary stock remains available.

[PlannerTradeRelevanceProjectionBuilder](../src/SettlementTradeOverview/Integration/Planner/PlannerTradeRelevanceProjectionBuilder.cs) owns transient relevance. It is not saved, does not enter stock snapshots or cache keys, and has no cross-mod event subscription. An already open window is not automatically updated when plans change. Reopen recomputes relevance over a reusable stock snapshot.

The common Details column combines Planner indicators and pawn trade details. Its query-owned [TradeDetailsSortKey](../src/SettlementTradeOverview/Query/TradeDetailsSortKey.cs) orders Planner match counts, colonist/slave purchase outcomes, rideable speed factors and neutral rows through one shared contract. Relevant indicators use the STO-owned `PlannerRelevance` texture and tooltips, with no remote acquisition or change to plan readiness.

## UI and localization

Both windows use [TradeListView](../src/SettlementTradeOverview/UI/TradeList/TradeListView.cs), query rules and shared UI controls. Each window owns transient search, category, sort, scroll and presentation state. Category availability is calculated from the search-matching projection before applying the selected category.

Projection metadata and row presentation are cached when their inputs change. List virtualization limits drawing to visible content; runtime targets are resolved through integration adapters. UI code does not discover traders or call `ITrader.Goods` directly.

Shared UI owns generic IMGUI controls, layout, scrolling, tooltips and state restoration. STO owns trade semantics and localization. Code-rendered monochrome icons use white textures with semantic tint; multicolor and game-owned icons preserve their intended rendering paths.

The package supplies English, Russian, Ukrainian, Spanish and Latin American Spanish. Annabelesca provided the two Spanish translations. Keyed entries use the `STO.` prefix, and main-button translations are placed beneath `DefInjected/MainButtonDef/`. Package checks can establish key/placeholder and directory consistency; the main-scenario runtime report does not provide separate translation-quality, layout or fallback results for each language.

## Configuration

[SettlementTradeOverviewSettings](../src/SettlementTradeOverview/Settings/SettlementTradeOverviewSettings.cs) owns persisted mod configuration. [The settings policy](../src/SettlementTradeOverview/Settings/SettlementTradeOverviewSettingsPolicy.cs) normalizes values and produces eligibility criteria; windows do not duplicate these rules. User-facing defaults and controls are documented in README.

`WriteSettings` normalizes and persists the values, applies the settings service, invalidates stock when eligibility changes, and updates main-button visibility independently. Hiding the global button does not disable settlement-specific access.

## Persistence and save lifecycle

Mod settings use `ModSettings` and `Scribe_Values`. Stock snapshots, cache keys, runtime target references, query state and Planner relevance are derived in-memory data and are not serialized as an alternative stock store.

`GameComponent_SettlementTradeOverview` invalidates derived state on `StartedNewGame` and `LoadedGame`. The settlement component stores no independent settlement reference or project data; its parent is provided by RimWorld.

Vanilla retains ownership of stock serialization and pawn references. Source inspection of component initialization/load supports the intended integration, but adding to/removing from a save, actual Scribe behavior and old-save migration are separate runtime questions. No 1.7 save-safety guarantee follows from the absence of mod-owned stock data.

## Project boundaries

STO owns discovery policy, immutable snapshots, cache compatibility, presentation and optional integration handling. RimWorld owns trading, stock generation/destruction, world layers/routes, Def initialization and saves. Planner owns its public API and relevance decisions; the shared UI supplier owns generic controls and their tests.

The product does not include remote trading, forced stock regeneration, private stock lifecycle manipulation, automatic dirty-cache restocking, trader-specific partial refresh or continuous restock countdowns. A new measured requirement would be needed to justify changing these boundaries.

Only STO's DLL belongs in the mod's assembly package. Game/reference DLLs, a separate shared UI assembly and test assemblies are not distributed. Compilation, opt-in DLL deployment, complete package preparation, runtime acceptance and publication are distinct steps.