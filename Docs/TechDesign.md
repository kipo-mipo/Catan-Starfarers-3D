# Technical Design — Unity + Mirror + Deterministic GameCore

## 1) Architecture Pillars (If You Break These, You Deserve the Bugs)
1) **One authoritative state:** `GameState` is the only truth.
2) **Actions in, events out:** clients send intent actions; server validates/applies; server emits events/snapshots; clients render.
3) **Strict phase machine:** phase is data (not booleans); legality is determined by the current phase state.

## 2) Project Layout (Recommended)
### 2.1 Core (Pure C# Assembly)
`/GameCore` (no Unity types: no `MonoBehaviour`, no `GameObject`, no `Vector3`)

Suggested folders/classes:
- `State/`
  - `GameState.cs` (entire match)
  - `MapState.cs` (nodes/lanes)
  - `PlayerState.cs`
  - `ShipState.cs`
  - `DeckState.cs`, `CardState.cs` (data-driven content)
- `Phases/`
  - `IPhaseState.cs`
  - `SetupPhaseState.cs`
  - `TurnStartPhaseState.cs`
  - `ProductionPhaseState.cs`
  - `MovementPhaseState.cs`
  - `EncounterPhaseState.cs`
  - `BuildPhaseState.cs`
  - `TurnEndPhaseState.cs`
- `Actions/` (`IGameAction.cs`, one file per action kind)
- `Events/` (`IGameEvent.cs`, one file per event kind)
- `Rules/`
  - `RulesEngine.Apply(state, action) -> (newState, events)` (or strict mutation + events)
  - `RulesEngine.GetLegalActions(state, player) -> List<LegalAction>`
- `Serialization/`
  - `SaveGame.cs` (seed + state + actionIndex)
  - `Serializer.cs`
- `Tests/` (at minimum: serialization round-trip tests)

### 2.2 Unity Client
- `BoardView/` (render nodes/lanes/ships from map state)
- `Selection/` (raycast selection; ship-first priority)
- `UI/ContextMenu/` (render legal actions only)
- `UI/HUD/` (player/phase/timer/recenter/end turn)
- `UI/Log/` (event/action log)

### 2.3 Networking Layer (Mirror)
- `UnityNet/ServerHost.cs` (authoritative state owner)
- `UnityNet/ClientNet.cs`
- `UnityNet/NetMessages/` (ActionRequest, Snapshot, EventBatch, Accept/Reject)

## 3) Data Model: Nodes + Lanes (Graph Board)
- Map is a graph:
  - `NodeId` (int), `LaneId` (int)
  - `MapNode`: type (planet/empty/pirate/etc.), position stored as numeric tuple (not Unity vector), plus optional PlanetData.
  - `MapLane`: endpoints `a,b`, type, movement cost/length.
- Ship state:
  - `ShipId`, owner player, current `NodeId`
  - stats (atk/def/speed/cargo etc.), upgrades list, cargo/resources.

## 4) Phase Machine (Strict)
Phase is explicit structured data, e.g.:
- `Setup(step, ...)`
- `TurnStart(player)`
- `Production(player)`
- `Movement(player, movementPointsRemaining, activeShipId, ...)`
- `Encounter(player, encounterContext)`
- `Build(player)`
- `TurnEnd(player)`

Minimal action list per phase (initial):
- Setup: `PlaceStartingShip(NodeId)`, `ConfirmSetup()`
- TurnStart: `AcknowledgeTurnStart()`
- Production: `RollProduction()`, `AcknowledgeProduction()`
- Movement: `SelectActiveShip(ShipId)`, `MoveShip(ShipId, LaneId)` (one step per action), `EndMovement()`
- Encounter (stub): `ResolveEncounterAuto()`
- Build: `BuildAtPlanet(NodeId, BuildKind, ...)`, `Acquire(UpgradeId/CardId/...)`, `EndBuildPhase()`
- TurnEnd: `EndTurn()`

## 5) Legal Actions API (Backbone of UI + Bots)
Expose from GameCore:
- `IEnumerable<LegalAction> GetLegalActions(GameState s, PlayerId p)`

Each `LegalAction` includes:
- `ActionKind kind`
- `string label`
- template/required parameters
- optional UI hints: nodes/lanes/ships to highlight

UI rule: the context menu is a **dumb renderer** of this list; do not handcraft per-phase menus.

## 6) Map Generation (Seeded, Deterministic)
- Generator inputs: seed, playerCount, rulesetVersion (base first; expansion later).
- Output: nodes/lanes, tokens/numbers, starting positions.
- Save only the seed (plus resulting state); ability to reproduce from seed is key for debugging and reconnect.

## 7) Networking Protocol (Turn-Based, Efficient)
Turn-based means you don’t sync transforms; you sync **state + events**.

### 7.1 Messages
Client → Server:
- `ActionRequest { playerId, action, knownActionIndex }`

Server → Client:
- `ActionRejected { reason, stateSnapshot? }`
- `ActionAccepted { newActionIndex, eventsSinceClientIndex }`
- `Snapshot { state, actionIndex }` (on join/reconnect and periodic)

### 7.2 Snapshots + Event Log
- Full snapshot on join/reconnect; periodic snapshot every N actions (10–20) otherwise stream events.

### 7.3 Turn Timer
- Server owns timer; on expiry server forces default action (end turn / auto-pass / auto-resolve).

## 8) Encounters Stub (Replaceable Later)
- Deterministic, stat-based resolve:
  - `EncounterOutcome Resolve(attackerStats, defenderStats, seedSalt)` using seed + actionIndex + salt for small randomness.
- Emit an `EncounterResolved(...)` event; later swap only the resolver, not the whole pipeline.

## 9) Save/Load (Early, Server-Authoritative)
- SaveGame includes: `mapSeed`, `rulesetVersion`, `actionIndex`, full `GameState`, optional action log for replay.
- Server-side only; clients can request but server decides. JSON first; optimize later if needed.
