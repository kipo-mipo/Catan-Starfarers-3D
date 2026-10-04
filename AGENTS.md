# Catan Starfarers Unity Project

## Project Overview

This is a Unity 3D implementation inspired by **Catan: Starfarers**.

The project includes:

- Procedural / data-driven board generation
- Planet sectors
- Empty sectors
- Trade-station sectors
- Nodes and lanes
- Player colonies
- Player ships
- Setup-phase placement
- Ship movement
- Client-side board visualization
- Input / selection handling
- Multiplayer/network-aware architecture

The current Unity editor version is:

```text
6000.3.7f1
```

Do not upgrade the Unity version, packages, render pipeline, or major dependencies unless explicitly instructed.

---

# Core Development Philosophy

Prefer:

- Small changes
- Clear ownership of systems
- Existing architecture
- Deterministic game logic
- Explicit game-state transitions
- Easy-to-review diffs
- Debuggable implementations

Avoid:

- Large rewrites
- Premature abstractions
- Replacing working systems just because another architecture is more fashionable
- Introducing new frameworks without a strong reason
- Moving files or renaming public types unnecessarily
- Changing gameplay rules to simplify implementation
- Mixing presentation logic with authoritative game logic

If a task can be completed by modifying three existing methods, do not invent twelve new classes.

---

# Before Editing Code

Before making a non-trivial change:

1. Locate the relevant implementation.
2. Trace its callers.
3. Trace any data types it depends on.
4. Search for other systems that consume the same state.
5. Identify whether the code belongs to:
   - game rules,
   - board generation,
   - networking,
   - visualization,
   - input,
   - or UI.
6. Preserve established ownership boundaries.

Do not assume a class is unused because its references are not obvious from one file.

Unity frequently creates dependencies through:

- serialized fields,
- prefabs,
- scene objects,
- ScriptableObjects,
- Inspector assignments,
- tags,
- layers,
- network objects,
- component lookup,
- and prefab references.

Treat serialized Unity-facing APIs carefully.

---

# Repository Structure

Important project code currently lives under:

```text
Assets/MyAssets/
```

Major architectural areas include:

```text
Assets/MyAssets/GameCore/
Assets/MyAssets/Client/
```

The exact folder structure may evolve, so inspect the repository before assuming paths beyond these known roots.

---

# GameCore

`GameCore` should contain gameplay rules, board state, deterministic logic, and game-domain concepts.

Typical examples include:

- board topology,
- sectors,
- nodes,
- lanes,
- player state,
- placement legality,
- movement legality,
- setup sequencing,
- game-rule validation.

Where practical, `GameCore` should avoid dependencies on visual Unity components.

Do not move rendering, MonoBehaviour lifecycle logic, cameras, scene interaction, or input handling into `GameCore`.

---

# Client

`Client` contains Unity-facing presentation and interaction systems.

Examples include:

- board spawning,
- GameObjects,
- prefabs,
- rendering,
- visual orientation,
- selection,
- raycasting,
- player interaction,
- local visual feedback.

Known systems include:

```text
Assets/MyAssets/Client/BoardView/BoardSpawner.cs
Assets/MyAssets/Client/Input/SelectionController.cs
```

These paths may change later, so verify before editing.

---

# Board Architecture

The game board is composed of sectors.

Sectors contain or relate to:

- nodes,
- lanes,
- planets,
- empty space,
- trade stations,
- movement topology.

Board generation logic currently includes:

```text
BoardGenerator
```

Board visualization / prefab spawning currently includes:

```text
BoardSpawner
```

Keep logical board generation separate from visual board spawning whenever possible.

The logical board should not depend on whether a particular mesh, prefab, material, or particle effect exists.

---

# Nodes and Passability

Node behavior is part of the game rules.

Do not assume every generated node is traversable.

Known rule:

- The center node of a planet sector must NOT be passable.
- The center node of an empty sector must NOT be passable / usable for movement.

Do not “fix” these center nodes by making them traversable simply because they exist geometrically.

If changing board generation, explicitly verify center-node behavior for:

- planet sectors,
- empty sectors,
- trade sectors,
- and any future sector types.

Do not add constructor arguments or named arguments to domain models without first checking their actual constructor definitions and all call sites.

Example of a previously encountered failure:

```text
The best overload for 'Node' does not have a parameter named 'isPassable'
```

Do not guess API signatures.

Inspect the type first.

---

# Lanes

Ships move through lanes connecting valid nodes.

Movement topology must come from board state rather than visual proximity.

Do not determine legal movement by:

- GameObject distance,
- mesh adjacency,
- collider overlap,
- nearest-neighbor searches,
- or scene hierarchy.

Legal movement should use logical node/lane connections.

---

# Planet Sectors

Planet sectors have orientation-sensitive visuals.

Their prefab orientation may vary depending on board position.

Planet sectors can have directional variants such as:

```text
Up
Down
```

or equivalent orientation state.

Do not assume a single prefab rotation fits every planet-sector slot.

The logical sector identity and the visual orientation should remain conceptually separate.

---

# Trade Station Sectors

Trade stations also require directional orientation comparable to planet sectors.

Known trade-station themes include:

- Greenfolk / merchant
- Scientists
- Diplomats

Trade stations should support the corresponding up/down orientation behavior required by their board slots.

Do not hard-code trade stations as visually orientation-independent.

---

# Setup Phase

The setup phase currently supports player placement.

At minimum, the flow includes placement of:

1. Colony
2. Ship

Placement is constrained to viable logical board locations.

Setup should be modeled as explicit game state rather than inferred solely from what has already spawned visually.

Do not bypass legality checks in order to make click handling easier.

---

# Colony Placement

Colony placement should only occur on valid setup nodes.

Clicking arbitrary world geometry must not create a colony.

The client should:

1. Detect the user's selection.
2. Resolve the clicked visual object to its logical board element.
3. Ask game logic whether placement is legal.
4. Submit / perform the placement through the appropriate game-state path.
5. Reflect the result visually.

The visualization layer should not independently invent placement legality.

---

# Ship Placement

Ship placement follows setup rules and should occur only on valid starting lanes / positions associated with the player's setup state.

Do not allow client presentation code to place ships onto arbitrary lanes without game-rule validation.

---

# Ship Movement

Basic ship movement currently works one lane at a time.

Known working behavior:

- A ship can move from its current position through a connected valid lane.
- Movement is constrained by board topology.
- Movement should not pass through invalid center nodes.

Do not convert this into free-form world-space movement.

Do not infer movement from visual GameObject positions.

---

# Selection and Input

User selection currently uses:

```text
SelectionController
```

The selection system performs screen-space click handling and raycasting into the board.

A known Unity dependency is:

```text
Camera.main
```

The gameplay camera therefore MUST have the Unity tag:

```text
MainCamera
```

A previous input failure occurred because the gameplay camera was not tagged `MainCamera`.

Symptoms included:

```text
Mouse click received
```

but no valid board raycast hit.

Do not remove or replace the `MainCamera` dependency casually.

If changing camera or raycast code, verify:

- the correct camera is being used,
- the camera exists when input runs,
- the relevant colliders exist,
- the correct layers are included,
- raycasts resolve to logical board objects,
- and scene/persistent-object lifecycle does not invalidate references.

---

# Persistent Network Objects

Some client-side controllers may live under persistent network/root objects.

Do not assume scene reloads recreate every controller.

Before introducing scene-local references into persistent systems, verify their lifecycle.

Watch for issues involving:

- stale references,
- destroyed scene cameras,
- scene transitions,
- duplicated controllers,
- persistent EventSystems,
- network root objects.

---

# EventSystem

Unity UI/EventSystem objects may coexist with world interaction.

Do not automatically blame the EventSystem when world clicks fail.

When debugging world selection, verify in this order:

1. Input event received
2. Correct camera acquired
3. Ray produced
4. Physics layer mask correct
5. Collider hit
6. Hit object maps to selectable board element
7. Game-state legality check
8. Requested action executed

Add temporary diagnostic logs when necessary, but remove noisy logs once the issue is understood unless they provide durable debugging value.

---

# BoardSpawner

`BoardSpawner` is responsible for turning logical board state into scene visuals.

Keep it primarily concerned with representation.

Appropriate responsibilities include:

- choosing sector prefabs,
- instantiating board visuals,
- applying orientation,
- connecting spawned visuals to logical IDs,
- arranging board objects.

Avoid putting authoritative game rules in `BoardSpawner`.

If `BoardSpawner` needs to know whether something is legal, prefer reading that state from game logic rather than duplicating the rule.

---

# BoardGenerator

`BoardGenerator` is responsible for constructing logical board topology.

When modifying it, verify:

- sector creation,
- node IDs,
- lane IDs,
- node adjacency,
- lane adjacency,
- center-node rules,
- sector orientation metadata,
- duplicate nodes,
- duplicate lanes,
- cross-sector connections,
- deterministic results if required by multiplayer.

Do not make board topology depend on spawned GameObject transforms.

---

# Multiplayer and Determinism

Assume gameplay state may need to remain deterministic and network-safe.

Avoid introducing game-rule decisions based on:

- frame timing,
- unordered scene searches,
- floating-point proximity,
- non-deterministic physics results,
- local visual state,
- random values without controlled seeds.

Visual effects may be nondeterministic.

Authoritative game state should not be.

Before changing ownership of a multiplayer-related action, inspect the existing networking pattern.

Do not casually move logic between client and authority/server paths.

---

# Unity Inspector and Serialized Fields

Be conservative with:

```csharp
[SerializeField]
public
```

fields that may be referenced in scenes or prefabs.

Renaming a serialized field can silently break Inspector assignments.

If renaming is necessary, consider Unity migration mechanisms such as:

```csharp
[FormerlySerializedAs("oldName")]
```

when appropriate.

Do not rename serialized fields purely for cosmetic reasons.

---

# Prefabs

Do not assume code changes alone are enough when prefab structure matters.

Before requiring a new component on an existing prefab, determine whether:

- all prefab variants need it,
- it is already inherited from a base prefab,
- scenes contain manually placed instances,
- network prefabs require registration,
- serialized references need assignment.

Avoid creating fragile code that depends on an Inspector reference being populated without validation.

Where useful, fail with an explicit error rather than a vague `NullReferenceException`.

---

# Tags and Layers

Unity tags and layers are part of runtime behavior.

Known required tag:

```text
MainCamera
```

If code depends on a tag or layer:

- document it,
- validate it,
- avoid silently failing,
- and do not invent new project-wide layers unless necessary.

---

# Visual Asset Rules

Visual assets should retain the established board-game aesthetic.

Current direction favors:

- stylized
- colorful
- readable
- somewhat cartoon-like
- clean geometry
- limited realism
- restrained lighting
- restrained transparency
- strong silhouettes

Avoid pushing assets toward photorealism.

Trade-station visuals should remain visually distinctive between factions / station types.

Transparency should be intentional.

Unexpected semi-transparent materials or windows should not be introduced unless explicitly requested.

---

# Scene and Prefab Orientation

Some board-sector prefabs have directional variants.

Do not rely on arbitrary world rotation unless that matches the existing spawning architecture.

Before changing orientation logic, inspect:

- logical sector orientation,
- board slot orientation,
- prefab local axes,
- existing rotation code,
- child model transforms.

Prefer one clear orientation system.

Do not stack multiple corrective rotations in unrelated classes.

---

# Error Handling

When a compiler error occurs, solve the actual API mismatch.

Do not patch errors by inventing fields, overloads, constructors, or properties.

Examples of bad behavior:

```text
Guessing a Node constructor has an isPassable parameter.
```

Instead:

1. Inspect `Node`.
2. Inspect its constructors.
3. Inspect current construction sites.
4. Understand how passability is represented.
5. Make the smallest consistent change.

---

# Refactoring Rules

A refactor is justified when it solves a concrete problem such as:

- duplicated rules causing bugs,
- impossible-to-test coupling,
- broken ownership,
- repeated fragile logic,
- a feature requirement that cannot fit the current structure.

A refactor is NOT justified merely because:

- a class is long,
- another architecture looks cleaner,
- a design pattern could technically be used,
- inheritance could be introduced,
- interfaces could be added,
- dependency injection could be added.

Do not turn straightforward Unity gameplay code into enterprise architecture.

---

# Public API Changes

Before changing:

- constructors,
- method signatures,
- enums,
- serialized fields,
- event names,
- network messages,
- public properties,

search the entire project for call sites.

If a public API must change, update all affected references in the same change.

Do not leave the project in a partially migrated state.

---

# Debugging

When diagnosing a bug, establish which layer is failing.

Useful boundary checks include:

```text
Input
↓
Raycast
↓
Visual board object
↓
Logical board ID
↓
Game rule validation
↓
Game state mutation
↓
Network synchronization
↓
Visual refresh
```

Instrument boundaries instead of adding random logs everywhere.

A good diagnostic log should answer a specific question.

Examples:

```text
[SelectionController] Click received
[SelectionController] Raycast hit node visual ID=42
[SetupController] Node 42 placement legal=True
[GameState] Colony placed player=1 node=42
```

Avoid logs that merely say:

```text
here
working
clicked
test
```

---

# Code Style

Follow the project's existing C# style.

Prefer:

- descriptive names,
- short methods where practical,
- explicit state,
- guard clauses,
- clear domain terminology.

Avoid:

- deeply nested conditional logic,
- giant multi-purpose managers,
- reflection-heavy solutions,
- unnecessary LINQ in hot gameplay loops,
- hidden side effects,
- magic scene-object names.

Do not introduce a new naming convention into existing code.

---

# Comments

Comments should explain:

- unusual game rules,
- non-obvious architectural constraints,
- Unity lifecycle traps,
- networking requirements,
- intentional exceptions.

Do not write comments that merely restate code.

Bad:

```csharp
// Increment i
i++;
```

Useful:

```csharp
// Planet-sector center nodes exist geometrically but are not legal
// ship-movement nodes.
```

---

# Performance

Correctness and architecture come before micro-optimization.

However, avoid obviously expensive patterns inside frequent Unity callbacks.

Be cautious with:

```csharp
Update()
FixedUpdate()
LateUpdate()
```

Avoid repeated:

```csharp
FindObjectOfType
FindObjectsOfType
GameObject.Find
GetComponentsInChildren
```

every frame unless clearly justified.

Cache stable dependencies where appropriate.

---

# Generated Unity Folders

Do not edit or commit generated Unity folders.

Typical generated folders include:

```text
Library/
Temp/
Logs/
Obj/
Build/
Builds/
```

Treat Unity-generated project artifacts as disposable unless the repository explicitly indicates otherwise.

---

# Git

Prefer changes that produce focused diffs.

Before completing a substantial task:

1. Review changed files.
2. Remove unrelated formatting churn.
3. Remove accidental generated files.
4. Verify no assets were unnecessarily reserialized.
5. Check for compilation issues.
6. Summarize what changed and why.

Do not combine unrelated cleanup with feature work unless explicitly requested.

---

# When Asked to Implement a Feature

Use this process:

1. Inspect relevant architecture.
2. Explain the intended implementation briefly.
3. Identify files that must change.
4. Make the smallest viable change.
5. Check related call sites.
6. Check for obvious compile errors.
7. Review the diff.
8. Explain:
   - what changed,
   - why,
   - what should be tested in Unity.

Do not claim a Unity runtime behavior was tested unless it was actually tested.

---

# When Asked to Fix a Bug

Do not jump directly into edits.

First determine:

- expected behavior,
- actual behavior,
- failing architectural layer,
- likely root cause.

Prefer root-cause fixes over symptom patches.

If the problem is caused by a Unity configuration detail such as:

- missing tag,
- incorrect layer,
- missing collider,
- unassigned prefab reference,

say so clearly rather than rewriting working code.

---

# When Requirements Are Ambiguous

Infer from:

1. Existing game rules
2. Existing architecture
3. Existing code behavior
4. Existing naming conventions

Do not invent major mechanics.

If two interpretations would produce materially different game behavior, state the ambiguity before making a broad architectural change.

For small implementation details, choose the option most consistent with existing code.

---

# Protect Working Systems

Several systems are already functioning.

Known working functionality includes:

- setup-phase colony placement,
- setup-phase ship placement,
- board-node selection,
- one-lane-at-a-time ship movement,
- directional sector spawning,
- trade-station visual placement.

Do not rewrite these systems unless the requested feature requires it.

When touching one of them, preserve existing behavior unless explicitly told otherwise.

---

# Current Known Unity Gotchas

## Main Camera

World selection relies on the gameplay camera being tagged:

```text
MainCamera
```

Failure to do this can result in mouse input being logged while raycasts fail to resolve board objects.

## Center Nodes

Planet-sector center nodes are intentionally not passable.

Empty-sector center nodes are also intentionally not passable.

Their existence in generated topology or visuals does not imply legal movement.

## Constructor Assumptions

Do not assume named parameters exist on game-domain constructors.

Inspect the actual type definition before modifying creation logic.

---

# Codex-Specific Instructions

When beginning work in this repository:

1. Read this file.
2. Inspect the relevant code before proposing changes.
3. Do not perform broad cleanup unless requested.
4. Preserve working gameplay.
5. Prefer modifying existing systems over adding parallel ones.
6. Search the repository before creating a type that may already exist.
7. Do not invent missing APIs.
8. Check Unity serialization implications.
9. Check multiplayer implications.
10. Report assumptions clearly.

For substantial tasks, first provide a short architecture assessment before modifying code.

If the requested implementation conflicts with an established game rule in this document, do not silently override the rule.

Call out the conflict.

---

# Definition of Done

A code change is not done merely because the edited method looks correct.

Before considering work complete, verify as much as possible:

- Code is syntactically consistent with the project.
- Referenced methods and constructors actually exist.
- Call sites were updated.
- Unity serialization was considered.
- Board topology remains valid.
- Network ownership / authority was considered.
- Existing gameplay rules remain intact.
- Changes are limited to the requested scope.
- No generated Unity files were added.
- The user is told what must still be tested inside Unity.

When runtime verification is not possible, explicitly state that the final Unity behavior still needs editor/play-mode testing.