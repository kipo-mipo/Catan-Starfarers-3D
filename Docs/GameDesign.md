# Catan Starfarers Unity 3D — Game Design (Working Spec)

## 1) Project Intent
- Goal: a **private learning** recreation with **exact-ish Starfarers rules**, not a publishable commercial product.
- Target: playable full matches first; content-completeness (all cards/equipment, detailed encounters, etc.) comes **after** the core loop + LAN are stable.

## 2) Visual & UX Pillars (Non-Negotiable)
### 2.1 Hologram Control Room Aesthetic
- Scene is a spaceship control room backdrop + a **central hologram projector table**; the holomap is the hero.
- Build order: **Hologram-in-void first**, then wrap it with the control room later.

### 2.2 Color Language
- Default look is **cyan hologram** for lanes/ships/panels/highlights.
- Non-cyan colors are reserved for **meaning** (planet/resource indicators, warnings, current player, encounter outcomes).
- Don’t encode critical info in color alone (icons/shapes also required).

### 2.3 UI Layering
Use three layers:
- **Layer A (world-space holomap):** nodes/planets, lanes, ships, highlights.
- **Layer B (world-space anchored panels):** contextual small panels spawned near selection with a connector line.
- **Layer C (minimal screen-space HUD):** current player, phase, timer, recenter, end turn. (Yes, even with diegetic UI.)

### 2.4 “As Much Visible On-Map As Possible” Without Visual Sludge
Use a 3-tier information system:
- **Tier A (always visible):** node type/resource icon, occupancy, connectivity, current player highlight, legal move highlights (when relevant).
- **Tier B (hover/selection):** production tokens/numbers, lane cost/length, ship stats summary, resource payout preview.
- **Tier C (panel only):** purchase lists, equipment details, encounter detail text, anything verbose.

## 3) Camera & Readability Rules
- Perspective: **fixed “seated at station” angle** (locked pitch/roll), not free-flying.
- Map readability: board size doesn’t change for expansion, but the camera/view should **pan/yaw for readability** (human head-turn feel).
- Implementation guidance:
  - Prefer **yaw rotation** over literal sideways camera translation.
  - Mouse-edge yaw with dead-zone, damping, clamp to a modest range (your original “~30°” is likely too much if constant).
  - Provide **recenter** (button/key + gentle auto-recenter).

## 4) Interaction Contract (Must Not Drift)
### 4.1 Selection Priority
- Default click selects **ship first** if present; otherwise selects node.
- If stacked/ambiguous, add a small cycle or stack picker later; do not block MVP.

### 4.2 Lane/Movement Selection by Endpoints
- Lane selection “by endpoints” requires a **two-step Move flow**:
  1) Select ship  
  2) Choose **Move** from context menu  
  3) Enter Move Mode → highlight **legal endpoint nodes** (and/or adjacent legal lanes)  
  4) Player clicks destination node → system resolves lane/step; if ambiguous, prompt  
  5) Escape/right-click cancels Move Mode
- In Move Mode: **only legal endpoints glow; clicking anything else does nothing** (or subtle invalid ping).

### 4.3 Ship Motion
- Movement animation is **simple glide along lanes** (0.2–0.6s), no overdone easing; snap to node; optional faint cyan trail.

## 5) Scope Decisions (Locked)
- Multiplayer: **LAN, direct IP, host-authoritative, reconnect support, turn timer**.
- Players: design for **2–6**, but ship a stable **2–4 base** first; expansion later.
- Bots: desired eventually; start dumb later after LAN stability.
- Trading: later.
- Encounters/combat: **auto-resolve stub** until Milestone 2.
- Assets: low-poly stand-ins/replicas for learning.
