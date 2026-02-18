# Backlog — Ordered Build Plan (Do Not Shuffle)

## Milestones (Reality-Based)
- **M0: Tabletop skeleton (no networking)** — core loop in GameCore, minimal debug UI.
- **M1: Local playable** — 3D board, selection, context menu, HUD/log.
- **M2: LAN host authoritative** — host/join by IP, server validation, snapshots/events, reconnect, timer.
- **M3: Bots** — dumb first, heuristics later.

## The First 20 Tasks (In Order)
1) Create Unity project + add Mirror.
2) Create GameCore assembly; enforce “no Unity types.”
3) Implement `GameState` + `PhaseState` union structure.
4) Implement serializer round-trip test (state → json → state).
5) Implement seeded `MapGenerator` producing nodes + lanes.
6) Implement `SetupPhaseState` + actions to place starting ships.
7) Implement phase transitions: TurnStart → Production → Movement → Encounter → Build → TurnEnd (placeholder actions allowed).
8) Implement `RulesEngine.Apply()` with strict validation (illegal action = reject).
9) Implement `GetLegalActions()` for Setup + Movement + EndTurn minimum.
10) Implement server host mode: server runs core state, accepts actions.
11) Implement client connect + receive snapshot.
12) Implement action send + accept/reject flow.
13) Implement simple in-editor debug UI to trigger actions (no 3D yet).
14) Implement action/event log UI (text).
15) Build 3D board view: instantiate node/lane meshes from map state.
16) Add camera + selection (raycast nodes/ships); ship-first selection priority.
17) Implement context menu fed only by legal actions.
18) Implement movement UI: Move mode → highlight legal endpoints → click destination → send MoveShip.
19) Implement EncounterPhase auto-resolve using stats; show result in log.
20) Implement Save/Load on server + broadcast loaded snapshot to clients.

## Explicit “Later” List (Don’t Touch Early)
- 5–6 players + expansion ruleset (design for it now; **test later**).
- Trading (LAN player-to-player).
- Full encounter/combat system (replace stub).
- “All cards/equipment” content explosion: only after LAN is stable and the rules engine is trustworthy.
- Bots (after M2; dumb first).
