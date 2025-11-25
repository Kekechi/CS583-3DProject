# 🎮 Project Summary & Architectural Guide

## **Project Overview**

A **3D Japanese room decoration game** where players:

1. Explore a traditional Japanese room
2. Click on placement spots to trigger mini-games
3. Complete mini-games to earn decorative items
4. Place customized items in the room
5. Fill the room to achieve harmony

**Current Implementation:** Lantern mini-game (brightness balancing mechanic)

---

## **Core Architecture Decisions**

### **1. Event-Driven Architecture** ✅

**Pattern:**

- **Commands** (requests with validation) → Direct method calls
- **Notifications** (fire-and-forget) → Events

```csharp
// Command (needs validation/response)
gameManager.StartMiniGame(MiniGameType.Lantern);

// Notification (broadcast to listeners)
OnItemPlaced?.Invoke(spot, item);
```

**Why:** Clear distinction between control flow and data flow.

---

### **2. Manager Hierarchy**

```
GameManager (Singleton, DontDestroyOnLoad)
    ├─ Manages global state (GameState enum)
    ├─ Coordinates between managers
    └─ Persists across scenes

RoomController (Scene-specific)
    ├─ Manages placement spots
    ├─ Handles item placement
    └─ Tracks room progress

MiniGameController (Scene-specific)
    ├─ Manages mini-game lifecycle
    ├─ Coordinates camera transitions
    └─ Routes completion events
```

**Key Principle:**

- **GameManager** = Singleton (truly global, one instance)
- **Other managers** = Inspector references (scene-specific, testable)

---

### **3. State Management** ✅

**Simple Enum Pattern:**

```csharp
public enum GameState
{
    RoomExploration,
    PlayingMiniGame,
    PlacingItem
}
```

**Why NOT State Machine Pattern:**

- Only 3 states (simple enough for enum)
- Linear flow (no complex transitions)
- Easy to understand and debug

**When to upgrade:** If you reach 5+ states with complex logic, consider State Machine pattern.

---

### **4. Prefab + Customization Pattern** ⭐ **Critical Decision**

**DO NOT pass mini-game visual instances to room!**

**Correct Flow:**

```
Mini-game creates visual instance (for gameplay)
    ↓
On completion: Pass PREFAB REFERENCE + CUSTOMIZATION DATA
    ↓
Destroy mini-game visual
    ↓
RoomController instantiates NEW instance from prefab
    ↓
Apply customization via ICustomizableItem interface
```

**Example:**

```csharp
// LanternGame.cs - CompleteGame()
LanternResult result = new LanternResult
{
    roomItemPrefab = roomLanternPrefab,  // ← PREFAB, not spawnedLantern instance
    finalBrightness = brightness,        // ← Customization data
    CompletionTime = Time.time
};
```

**Why:**

- ✅ Separates mini-game visuals from room decorations
- ✅ Allows different models/scales for gameplay vs decoration
- ✅ Easier to save/load (prefab names + data)
- ✅ Supports future features (item variants, upgrades)

---

### **5. Mini-Game Interface Pattern** ⭐ **Critical Decision**

**All mini-games implement `IMiniGame`:**

```csharp
public interface IMiniGame
{
    MiniGameType GameType { get; }
    void StartGame();
    void StopGame();
}
```

**MiniGameController uses Dictionary lookup:**

```csharp
private Dictionary<MiniGameType, IMiniGame> miniGames;

// In StartMiniGame()
if (miniGames.TryGetValue(gameType, out IMiniGame game))
{
    ActivateMiniGame(game);
}
```

**Why:**

- ✅ Data-driven (no switch statements)
- ✅ Easy to add new games (just implement interface)
- ✅ One `ActivateMiniGame()` method handles all games
- ✅ Automatic registration in `InitializeMiniGames()`

**When adding new mini-games:**

1. Implement `IMiniGame` interface
2. Assign in Inspector
3. Auto-registers in dictionary (no code changes to MiniGameController)

---

### **6. Customization Interface Pattern**

**Items that accept customization implement `ICustomizableItem`:**

```csharp
public interface ICustomizableItem
{
    void ApplyCustomization(MiniGameResult result);
}
```

**Example:**

```csharp
public class LanternItem : MonoBehaviour, ICustomizableItem
{
    public void ApplyCustomization(MiniGameResult result)
    {
        if (result is LanternResult lanternResult)
        {
            brightness = lanternResult.finalBrightness;
            UpdateLight();
        }
    }
}
```

**Why:**

- ✅ Decouples mini-game results from item implementation
- ✅ Type-safe (can cast to specific result type)
- ✅ Optional (items without interface just get default values)

---

### **7. Result Data Structure**

**Use custom classes per mini-game:**

```csharp
public class LanternResult : MiniGameResult
{
    public override GameObject ItemInstance => roomItemPrefab;
    public override MiniGameType GameType => MiniGameType.Lantern;

    public GameObject roomItemPrefab;  // What to place
    public float finalBrightness;      // How to customize
    public int adjustmentsMade;        // Stats
}
```

**Why NOT Dictionary<string, object>:**

- ✅ Type safety (compile-time checking)
- ✅ IntelliSense support
- ✅ Clear contract for each game
- ✅ Easy to refactor

**Only 3 mini-games planned** → Custom classes are worth it.

---

### **8. Event Subscription Timing** ⚠️ **Critical**

**ALWAYS subscribe in `Start()`, NOT `OnEnable()`:**

```csharp
void OnEnable()
{
    // DON'T subscribe here - serialized refs might be null
}

void Start()
{
    // DO subscribe here - all refs are assigned by now
    SubscribeToEvents();
}

void OnDisable()
{
    UnsubscribeFromEvents();
}

void OnDestroy()  // For DontDestroyOnLoad objects only
{
    UnsubscribeFromEvents();
}
```

**Why:**

- Unity assigns serialized references **between Awake() and Start()**
- `OnEnable()` might run before references are assigned
- `Start()` guarantees all serialized fields are valid

---

### **9. Placement Anchor System**

**PlacementSpot has optional `itemAnchor` transform:**

```csharp
public Transform itemAnchor;  // Optional: for precise positioning
```

**RoomController uses it:**

```csharp
Transform anchor = spot.itemAnchor != null ? spot.itemAnchor : spot.transform;
GameObject item = Instantiate(itemPrefab, anchor.position, anchor.rotation);
```

**Use case:** Align bottom of lantern to shelf surface (not center of collider).

---

## **Key Code Patterns**

### **Adding a New Mini-Game:**

1. **Create game class:**

```csharp
public class OrigamiGame : MonoBehaviour, IMiniGame
{
    public MiniGameType GameType => MiniGameType.Origami;
    public GameObject roomItemPrefab;  // Assign in Inspector

    public void StartGame() { /* your code */ }
    public void StopGame() { /* cleanup */ }
}
```

2. **Create result class:**

```csharp
public class OrigamiResult : MiniGameResult
{
    public override GameObject ItemInstance => roomItemPrefab;
    public override MiniGameType GameType => MiniGameType.Origami;

    public int foldQuality;
    public Color paperColor;
}
```

3. **Create item component (if customizable):**

```csharp
public class OrigamiItem : MonoBehaviour, ICustomizableItem
{
    public void ApplyCustomization(MiniGameResult result)
    {
        if (result is OrigamiResult origamiResult)
        {
            // Apply fold quality, color, etc.
        }
    }
}
```

4. **Assign in Inspector:**
   - Add OrigamiGame reference to MiniGameController
   - It auto-registers in dictionary!

**No changes needed to:**

- ✅ GameManager
- ✅ RoomController
- ✅ MiniGameController logic

---

### **Event Flow Example (Complete Flow):**

```
1. Player clicks PlacementSpot
   ↓ PlacementSpot.OnClicked event

2. RoomController.HandleSpotClicked()
   ↓ Stores currentTriggeredSpot
   ↓ Calls gameManager.StartMiniGame()

3. GameManager.StartMiniGame()
   ↓ Validates state (guards)
   ↓ Changes state to PlayingMiniGame
   ↓ Calls miniGameController.StartMiniGame()

4. MiniGameController.StartMiniGame()
   ↓ Looks up game in dictionary
   ↓ Moves camera to mini-game view
   ↓ Activates game (IMiniGame.StartGame())

5. Player completes mini-game
   ↓ LanternGame.CompleteGame()
   ↓ Creates LanternResult (with roomItemPrefab + data)
   ↓ Fires OnGameCompleted event

6. MiniGameController.HandleLanternComplete()
   ↓ Shows success UI (2 sec delay)
   ↓ Moves camera back to room
   ↓ Fires OnMiniGameComplete event

7. GameManager.HandleMiniGameComplete()
   ↓ Validates state
   ↓ Changes state to PlacingItem
   ↓ Fires OnItemReadyToPlace event (with prefab + result)

8. RoomController.HandleItemReadyToPlace()
   ↓ Retrieves currentTriggeredSpot
   ↓ Instantiates prefab at spot.itemAnchor
   ↓ Calls ICustomizableItem.ApplyCustomization()
   ↓ Marks spot as occupied
   ↓ Fires OnItemPlaced event
   ↓ Checks room completion
```

---

## **Inspector Setup Checklist**

### **GameManager:**

- ✅ `roomController` → Assign RoomController
- ✅ `miniGameController` → Assign MiniGameController

### **RoomController:**

- ✅ `allSpots` → Assign all PlacementSpot GameObjects
- ✅ `itemParent` → Assign empty GameObject for organization
- ✅ `totalRequiredItems` → Set to 3
- ✅ `gameManager` → Assign GameManager

### **MiniGameController:**

- ✅ `lanternGame` → Assign LanternGame
- ✅ `origamiGame` → Assign when implemented
- ✅ `calligraphyGame` → Assign when implemented
- ✅ `cameraController` → Assign CameraController

### **LanternGame:**

- ✅ `ui` → Assign LanternUI
- ✅ `lanternPrefab` → Mini-game visual (with LanternVisual component)
- ✅ `roomLanternPrefab` → Room item (with LanternItem component)
- ✅ `spawnPoint` → Where to spawn mini-game visual

### **PlacementSpot (per spot):**

- ✅ `triggersGame` → Set to Lantern/Origami/Calligraphy
- ✅ `itemAnchor` → Optional child transform for precise positioning
- ✅ Collider component (for clicks)

---

## **What NOT To Do** ❌

1. **DON'T pass mini-game visual instances to room**

   - Use prefab references instead

2. **DON'T subscribe to events in `OnEnable()`**

   - Use `Start()` instead

3. **DON'T use switch statements for mini-games**

   - Use dictionary lookup via IMiniGame

4. **DON'T make everything a singleton**

   - Only GameManager needs it

5. **DON'T use Dictionary<string, object> for result data**

   - Use typed classes (LanternResult, etc.)

6. **DON'T implement State Machine pattern yet**
   - Simple enum is sufficient for 3 states

---

## **Future Considerations**

### **Save/Load System (Not Implemented Yet):**

```csharp
[Serializable]
public class RoomSaveData
{
    public List<PlacedItemData> placedItems;
    public bool isComplete;
}

[Serializable]
public class PlacedItemData
{
    public string spotName;
    public string itemPrefabName;  // Use Resources.Load()
    public SerializableDictionary<string, float> customData;
}
```

**Architecture supports this** because:

- ✅ Using prefab references (can save as strings)
- ✅ Customization data in MiniGameResult (serializable)
- ✅ Placement logic centralized in RoomController

### **Multi-Scene Setup (Not Needed Yet):**

- Current: Single scene (sufficient for 1-3 rooms)
- Future: Persistent scene + additive room scenes
- GameManager already uses DontDestroyOnLoad (ready for it)

### **UI Manager (Partially Implemented):**

- Game-specific UI stays with games (LanternUI, etc.)
- Global UI goes through UIManager (pause menu, room complete, etc.)
- **TODO:** Integrate UIManager for global screens

---

## **Testing Workflow**

### **Context Menu Tests (Already Implemented):**

```csharp
[ContextMenu("Test/Start Room Placement")]
void TestStartRoomPlacement() { }

[ContextMenu("Test/Complete Mini-Game")]
void TestCompleteMiniGame() { }
```

### **Manual Test Flow:**

1. Play scene
2. Click placement spot → Mini-game starts
3. Complete mini-game → Item places in room
4. Check console for flow logs
5. Verify customization applied (brightness, etc.)

---

## **Current State**

### **✅ Implemented:**

- GameManager (state management, event coordination)
- RoomController (spot management, item placement)
- MiniGameController (game lifecycle, camera transitions)
- LanternGame (brightness balancing mini-game)
- IMiniGame interface + dictionary pattern
- ICustomizableItem interface + data application
- Event-driven architecture
- Guard clause validation
- Prefab + customization pattern

### **🟡 Partially Implemented:**

- UIManager (exists but not integrated)
- Audio system (placeholder methods)
- Harmony meter UI (TODO)

### **❌ Not Implemented:**

- Origami mini-game
- Calligraphy mini-game
- Save/load system
- Multi-scene management
- Room completion UI
- Settings/pause menu

---

## **Key Files Reference**

```
Assets/Scripts/
├── Managers/
│   ├── GameManager.cs          # Global state & coordination
│   ├── RoomController.cs       # Placement & room logic
│   └── MiniGameController.cs   # Mini-game lifecycle
├── MiniGames/
│   ├── IMiniGame.cs            # Mini-game interface
│   ├── LanternGame.cs          # Lantern mini-game
│   └── Lantern/
│       ├── LanternVisual.cs    # Mini-game visual
│       └── LanternResult.cs    # Result data
├── Items/
│   └── LanternItem.cs          # Room item with customization
├── Data/
│   ├── PlacementSpot.cs        # Clickable spot
│   ├── MiniGameResult.cs       # Base result class
│   └── MiniGameType.cs         # Enum of game types
├── ICustomizableItem.cs        # Customization interface
└── UI/
    └── LanternUI.cs            # Lantern mini-game UI
```

---

## **Quick Reference: Adding New Content**

### **New Mini-Game:**

1. Create `[Game]Game.cs : MonoBehaviour, IMiniGame`
2. Create `[Game]Result.cs : MiniGameResult`
3. Create `[Game]Item.cs : MonoBehaviour, ICustomizableItem` (optional)
4. Assign in MiniGameController Inspector
5. Done! (Auto-registers)

### **New Placement Spot:**

1. Add PlacementSpot component to GameObject
2. Add Collider (for clicks)
3. Set `triggersGame` enum
4. Optionally add `itemAnchor` child
5. Add to RoomController.allSpots list

### **New Room Item:**

1. Create prefab with visual
2. Add `[Item]Item.cs : ICustomizableItem` (optional)
3. Assign as `roomItemPrefab` in mini-game

---

**Architecture Philosophy:**

- ✅ **Explicit over implicit** (Inspector refs, not FindObjectOfType)
- ✅ **Data-driven** (Prefabs + data, not hardcoded instances)
- ✅ **Event-driven** (Loose coupling, easy testing)
- ✅ **Interface-based** (Extensible, maintainable)
- ✅ **Simple when possible** (Enum state, not State Machine)
- ✅ **Type-safe** (Custom classes, not dictionaries)
