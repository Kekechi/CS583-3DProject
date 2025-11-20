# 🏯 Project Summary – The Little Zen Atelier

A calm, minimalist 3D Unity simulation game inspired by Japanese arts.
The player explores a Zen room, discovers three placement spots (tokonoma wall, floor areas), and completes mini-games — Origami, Calligraphy, and Lantern — to create decorative items.
Each crafted item is placed in the room, gradually bringing harmony to the space with soft lighting, gentle camera motion, and ambient sound.

## ⚙️ Core Features

**Main Menu** → start game, select room, settings.

**Room Exploration** → orbit camera with limited rotation to look around the tokonoma area.

**Placement Spot Discovery:**

- Semi-transparent ghost objects mark where items will go (tokonoma wall, floor positions).
- Center camera cursor on a spot → highlight activates.
- Click highlighted spot → zoom to that location and start corresponding mini-game.

**Mini-games** (UI-based, triggered by placement spots):

- **Origami:** Press correct arrow keys for folding accuracy (floor spots).
- **Calligraphy:** Keep brush speed consistent with hold/release rhythm (wall spot only).
- **Lantern:** Hold/release SPACE to keep brightness bar in green "harmony zone" (floor spots).

**Immediate Placement:**

- After completing a mini-game, camera returns to room.
- Valid placement spots highlight (based on item type and availability).
- Calligraphy: Must go on tokonoma wall (1 spot only).
- Origami/Lantern: Flexible placement on 2-3 floor spots around tokonoma.
- Click to place item → appears immediately, ghost disappears.

**Harmony Meter:** Fills as items are placed (33% → 66% → 100%); when full, triggers room completion.

**Camera System:**

- Orbit mode for exploration (limited rotation, no movement).
- Smooth zoom transitions to/from placement spots.
- Configurable timing for playtesting.

**Visual Progression:**

- Empty room starts with semi-transparent ghost objects.
- Real items replace ghosts as they're crafted and placed.
- Lighting and ambience evolve as room fills.

**Audio:** Ambient sounds, gentle chimes on selection, paper/ink SFX during mini-games.

## 🧩 Architecture Notes

**Single Unity scene** for simplicity (recommended for 1-month dev).

**State Flow:**

- `RoomExploration` → camera orbit, spot selection via raycast from center cursor.
- `PlayingMiniGame` → active mini-game (origami/calligraphy/lantern).
- `PlacingItem` → highlight valid spots, player chooses where to place.
- Loop 3 times until `RoomCompletion`.

**Camera System:**

- Orbit controller with rotation limits (horizontal: 180°, vertical: ±30°).
- Coroutine-based smooth transitions with AnimationCurve easing.
- `IsMoving` property for transition state tracking.

**Placement Spot System:**

- Each spot has: position, rotation, allowed item types (enum flags), occupied state.
- Placement spots double as mini-game triggers (click spot → play game → place item there).
- Tokonoma Wall: Calligraphy only (1 spot).
- Floor Right/Left/Front: Origami or Lantern (2-3 flexible spots).
- Smart highlighting: only show available, unoccupied spots matching current item.

**Mini-Game Architecture:**

- Separate scripts: `LanternGame` (logic), `LanternUI` (display), `LanternVisual` (emission).
- Event-driven: mini-game fires `OnGameCompleted` → `MiniGameController` handles transition.
- Transition flow: success display (configurable time) → camera return → placement mode.

**Managers:**

- `GameManager`: State machine, high-level flow.
- `MiniGameController`: Mini-game lifecycle, post-completion transitions.
- `UIManager`: Panel-based UI organization.
- `AudioManager`: Sound effects and ambient audio.
- `RoomController`: Placement logic, harmony meter, completion sequence.
- `CameraController`: Orbit mode + zoom transitions.

**Visual Feedback:**

- Semi-transparent ghost objects (30-40% opacity) at empty placement spots.
- Highlight shader when cursor centers on spot.
- Real items replace ghosts after placement.
- Room lighting evolves as harmony meter fills.

**Data-driven rooms:** ScriptableObjects define lighting, theme, and mini-game difficulty (future expansion).

**Art:** Free CC0 textures (tatami, fusuma, wood) and simple 3D geometry.

**Sound:** Gentle ambience, chimes on selection, paper/ink SFX.

## 🗓️ Development Progress

**✅ Completed:**

- GameManager state flow system with events.
- Lantern mini-game with progress bar UI (LanternGame, LanternUI, LanternVisual).
- Camera controller with coroutine-based smooth transitions.
- MiniGameController transition system (success → room → placement).
- Event-driven architecture (mini-game completion → transition → placement).

**🚧 In Progress:**

- Camera orbit system for room exploration.
- Placement spot selection with raycast and highlights.

**📋 Next Steps:**

1. Implement camera orbit controller with rotation limits and center cursor targeting.
2. Create placement spot system (Transform markers, PlacementSpot data class).
3. Add semi-transparent ghost objects for empty spots.
4. Implement spot highlighting and click-to-start mini-game flow.
5. Build placement mode UI (highlight valid spots, click to place).
6. Test full loop: explore → select spot → play → place → repeat × 3.
7. Clone Lantern logic for Origami (arrow keys) and Calligraphy (brush rhythm).
8. Implement harmony meter and room completion animation.
9. Integrate audio and polish lighting transitions.
10. Playtest and adjust configurable timing parameters.
