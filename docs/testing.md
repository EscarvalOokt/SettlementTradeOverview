# Settlement Trade Overview testing policy

This document defines automated test boundaries and reproducible in-game acceptance scenarios. Build and test commands, including deployment behavior, are in [README](../README.md#development). Implementation contracts are in [Architecture](architecture.md).

## Testing principle

Protect meaningful project-owned behavior: which settlements and offers appear, their ordering and displayed values, settings semantics, cache lifetime, isolation of failures and separation from live game objects.

Use deterministic tests for STO decisions given supplied inputs. Use source inspection for a specific external implementation. Use the game for actual stock generation, save/load, Def initialization, optional integrations, input and rendering. None substitutes for the others, and a scenario written here is not evidence that it ran.

Test count and method coverage are not acceptance goals. Do not reproduce private RimWorld systems in mocks or add production abstractions solely to inspect a literal UI value.

## Validation status

The 1.7 source review on 2026-10-21 examined game version `1.7.4984 rev1242` and `Assembly-CSharp` version `1.7.9789.37269`. It addressed settlement stock lifecycle, native world-object components, genepack grouping, world-layer identity and the UI compatibility paths used by STO.

The Release solution build passed with no warnings or errors using .NET SDK `10.0.401`. STO passed 552/552 tests on the normal `net472` test host. An isolated `net10.0` test project also passed 552/552 while referencing the same production `net472` DLL, using runtime `10.0.12`. The supplier's unchanged source/tests passed 64/64 separately on each host. These results belong to the 2026-10-21 working-tree migration and production DLL SHA-256 `28182d1a31980b870a1c6f3ce1ae52fde2a0f8a3bcbe4003b6baf426e341b031`.

All 28 package XML files parsed; the five language sets each contained 95 matching keyed entries with matching placeholders and the expected main-button DefInjected paths. Ordinary builds did not deploy files, including with an empty deployment destination. Explicit deployment was checked against a scratch directory, and enabling it without a destination produced the expected validation error.

The maintainer subsequently reported a successful local build/test run, error-free game startup and normal mod operation on 1.7. On 2026-10-21, the maintainer confirmed that the main manual runtime scenarios passed without errors. This is a reported manual pass; diagnostic scenarios remain unverified.

The report does not identify the exact runtime build, tested DLL hash, DLC/mod list, languages or individual scenario/configuration results. The earlier assembly versions, DLL hash and test counts above identify the automated/source baseline, not the maintainer's later game session. The aggregate manual pass does not establish every optional configuration, specialized fixture or instrumented assertion in R01–R28.

Historical 1.6 project records report successful stock lifecycle, settlement-component, grouping, integration, localization and performance acceptance. Their original run dates and exact runtime build were not recorded. Those results remain historical documentary evidence, not a 1.7 pass or a general compatibility claim.

The package was subsequently corrected to retain the historical 1.6 DLL alongside the current 1.7 DLL. `PackageMod` completed with no warnings or errors, and deployment checks accepted `1.7/Assemblies/` while rejecting a 1.6 destination, an unversioned destination, a missing destination and an installation retaining root-level DLLs. The new `LoadFolders.xml` was checked against the inspected 1.6 and 1.7 loader implementations. This package change requires a separate installed-package smoke check under each game version (R28); it has not been launched in either game version and does not invalidate the earlier 1.7 gameplay report.

## Automated test boundary

The NUnit project at [tests/SettlementTradeOverview.Tests](../tests/SettlementTradeOverview.Tests/SettlementTradeOverview.Tests.csproj) references the production project. It runs outside the complete game. Its smoke fixture establishes test discovery/execution only.

| Area | Meaningful project contracts |
| --- | --- |
| Query | Search across items, pawns and settlements; categories; search-sensitive category availability; all sort modes/directions; deterministic ties; empty and unavailable data. |
| Eligibility and commands | Trade/hostility/technology criteria; communications, distance, reachability and Royalty rules from supplied facts; enabled/disabled/hidden state and disabled-reason selection. |
| Snapshot conversion | One adapter capture per trader build; counts/currency; runtime-free pawn details and canonical gene names; safe malformed/missing metadata; per-trader failure isolation. |
| Price/restock/distance policies | Validity and fallback states; negotiated versus market-value presentation; independent numeric distance and route state; restock ticks and origin-relative moment data. |
| Layer identity | Origin layer in snapshots and cache/reuse equality; rejection of cross-layer distance comparison; safe unavailable or unreachable results; same-layer behavior without replacing endpoints by surface IDs. |
| Cache | Full key versus lightweight reuse key; map/origin/layer/negotiator/criteria changes; conditional communications/Royalty facts; refresh/invalidation; unavailable context and failure states. |
| Settings | Defaults, normalization, optional criteria and upper distance bound; global-button visibility independent of eligibility; defaults for missing persisted values. |
| Planner adapter | Exact API-version/public-shape acceptance, missing/incompatible API, invalid/failed replies, copied stable IDs/names, ordinary-stock fallback and transient projection. |
| Presentation and runtime targets | Details ordering, formatting policies, target registration/reset and safe missing-target fallback without live values entering immutable contracts. |
| Source composition | Shared UI types are compiled into the STO assembly; no separate runtime UI assembly is required. |

These checks establish STO behavior under their supplied inputs. They do not prove that RimWorld supplies those facts, that `Goods` is pure, that Scribe saves correctly, or that a real Planner version has the behavior of a fixture.

Generic UI helper tests belong to `Escarval.RimWorld.UI.Tests` at the supplier. Their execution is recorded separately; passing supplier tests does not establish consumer rendering or input correctness. STO keeps its own composition and integration checks.

Package inspection complements the suite: verify XML parsing, Def references, language keys/placeholders, `DefInjected/<DefType>/` placement, referenced textures, load-folder selection and assembly contents. Check that each supported version selects exactly one STO DLL and that the preserved 1.6 DLL retains its checksum. File presence does not establish successful loading or translation quality.

## Runtime acceptance boundary

The scenarios below remain the procedures and acceptance criteria for 1.7, with R28 additionally checking package selection on 1.6. The main manual checks have a maintainer-reported pass as summarized above; this does not mark every assertion in the tables passed. In particular, separate visible behavior from internal state and call counts that require diagnostic observation. Do not derive a runtime pass from source analysis or an automated fixture.

### Diagnostic checks still unverified

| Scenario | Outstanding diagnostic observation |
| --- | --- |
| R05–R06, R10 | Exact public stock-read counts, absence of rediscovery/capture during compatible reopen, and internal cache invalidation/reuse decisions. Successful opening and Refresh alone do not prove these counts. |
| R07–R08 | Tracker state and the exact destruction/generation sequence around expiry and save/load, including empty stock. Manual display/load outcomes are distinct from observing the tracker. |
| R09 | Settlement removal/unavailability with a retained snapshot and an injected failing trader; isolation of failures and destroyed runtime targets. |
| R14 | Restoration of the internal `TradeSession` fields after capture. Visible price comparisons do not establish restoration. |
| R19 | Empty, unavailable or malformed genepack composition fixtures and the actual grouping/conversion failure boundary. |
| R20 | A deliberately incompatible Planner API fixture/build and its runtime fallback. Ordinary operation with or without Planner does not cover this branch. |

Other assertions requiring component/snapshot inspection, API-result comparison, callback tracing or profiling also remain unverified without their own observations. Optional DLC, layer, language, save add/remove and package-identity cases have no separately recorded outcomes in the aggregate report; do not infer that every configuration or fixture was exercised.

### Candidate, environments and observations

Before a run, record the date, game version/revision, operating system, STO source revision and DLL SHA-256, shared UI source revision, complete DLC/mod list and load order, settings, language, resolution and UI scale. Record the candidate package path and verify that its DLL matches the one being evaluated.

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

Where a configuration or fixture is unavailable, record that case as not run with the missing prerequisite. A core-only pass does not cover it.

Capture the log interval and visible result for each case. For claims about capture count, cache reuse, tracker state, trade-session restoration or component multiplicity, observe the named members with debugger breakpoints/watches or temporary diagnostic instrumentation in an isolated candidate. State exactly how the observation was made; screen appearance alone cannot prove that no stock read occurred. Remove diagnostic changes before checking the release candidate.

For a failed case, retain the starting save, exact actions and relevant log. Separate failure, not run and deliberate support exclusion.

### Loading and entry points

| ID | Setup | Action | Expected result |
| --- | --- | --- | --- |
| R01 | Clean Core configuration and complete candidate package, no other STO copy. | Start RimWorld, enable STO, restart, create/load a colony and open both entry points. | Metadata, Defs, patches, assemblies and textures load without STO errors. Global overview and eligible settlement window open without Harmony or Planner. |
| R02 | Current map with no powered console; eligible friendly settlement otherwise within configured limits. | Open the global overview and select the settlement. Enable/disable the console requirement, then power a console on/off. | Availability follows the setting and actual power state; disabled command explains the reason. An unchanged eligible case can open normally. |
| R03 | New world and a save made before STO was enabled; supported settlement and a player-owned settlement. | Inspect commands in both games; save/reload twice and inspect component instances. | Exactly one STO command/component on supported targets, vanilla commands retained, no STO command on player-owned targets. No duplicate after load. |
| R04 | Inherited-settlement configuration plus an independently defined settlement-like fixture, if available. | Select each object and inspect its Def/components and gizmos. | Inheritance follows the patched comps list; player-owned Archonexus objects suppress trade access. Independent Defs are not treated as guaranteed coverage; record their actual behavior without adding a Harmony workaround. |

### Stock and cache lifecycle

For R05–R08 observe `Settlement_TraderTracker` stock state and public restock values before and after the action. Avoid reading `Goods` merely to inspect the precondition: the getter can generate stock.

| ID | Setup | Action | Expected result |
| --- | --- | --- | --- |
| R05 | Friendly eligible settlement with stock not generated; record initial public metadata and tick. | Open STO once, before opening vanilla trade. | One public capture builds a displayable snapshot, accepting vanilla lazy generation. Public restock values reflect generated stock. STO does not invoke private regeneration or alter the schedule. |
| R06 | Nonempty active stock from R05; fixed origin, negotiator and settings. | Close/reopen both windows, observe capture counts, then press Refresh. | Compatible reopen reuses stock without full rediscovery/capture. Refresh performs full discovery and one new capture per trader; it does not force a replacement for still-active nonempty stock. |
| R07 | Active stock with recorded next-restock tick; separate save copy. | Advance time beyond the expiry threshold, observe the tracker tick, then reopen a compatible cached view and finally Refresh. | Vanilla destroys expired stock without immediate replacement. Cached presentation can remain a snapshot; the next required public capture accepts lazy regeneration. No private lifecycle control or new read is attributed to reopen without observing it. |
| R08 | Save copies with active stock, expired/absent stock and completely empty generated stock. | Save/reload each; open STO and Refresh once. | Vanilla stock/schedule states survive as appropriate. STO rebuilds derived state after load. Empty stock follows vanilla getter behavior without an STO retry loop or corruption of other traders. |
| R09 | Loaded snapshot containing several settlements; disposable save. | Remove one settlement or make it unavailable, then reopen/Refresh. Separately introduce one failing trader using a recorded fixture. | Unresolvable cached traders prevent stale reuse; unrelated traders remain usable. Supported failure paths produce unavailable/partial state without retaining destroyed targets. |
| R10 | Loaded snapshot and a second map; record map, layer, tile and negotiator. | Change the active map, best negotiator, eligibility setting, applicable console power or Royalty context one at a time; reopen after each. | Each relevant context change prevents incompatible reuse. Hiding the main tab alone does not change eligibility or settlement access. Opening a settlement absent from the loaded snapshot follows the full path. |

### Filters, prices, distance and restock

| ID | Setup | Action | Expected result |
| --- | --- | --- | --- |
| R11 | Settlements covering friendly/hostile, trader/no-trader and below/at-Industrial technology cases; record distances and settings. | Toggle one filter at a time and inspect global rows and settlement-command reasons. | Discovery and command policy agree. Each supported failure explains itself; unsupported/player-owned objects stay hidden. |
| R12 | Current map, another player home map and a non-home map, with controlled console locations and power. | Leave only one powered console at each location in turn, then turn all off. | Current or other home-map console satisfies the enabled rule; a console exclusively on an unrelated non-home map does not. |
| R13 | Royalty configuration, a settlement requiring a title, qualifying/non-qualifying colonists; separate Core case. | Enable the Royalty filter and switch qualifying availability, then repeat with Royalty inactive. | Permission affects eligibility only when the filter and DLC apply. Core browsing remains usable without Royalty. |
| R14 | Trade goods with known current values; best-price colonist and another candidate. Record pre-existing `TradeSession` negotiator/trader/gift mode. | Compare STO player-buy estimates with vanilla trade using the same context; change candidates; exercise a map without an eligible negotiator. | Negotiator selection is deterministic. Estimates correspond to the selected context; without a negotiator valid market value is clearly a fallback. TradeSession fields are restored after capture. Invalid negotiated calculations display unavailable state. |
| R15 | Same-layer settlements with a reachable route and a known route failure; turn distance/reachability filters off initially. | Compare displayed distance with the same vanilla approximate-distance call; enable each filter independently. | Valid numeric distance survives route failure and shows the route warning. Reachability and distance thresholds filter independently; invalid distance is not shown as zero. |
| R16 | Multiple-layer configuration with full origin/target identities recorded, including the same numeric tile ID on different layers when constructible. | Inspect cross-layer stock with filters off; toggle distance and route filters; change active origin layer and reopen. | Cross-layer distance displays unavailable and no ground route. Either relevant filter excludes the case. Layer changes do not reuse a snapshot merely because numeric tile IDs match. No surface substitution occurs. |
| R17 | Scheduled restock, recorded capture tick and origin coordinates; two origins with different local times if available. | Compare expected timestamp with vanilla date conversion, leave the window open while time advances, then Refresh from each origin. | Timestamp uses the active origin's local context. Displayed remaining time is fixed to capture until refreshed; it is not advertised as a live countdown. |

### Grouping and optional integration

| ID | Setup | Action | Expected result |
| --- | --- | --- | --- |
| R18 | Biotech trader stock containing recorded physical genepacks: equal composition/label, different genes, equal genes/different labels and reordered genes. | Compare vanilla and STO grouping; inspect each row's `AnyThing` and copied gene names. | A supported row has one set-based composition; labels/stacking rules can split equal compositions. Copied names are distinct and ordinally sorted. A row's representative describes every supported member. |
| R19 | Isolated fixture for empty/unavailable/malformed composition and ordinary goods in the same capture. Record whether the fault occurs during grouping or later conversion. | Capture the fixture and inspect ordinary rows, composition metadata and other traders. | Conversion failures do not create false Planner matches; ordinary rows survive when only composition extraction fails. A vanilla grouping exception is isolated at its actual boundary and is not falsely reported as a successful malformed-row conversion. |
| R20 | Core/Biotech without Planner; then compatible and incompatible Planner configurations. | Open/Refresh both STO windows in each configuration. | Core stock browsing works throughout. Compatible API v1 can produce relevance; missing or rejected API produces neutral relevance without making Planner mandatory. |
| R21 | Real Planner with recorded unfinished coverage and exact-gene-set plans, exact-conflict-only, ready, degraded and prerequisite-only cases. | Compare indicators and tooltip names with the Planner API result. Change a plan while STO remains open, then close/reopen without Refresh. | STO shows only returned matches. It does not invent readiness rules or count offers as acquired. Already-open projection remains unchanged; reopen recomputes relevance without a new stock capture. Details sorting remains consistent in both windows. |

### UI, settings, save changes and packaging

| ID | Setup | Action | Expected result |
| --- | --- | --- | --- |
| R22 | Both windows with mixed goods/pawns/genepacks, empty result, unavailable context and large result set. Record resolution and supported UI scale. | Type/edit/clear search, switch categories, sort every visible column both ways, scroll, resize or change UI scale, hover icons and open info cards/navigation. | Input, clipping, scrolling, tooltips and hit targets remain aligned; category availability matches search before category filtering. Instance icons/details work and destroyed targets use safe fallback. No game errors or persistent stuck-loading state. |
| R23 | Candidate in each of the five supplied languages. | Open main tab, settlement command, settings, disabled reasons and both lists; inspect substitutions and longer labels. | Translations resolve, including Spanish main-button DefInjected entries; no raw keys/missing parameters or harmful clipping. Fallback behavior is recorded separately from translation quality. |
| R24 | Recorded settings defaults and non-default values, including distance boundaries. | Change settings, close/reopen settings, restart the game and load a different save; hide/show the main tab and use the settlement command. | Normalized mod settings persist with their intended scope. Eligibility changes invalidate derived stock; main-button visibility stays independent. No persisted stock snapshot is loaded from the save. |
| R25 | Separate disposable saves: one created without STO, one saved with STO. | Add STO to the former and save/reload; remove STO from the latter, load and save/reload again. Inspect logs and surviving game state separately. | Record actual component and settings consequences for each direction. No stock loss/corruption or continuing STO-related errors is the acceptance target. Missing-component messages must be recorded, not silently counted as clean acceptance. This case does not establish general 1.6-to-1.7 save migration. |
| R26 | New game and multiple save files with distinguishable settlements/stock. | Alternate new game/load transitions while each window has previously been used. | Previous game's snapshots and live targets do not appear in the next game; loading state and selections degrade safely. |
| R27 | Representative large-world save; record settlement/row counts, hardware, graphics/UI settings and profiling method. | Measure first capture, manual refresh, compatible reopen and steady open-window rendering separately. | No repeated capture in the steady rendering path; virtualized drawing and reuse behave as intended. Report generation hitches separately from continuous UI cost. No universal FPS/TPS guarantee is inferred. |
| R28 | The same complete candidate package installed cleanly for separate 1.6 and 1.7 runs, with both versioned DLLs and `LoadFolders.xml`; record both hashes. | In each game version verify the selected STO assembly path/hash, then repeat R01 and a focused browse/Refresh/settings/save-load smoke check. | Exactly one STO DLL loads: the unchanged historical artifact on 1.6, the current build on 1.7. Shared content resolves in both. No root-level, reference/game/test or separate shared UI DLL is loaded. This package-routing check remains unrun after the layout change. |

## Release candidate validation

A candidate is the complete package with an identified DLL for each supported game version. A build output, a source checkout and an installed candidate are different artifacts. Prepare the package with the `PackageMod` command in README; current sources are built only for 1.7.

1. Record the source/shared-source revisions, game references, configuration and current DLL hash, plus the historical 1.6 DLL's unchanged hash.
2. Inspect the complete package: `About/`, `LoadFolders.xml`, shared `Defs/`, `Patches/`, `Languages/`, `Textures/`, and `SettlementTradeOverview.dll` under both `1.6/Assemblies/` and `1.7/Assemblies/`.
3. Ensure each supported game version selects only its intended STO DLL. Do not ship unversioned DLLs under root `Assemblies/`, game DLLs, `UnityStubs.dll`, test assemblies or a standalone shared UI DLL.
4. Install the complete package in clean, isolated development locations for both versions. An opt-in 1.7 DLL copy does not install the remaining package files or remove leftovers from an older layout.
5. Record actual results against R28 for each version and the applicable 1.7 runtime cases above. Keep unavailable environments and unresolved failures visible rather than broadening a passing subset. The 1.6 check verifies the preserved artifact and package layout; it does not rebuild or port current code to 1.6.
6. Recheck modified cases after fixes, identify the final installed candidate, and distinguish readiness to publish from actual publication.

Product validation records remain with the project. Public documentation states the provenance, known context and limitations of reported results, including any missing candidate details; it does not require readers to open private records.

## Adding tests

For a defect or changed contract, first identify the realistic regression and its owner.

- Add a deterministic NUnit case when supplied inputs can reproduce the project's wrong decision, transformation, cache transition or fallback.
- Use the smallest existing seam; do not copy the private tracker, the engine, Scribe or Planner readiness semantics into a fake implementation.
- Add a runtime case when correctness depends on actual Def loading, stock generation, save/load, the engine's callbacks, input, rendering or a real optional integration.
- Keep regression cases focused on outcomes such as a hidden valid offer, wrong layer reuse, wrong price state, leaked runtime object or unavailable integration breaking normal browsing.
- Use guards/invalid-input cases where the input can realistically occur and safe rejection is part of the contract. Avoid tests that only mirror labels, colors, spacing or trivial API forwarding.
- Keep diagnostic performance runs separate from the regular suite unless a measured requirement justifies a stable threshold.

Update the appropriate procedure when behavior changes. Record execution separately; do not turn a newly added test or scenario into an implied pass.