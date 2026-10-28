# Settlement Trade Overview testing policy

This document defines automated test boundaries and reproducible in-game acceptance scenarios. Build and test commands, including deployment behavior, are in [README](../README.md#development). Implementation contracts are in [Architecture](architecture.md).

## Testing principle

Protect meaningful project-owned behavior: which settlements and offers appear, their ordering and displayed values, settings semantics, cache lifetime, isolation of failures and separation from live game objects.

Use deterministic tests for STO decisions given supplied inputs. Inspect the relevant external implementation when adapting to its API or lifecycle. Use the game for actual stock generation, save/load, Def initialization, optional integrations, input and rendering.

Test count and method coverage are not acceptance goals. Do not reproduce private RimWorld systems in mocks or add production abstractions solely to inspect a literal UI value.

## Automated test boundary

The NUnit project at [tests/SettlementTradeOverview.Tests](../tests/SettlementTradeOverview.Tests/SettlementTradeOverview.Tests.csproj) targets `net10.0`, references the production project and runs on the standard .NET 10 runtime outside the complete game. Passing this suite does not establish compatibility with RimWorld 1.7's Waylith runtime; the in-game acceptance scenarios remain required.

| Area | Meaningful project contracts |
| --- | --- |
| Query | Search across items, pawns and settlements; categories; search-sensitive category availability; all sort modes/directions; deterministic ties; empty and unavailable data. |
| Eligibility and commands | Trade/hostility/technology criteria; communications, distance, reachability and Royalty rules from supplied facts; enabled/disabled/hidden state and disabled-reason selection. |
| Snapshot conversion | One adapter capture per trader build; counts/currency; runtime-free pawn details and canonical gene names; safe malformed/missing metadata; per-trader failure isolation. |
| Price/restock/distance policies | Validity and fallback states; negotiated versus market-value presentation; independent numeric distance and route state; restock ticks and origin-relative moment data. |
| Layer identity | Origin layer in snapshots and cache/reuse equality; rejection of cross-layer distance comparison; safe unavailable or unreachable results; same-layer behavior without replacing endpoints by surface IDs. |
| Cache | Full key versus lightweight reuse key; map/origin/layer/negotiator/criteria changes; powered-console availability and Royalty DLC activity when their respective filters are enabled; refresh/invalidation; unavailable context and failure states. |
| Settings | Defaults, normalization, optional criteria and upper distance bound; global-button visibility independent of eligibility; defaults for missing persisted values. |
| Planner adapter | Exact API-version/public-shape acceptance, missing/incompatible API, invalid/failed replies, copied stable IDs/names, ordinary-stock fallback and transient projection. |
| Presentation and runtime targets | Details ordering, formatting policies, target registration/reset and safe missing-target fallback without live values entering immutable contracts. |
| Source composition | Shared UI types are compiled into the STO assembly; no separate runtime UI assembly is required. |

Use the runtime scenarios to check game-supplied data, stock generation, Scribe behavior and the real Planner integration.

Generic UI helper tests belong to `Escarval.RimWorld.UI.Tests`. STO keeps its own composition and integration checks, including in-game rendering and input scenarios.

## Build and package checks

Use isolated directories for build and deployment checks.

1. Build and test with deployment disabled. Verify that both the destination and `mod/` remain unchanged.
2. Perform a first deployment using `-p:DeployToGame=true -p:RimWorldModAssembliesDir=<absolute path>/Current/Assemblies/`. Verify a complete installation: all source content and historical DLLs match their source hashes, and the current DLL matches the build.
3. Repeat deployment after adding stale files to managed directories and obsolete root `Assemblies/` and `1.7/` directories. These files and obsolete directories must disappear; unrelated root files and a local source-absent `About/PublishedFileId.txt` must survive.
4. Reject missing, relative, historical or wrongly cased destinations, source-tree/ancestor/game-root destinations, a different installed package ID, symbolic links/junctions, invalid current load mapping and missing/changed historical DLLs before touching installed files.
5. Inspect the package: verify XML parsing, Def references, language keys/placeholders, `DefInjected/<DefType>/` placement, referenced textures, load-folder selection and assembly contents. Check that each supported version selects exactly one STO DLL and that the preserved 1.6 DLL retains its checksum. Check loading in R01/R28 and language rendering in R23.

<a id="runtime-acceptance-boundary"></a>

## Runtime acceptance setup

The scenarios below define acceptance criteria for 1.7, with R28 additionally checking package selection on 1.6. Record visible behavior separately from internal state and call counts that require diagnostic observation.

### Candidate, environments and observations

Before a run, record the date, game version/revision, operating system, STO source revision and DLL SHA-256, shared UI release tag or commit and any local source changes, complete DLC/mod list and load order, settings, language, resolution and UI scale. Record the candidate package path and verify that its DLL matches the one being evaluated.

Use disposable copies of saves for lifecycle and add/remove cases. Record the world seed, origin map/layer/tile, settlement IDs, relevant pawn IDs and initial game tick so another run can restore the same starting state. Keep a copy before stock access, expiry, settlement removal and each save transition.

Use these configurations as separate rows in the results:

| Configuration | Content |
| --- | --- |
| Core | Core plus STO; no optional DLC, Planner or Harmony mod. |
| Royalty | Core, Royalty and STO, including qualifying and non-qualifying colonists. |
| Biotech | Core, Biotech and STO, with physical genepacks and trade pawns. |
| Planner | Biotech configuration plus an identified compatible Planner API version 1 build. |
| Incompatible Planner | A separately identified test fixture or Planner build exposing an unsupported public API; never modify the installed production Planner merely to manufacture the case. |
| Inherited settlement | The DLC/configuration providing an Archonexus settlement Def, or a recorded test fixture inheriting vanilla Settlement. |
| Multiple layers | An identified DLC/test configuration providing at least two world layers and reproducible map/settlement endpoints. |

Where a configuration or fixture is unavailable, record that case as not run with the missing prerequisite.

Capture the log interval and visible result for each case. To check capture count, cache reuse, tracker state, trade-session restoration or component multiplicity, observe the named members with debugger breakpoints/watches or temporary diagnostic instrumentation in an isolated candidate. Include the observation method in the run results. Remove diagnostic changes before checking the release candidate.

For a failed case, retain the starting save, exact actions and relevant log. Separate failure, not run and deliberate support exclusion.

### Diagnostic observations

| Scenario | Required observation |
| --- | --- |
| R05–R06, R10 | Count public stock reads and discovery/capture calls during compatible reopen and Refresh. Trace cache invalidation/reuse decisions as context changes. |
| R07–R08 | Inspect tracker state and trace the destruction/generation sequence around expiry and save/load, including empty stock. |
| R09 | Settlement removal/unavailability with a retained snapshot and an injected failing trader; isolation of failures and destroyed runtime targets. |
| R14 | Compare the internal `TradeSession` negotiator, trader and gift-mode fields before and after capture. |
| R19 | Empty, unavailable or malformed genepack composition fixtures and the actual grouping/conversion failure boundary. |
| R20 | Run a deliberately incompatible Planner API fixture/build and inspect its runtime fallback. |

Also use diagnostic observation for component and snapshot inspection, API-result comparison, callback tracing and profiling. Record the method and configuration for each assertion.

## Runtime scenarios

### Save compatibility

RimWorld owns saved settlement stock and pawn references. STO rebuilds its in-memory snapshots after loading; its lifecycle component is saved as an empty entry in the game's component list.

| Scenario | Behavior |
| --- | --- |
| Add STO to a save | The game initializes the STO lifecycle component. Opening a trade window captures current trade data. |
| Migrate a save from RimWorld 1.6 to 1.7 | The complete mod package provides a DLL for each version. RimWorld 1.7 loads the matching DLL; STO rebuilds its derived state after loading the save. |
| Load a RimWorld 1.7 save after removing STO | Two load errors: the saved `GameComponent_SettlementTradeOverview` class is missing, and the fallback `Verse.GameComponent` is abstract. The save loads and the colony is accessible. This is a known removal limitation. |
| Save without STO and reload the new save | Saving omits the missing component entry. Reloading the new save no longer produces the missing-component errors. |

R25 covers addition, removal and the subsequent save/reload. R29 covers migration between game versions. Record first-load errors separately from the clean reload after resaving.

### Loading and entry points

| ID | Setup | Action | Expected result |
| --- | --- | --- | --- |
| R01 | Clean Core configuration and complete candidate package, no other STO copy. | Start RimWorld, enable STO, restart, create/load a colony and open both entry points. | Metadata, Defs, patches, assemblies and textures load without STO errors. Global overview and eligible settlement window open without Harmony or Planner. |
| R02 | Current map with no powered console; eligible friendly settlement otherwise within configured limits. | Open the global overview and select the settlement. Enable/disable the console requirement, then power a console on/off. | Availability follows the setting and actual power state; disabled command explains the reason. An unchanged eligible case can open normally. |
| R03 | New world and a save made before STO was enabled; supported settlement and a player-owned settlement. | Inspect commands in both games; save/reload twice and inspect component instances. | Exactly one STO command/component on supported targets, vanilla commands retained, no STO command on player-owned targets. No duplicate after load. |
| R04 | Inherited-settlement configuration plus an independently defined settlement-like fixture, if available. | Select each object and inspect its Def/components and gizmos. | Inheritance follows the patched comps list; player-owned Archonexus objects suppress trade access. Record whether independent Defs include the STO component; deriving their runtime class from Settlement alone does not guarantee the command. |

### Stock and cache lifecycle

For R05–R08 observe `Settlement_TraderTracker` stock state and public restock values before and after the action. Avoid reading `Goods` merely to inspect the precondition: the getter can generate stock.

| ID | Setup | Action | Expected result |
| --- | --- | --- | --- |
| R05 | Friendly eligible settlement with stock not generated; record initial public metadata and tick. | Open STO once, before opening vanilla trade. | One public capture builds a displayable snapshot, accepting vanilla lazy generation. Public restock values reflect generated stock. STO does not invoke private regeneration or alter the schedule. |
| R06 | Nonempty active stock from R05; fixed origin, negotiator and settings. | Close/reopen both windows, observe capture counts, then press Refresh. | Compatible reopen reuses stock without full rediscovery/capture. Refresh performs full discovery and one new capture per trader; it does not force a replacement for still-active nonempty stock. |
| R07 | Active stock with recorded next-restock tick; separate save copy. | Advance time beyond the expiry threshold, observe the tracker tick, then reopen a compatible cached view and finally Refresh. | Vanilla destroys expired stock without immediate replacement. Compatible reopen uses the cached snapshot; Refresh performs public capture and accepts vanilla lazy generation. |
| R08 | Save copies with active stock, expired/absent stock and completely empty generated stock. | Save/reload each; open STO and Refresh once. | Vanilla stock/schedule states survive as appropriate. STO rebuilds derived state after load. Empty stock follows vanilla getter behavior without an STO retry loop or corruption of other traders. |
| R09 | Loaded snapshot containing several settlements; disposable save. | Remove one settlement or make it unavailable, then reopen/Refresh. Separately introduce one failing trader using a recorded fixture. | Unresolvable cached traders prevent stale reuse; unrelated traders remain usable. Supported failure paths produce unavailable/partial state without retaining destroyed targets. |
| R10 | Loaded snapshot and a second map; record map, layer, tile, negotiator and enabled filters. Use the Royalty sequence below to check changes to qualifying colonists. | Change the active map, selected negotiator, eligibility setting or powered-console availability with its filter enabled, one at a time; reopen after each. Then run the Royalty sequence. | Changes to these key values or eligibility settings prevent incompatible reuse. Title or colonist-availability changes alone retain a compatible snapshot until Refresh. Hiding the main tab alone does not change eligibility or settlement access. Opening a settlement absent from the loaded snapshot follows the full path. |

For the Royalty sequence in R10, keep the DLC and **Require Royalty trade permission** enabled throughout the run. Do not reload the save or invalidate the cache except through the specified Refresh actions:

1. On the active map, have exactly one colonist who meets a settlement's required royal title for its faction, separate from the selected negotiator. Keep the settlement eligible under all other filters. Use nonempty stock, clear the search and select the **All** category so its offers are visible. Refresh the global overview and confirm that the settlement appears.
2. Move the qualifying colonist off the active map. Keep the active map, selected negotiator (including the recorded name and trade-price improvement), settings and console availability unchanged. Close and reopen the global overview without Refresh. The compatible snapshot is reused and still includes the settlement. Press Refresh; discovery reevaluates the remaining colonists and excludes it.
3. Return the qualifying colonist to the active map while keeping the same key values. Reopen the global overview without Refresh; the settlement remains absent from the reused snapshot. Press Refresh; the settlement appears again. Repeat with loss and restoration of the qualifying title while the colonist stays on the map, keeping the selected negotiator unchanged.

Use the global overview for this sequence: the settlement command evaluates permission separately, and opening a settlement absent from the cached snapshot can trigger discovery. Record reuse and discovery calls as well as the displayed result. `IsRoyaltyActive` means DLC activity; it does not track changes to colonists' titles.

### Filters, prices, distance and restock

| ID | Setup | Action | Expected result |
| --- | --- | --- | --- |
| R11 | Settlements covering friendly/hostile, trader/no-trader and below/at-Industrial technology cases; record distances and settings. | Toggle one filter at a time and inspect global rows and settlement-command reasons. | Discovery and command policy agree. Each supported failure explains itself; unsupported/player-owned objects stay hidden. |
| R12 | Current map, another player home map and a non-home map, with controlled console locations and power. | Leave only one powered console at each location in turn, then turn all off. | Current or other home-map console satisfies the enabled rule; a console exclusively on an unrelated non-home map does not. |
| R13 | Royalty configuration, a settlement requiring a title, qualifying/non-qualifying colonists; separate Core case. | Enable the Royalty filter, switch qualifying availability and press Refresh after each change before inspecting the overview. Check the settlement command separately. Repeat in the Core configuration with Royalty inactive. | Permission affects eligibility only when the filter and DLC apply. Refreshed rows and the settlement command reflect the current qualifying colonists. Core browsing remains usable without Royalty. |
| R14 | Trade goods with known current values; best-price colonist and another candidate. Record pre-existing `TradeSession` negotiator/trader/gift mode. | Compare STO player-buy estimates with vanilla trade using the same context; change candidates; exercise a map without an eligible negotiator. | Negotiator selection is deterministic. Estimates correspond to the selected context; without a negotiator valid market value is clearly a fallback. TradeSession fields are restored after capture. Invalid negotiated calculations display unavailable state. |
| R15 | Same-layer settlements with a reachable route and a known route failure; turn distance/reachability filters off initially. | Compare displayed distance with the same vanilla approximate-distance call; enable each filter independently. | Valid numeric distance survives route failure and shows the route warning. Reachability and distance thresholds filter independently; invalid distance is not shown as zero. |
| R16 | Multiple-layer configuration with full origin/target identities recorded, including the same numeric tile ID on different layers when constructible. | Inspect cross-layer stock with filters off; toggle distance and route filters; change active origin layer and reopen. | Cross-layer distance displays unavailable and no ground route. Either relevant filter excludes the case. Layer changes do not reuse a snapshot merely because numeric tile IDs match. No surface substitution occurs. |
| R17 | Scheduled restock, recorded capture tick and origin coordinates; two origins with different local times if available. | Compare expected timestamp with vanilla date conversion, leave the window open while time advances, then Refresh from each origin. | Timestamp uses the active origin's local context. Displayed remaining time is fixed to capture until refreshed; it does not count down while the window remains open. |

### Grouping and optional integration

| ID | Setup | Action | Expected result |
| --- | --- | --- | --- |
| R18 | Biotech trader stock containing recorded physical genepacks: equal composition/label, different genes, equal genes/different labels and reordered genes. | Compare vanilla and STO grouping; inspect each row's `AnyThing` and copied gene names. | A supported row has one set-based composition; labels/stacking rules can split equal compositions. Copied names are distinct and ordinally sorted. A row's representative describes every supported member. |
| R19 | Isolated fixture for empty/unavailable/malformed composition and ordinary goods in the same capture. Record whether the fault occurs during grouping or later conversion. | Capture the fixture and inspect ordinary rows, composition metadata and other traders. | A composition-extraction failure leaves the ordinary row available without Planner relevance. A vanilla grouping exception is isolated to the affected trader; other traders remain available. |
| R20 | Core/Biotech without Planner; then compatible and incompatible Planner configurations. | Open/Refresh both STO windows in each configuration. | Core stock browsing works throughout. Compatible API v1 can produce relevance; missing or rejected API produces neutral relevance without making Planner mandatory. |
| R21 | Real Planner with recorded unfinished coverage and exact-gene-set plans, exact-conflict-only, ready, degraded and prerequisite-only cases. | Compare indicators and tooltip names with the Planner API result. Change a plan while STO remains open, then close/reopen without Refresh. | STO shows only returned matches. It does not invent readiness rules or count offers as acquired. Already-open projection remains unchanged; reopen recomputes relevance without a new stock capture. Details sorting remains consistent in both windows. |

### UI, settings, save changes and packaging

| ID | Setup | Action | Expected result |
| --- | --- | --- | --- |
| R22 | Both windows with mixed goods/pawns/genepacks, empty result, unavailable context and large result set. Record resolution and supported UI scale. | Type/edit/clear search, switch categories, sort every visible column both ways, scroll, resize or change UI scale, hover icons and open info cards/navigation. | Input, clipping, scrolling, tooltips and hit targets remain aligned; category availability matches search before category filtering. Instance icons/details work and destroyed targets use safe fallback. No game errors or persistent stuck-loading state. |
| R23 | Candidate in each of the supplied languages listed in [README](../README.md#languages). | Open main tab, settlement command, settings, disabled reasons and both lists; inspect substitutions and longer labels. | Translations resolve, including main-button DefInjected entries in every supplied language; no raw keys/missing parameters or harmful clipping. Fallback behavior is recorded separately from translation quality. |
| R24 | Recorded settings defaults and non-default values, including distance boundaries. | Change settings, close/reopen settings, restart the game and load a different save; hide/show the main tab and use the settlement command. | Normalized mod settings persist with their intended scope. Eligibility changes invalidate derived stock; main-button visibility stays independent. No persisted stock snapshot is loaded from the save. |
| R25 | Separate disposable saves: one created without STO, one saved with STO. | Add STO to the former and save/reload. Remove STO from the latter and load it; inspect the colony, save under a new name and reload that copy. Capture logs for the initial load and the resaved copy separately. | Addition loads without STO errors. Removal leaves the colony accessible; record the initial missing-component errors as the known limitation. Reloading the resaved copy produces no STO-related errors. Check stock, pawns and world state for loss or corruption in both directions. |
| R26 | New game and multiple save files with distinguishable settlements/stock. | Alternate new game/load transitions while each window has previously been used. | Previous game's snapshots and live targets do not appear in the next game; loading state and selections degrade safely. |
| R27 | Representative large-world save; record settlement/row counts, hardware, graphics/UI settings and profiling method. | Measure first capture, manual refresh, compatible reopen and steady open-window rendering separately. Separate generation hitches from continuous UI cost. | No repeated capture in the steady rendering path; virtualized drawing and reuse behave as intended. |
| R28 | The same complete candidate package installed cleanly for separate 1.6 and 1.7 runs, with the historical and current DLLs and `LoadFolders.xml`; record both hashes. | In each game version verify the selected STO assembly path/hash, then repeat R01 and a focused browse/Refresh/settings/save-load smoke check. | Exactly one STO DLL loads: the preserved 1.6 binary on 1.6, the current build on 1.7. Shared content resolves in both. No root-level, reference/game/test or separate shared UI DLL is loaded. |
| R29 | A disposable copy of a RimWorld 1.6 save with STO; record game versions, both STO DLL hashes, DLC and other mods. | Load the copy on RimWorld 1.7 with the complete STO package, inspect the colony and trade windows, then save under a new name and reload. | No STO-related load errors; colony and trade stock remain accessible, and the resaved copy loads normally. Record other mods' migration issues separately. |

## Release candidate validation

A candidate is the complete package with an identified DLL for each supported game version. A build output, a source checkout and an installed candidate are different artifacts. Prepare the candidate by deploying to a clean external directory using the README command; current sources are built only for 1.7. The source `mod/` directory is not an installable current-version package.

1. Record the STO source revision, shared UI release tag or commit and any local source changes, game references, configuration and current DLL hash, plus the preserved 1.6 DLL's unchanged hash. Use the shared UI release specified in the [source setup instructions](../README.md#shared-ui-source) for the documented build configuration.
2. Inspect the complete package: `About/`, `LoadFolders.xml`, shared `Defs/`, `Patches/`, `Languages/`, `Textures/`, and `SettlementTradeOverview.dll` under `1.6/Assemblies/` and `Current/Assemblies/`.
3. Ensure each supported game version selects only its intended STO DLL. Do not ship unversioned DLLs under root `Assemblies/`, game DLLs, `UnityStubs.dll`, test assemblies or a standalone shared UI DLL.
4. Install the complete package in clean, isolated development locations for both versions. Explicit deployment prepares all package files and removes obsolete managed content; a standalone bin/ DLL is not a complete installation.
5. Record results against R28 for each version and the applicable 1.7 runtime cases above, including unavailable environments and unresolved failures. The 1.6 check verifies the preserved artifact and package layout; it does not rebuild or port current code to 1.6.
6. Recheck modified cases after fixes and identify the final installed candidate.

## Adding tests

For a defect or changed contract, first identify the realistic regression and its owner.

- Add a deterministic NUnit case when supplied inputs can reproduce the project's wrong decision, transformation, cache transition or fallback.
- Use the smallest existing seam; do not copy the private tracker, the engine, Scribe or Planner readiness semantics into a fake implementation.
- Add a runtime case when correctness depends on actual Def loading, stock generation, save/load, the engine's callbacks, input, rendering or a real optional integration.
- Keep regression cases focused on outcomes such as a hidden valid offer, wrong layer reuse, wrong price state, leaked runtime object or unavailable integration breaking normal browsing.
- Use guards/invalid-input cases where the input can realistically occur and safe rejection is part of the contract. Avoid tests that only mirror labels, colors, spacing or trivial API forwarding.
- Keep diagnostic performance runs separate from the regular suite unless a measured requirement justifies a stable threshold.

Update the appropriate procedure when behavior changes.