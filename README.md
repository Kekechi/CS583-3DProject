# 🏯 Project Summary – The Little Zen Atelier

A calm, minimalist 3D Unity simulation game inspired by Japanese arts.
The player explores a Zen room, discovers three placement spots (tokonoma wall, floor areas), and completes mini-games — Origami, Calligraphy, and Lantern — to create decorative items.
Each crafted item is placed in the room, gradually bringing harmony to the space with soft lighting, gentle camera motion, and ambient sound.

## Members

- Keigo Morita
  - Goal: Game Design, Clean Code Architecture, Create a Game for Children

## Demo

[Video](https://drive.google.com/file/d/1AkAbXXPzhtawEBqUYm9M2zpJIWc3aPqm/view?usp=sharing)

## ⚙️ Core Features

**Main Menu** → (Planned) Start game, quit. Part of MVP.

**Room Exploration** → First-person style camera with panning to look around the traditional Japanese room.

**Placement Spot Discovery:**

- Semi-transparent ghost objects mark where items will go.
- Click on a placement spot → camera smoothly zooms to mini-game view and starts the corresponding game.

**Mini-games** (3D interactive, triggered by placement spots):

- **Lantern:** Hold/release SPACE to keep brightness bar in green "harmony zone" for 10 seconds. Player customizes the lantern's final brightness level.
- **Origami:** (Planned) Folding mechanic to create paper decorations.
- **Calligraphy:** (Planned) Brush control mechanic for wall art.

**Automatic Placement:**

- After completing a mini-game, camera returns to room view.
- The customized item is **automatically placed** at the spot that triggered the mini-game.
- Each placement spot is tied to a specific mini-game type (no flexible placement).

**Harmony Meter:** Fills as items are placed (33% → 66% → 100%); when full, triggers room completion.

**Camera System:**

- First-person style exploration mode with panning (look around the room).
- Smooth zoom transitions to/from mini-game locations.
- Event-driven movement (fires OnMovementComplete when transition finishes).

**Visual Progression:**

- Empty room starts with semi-transparent ghost objects at placement spots.
- Real items replace ghosts as they're crafted and placed.
- Each item retains player customizations (brightness, colors, patterns).

**Audio:** (Planned) Ambient sounds, gentle chimes, mini-game SFX.

## 🧩 Architecture Notes

**Single Unity scene** for simplicity.

**State Flow:**

- `PlacingItem` → Room exploration, player clicks placement spots.
- `PlayingMiniGame` → Active mini-game (lantern/origami/calligraphy).
- `PlacingItem` → Item automatically placed at triggered spot after mini-game.
- `RoomCompletion` → All 3 items placed.

**Camera System:**

- Position-based transitions (not orbit).
- Smooth movement using lerp with AnimationCurve easing.
- Event-driven: `OnMovementStarted`, `OnMovementComplete`.
- Predefined positions: `roomViewPosition`, `lanternGamePosition`, `origamiGamePosition`, `calligraphyPosition`.

**Placement Spot System:**

- Each spot is tied to ONE specific mini-game type (no flexibility).
- Spot tracks: `triggersGame` (enum), `isOccupied` (bool), ghost visual.
- Click spot → camera zooms to game → complete → item auto-places at that spot.
- Optional `itemAnchor` transform for precise item positioning (e.g., bottom of lantern aligned to surface).

**Mini-Game Architecture:**

- **Interface-based:** All games implement `IMiniGame` (GameType, StartGame(), StopGame()).
- **Dictionary lookup:** MiniGameController uses `Dictionary<MiniGameType, IMiniGame>` instead of switch statements.
- **Prefab + Data pattern:** Games return prefab reference + customization data (NOT visual instances).
- **Custom result classes:** Each game has typed result (e.g., `LanternResult` with `finalBrightness`).
- **Customization:** Items implement `ICustomizableItem` interface to receive and apply player customizations.

**Managers:**

- `GameManager`: Global state machine (singleton, DontDestroyOnLoad), coordinates between managers.
- `MiniGameController`: Mini-game lifecycle, camera transitions, completion handling.
- `RoomController`: Placement spots, item instantiation, customization application, room progress.
- `CameraController`: Smooth position transitions with events.
- `UIManager`: (Planned) Panel-based UI organization.
- `AudioManager`: (Placeholder) Sound effects and ambient audio.

**Event-Driven Architecture:**

- **Commands** (validation needed) → Direct method calls: `gameManager.StartMiniGame(type)`
- **Notifications** (broadcast) → Events: `OnItemPlaced?.Invoke(spot, item)`
- All subscriptions happen in `Start()`, NOT `OnEnable()` (Unity serialization timing).

**Data-Driven Design:**

- Mini-games auto-register in dictionary via `IMiniGame` interface.
- Prefab references + serializable customization data (easy save/load later).
- Inspector-based configuration (no hardcoded FindObjectOfType calls).

**Visual Feedback:**

- Semi-transparent ghost objects at empty placement spots.
- Ghosts disappear when items are placed.
- Items display player customizations (brightness levels, etc.).

**Art:** Simple 3D geometry with Japanese aesthetic.

**Sound:** (Planned) Gentle ambience, chimes, mini-game SFX.

## 🗓️ Development Progress

**✅ Completed:**

- GameManager state flow system with guard clauses and event coordination.
- RoomController with placement spot management and automatic item placement.
- MiniGameController with dictionary-based mini-game management.
- LanternGame mini-game (brightness balancing mechanic with customization).
- CameraController with smooth position-based transitions and events.
- Event-driven architecture (subscriptions in `Start()`, proper cleanup).
- IMiniGame interface + dictionary pattern (data-driven, extensible).
- ICustomizableItem interface + customization application (prefab + data pattern).
- LanternItem component (applies brightness customization to placed items).
- PlacementSpot system with click detection and itemAnchor support.

**📋 Next Steps (MVP):**

1. Build Origami mini-game (implement IMiniGame, create OrigamiResult, OrigamiItem).
2. Build Calligraphy mini-game (implement IMiniGame, create CalligraphyResult, CalligraphyItem).
3. Create room item prefabs with proper components (Light for lantern, etc.).
4. Implement harmony meter UI (progress bar that fills with each placement).
5. Create room completion sequence (camera zoom out, success screen).
6. Integrate AudioManager for ambient sounds and mini-game SFX.
7. Add ghost visual materials and animations.
8. Implement lighting transitions as harmony increases.
9. Create main menu scene (start game, quit).
10. Playtest full game loop and polish timing/feel.

**🔮 Future Features (Post-MVP):**

- **Manual Placement Mode:** After completing a mini-game, show multiple valid placement options for the item. Player chooses where to place instead of automatic placement.
- **Multiple Rooms:** Expand to 3-5 different themed rooms with varying difficulty.
- **Item Variants:** Multiple prefab options per mini-game (different lantern styles, etc.).
- **Save/Load System:** Persist room progress and return to decorated rooms.
- **Difficulty Scaling:** ScriptableObject-based room configurations for mini-game parameters.
