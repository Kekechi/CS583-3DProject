# Calligraphy Mini-Game Design Document

## Overview

Player traces strokes on a Japanese calligraphy scroll. Simple strokes "awaken" a full phrase.

---

## Phrase: 一期一会 (Ichigo Ichie)

**Meaning:** "One time, one meeting" - Every encounter is once in a lifetime.

**Player traces:** 一 (ichi) - single horizontal stroke  
**Full reveal:** 一期一会 (complete phrase in calligraphy font)

---

## Architecture

### Prefab-Based Design

```
CalligraphyDesign (ScriptableObject)
├── paperPrefab ← Contains all visuals
├── phraseName
├── phraseReading
├── phraseMeaning
└── scrollPrefab (room item)

PaperPrefab (per-design, e.g., IchigoIchie_Paper.prefab)
├── CalligraphyPaper.cs ← Visual controller + stroke state
├── Paper (Quad with Collider) ← For raycast
├── Characters (positioned for this phrase)
├── StrokeGuides (positioned for this phrase)
└── StrokeLines (LineRenderers)
```

### Script Separation

| Script               | Location | Responsibility                        |
| -------------------- | -------- | ------------------------------------- |
| CalligraphyGame.cs   | Scene    | IMiniGame, input (raycast), game flow |
| CalligraphyPaper.cs  | Prefab   | Visuals, stroke data, state, events   |
| CalligraphyDesign.cs | Asset    | Data container (ScriptableObject)     |
| CalligraphyResult.cs | Code     | Completion data                       |

### Input Approach: Pure Raycast

**Why not OnMouse events like PlacementSpot?**

| PlacementSpot         | Calligraphy                       |
| --------------------- | --------------------------------- |
| Discrete (click once) | Continuous (click, drag, release) |
| No position tracking  | Need cursor position every frame  |

**Result:** Raycast already required for line → Use raycast for everything.

---

## Camera Flow

### Camera Positions

| Position      | Name                        | Purpose         |
| ------------- | --------------------------- | --------------- |
| Room          | `roomPosition`              | Normal gameplay |
| Wide Paper    | `calligraphyWidePosition`   | See full paper  |
| Zoomed Stroke | `calligraphyZoomedPosition` | Focus on stroke |

### Full Transition Sequence

```
┌─────────────────────────────────────────────────────────────────┐
│                         CAMERA FLOW                             │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  1. Player clicks placement spot                                │
│         ↓                                                       │
│  2. Camera → Wide Paper View (0.5s)                             │
│         ↓                                                       │
│  3. Paper spawns, shows full 一期一会                            │
│         ↓ (1.0s pause)                                          │
│  4. Camera → Zoomed Stroke View (0.3s)                          │
│         ↓                                                       │
│  5. Player traces stroke                                        │
│         ↓                                                       │
│  6. Stroke complete → Character turns gold                      │
│         ↓                                                       │
│  7. Camera → Wide Paper View (0.3s)                             │
│         ↓                                                       │
│  8. Success UI appears ("一期一会 - Ichigo Ichie")              │
│         ↓ (2.0s pause)                                          │
│  9. Magic reveal effect (1.0s)                                  │
│         ↓                                                       │
│  10. Camera → Room View (0.5s)                                  │
│         ↓                                                       │
│  11. Paper placed in room                                       │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Timing Summary

| Phase           | Duration | What Happens          |
| --------------- | -------- | --------------------- |
| Camera → Wide   | 0.5s     | Smooth transition     |
| Show Full Paper | 1.0s     | Player sees phrase    |
| Camera → Zoomed | 0.3s     | Focus on stroke       |
| Player Traces   | Variable | Input enabled         |
| Camera → Wide   | 0.3s     | After stroke complete |
| Success Display | 2.0s     | Show phrase info      |
| Reveal Effect   | 1.0s     | Magic animation       |
| Camera → Room   | 0.5s     | Return to room        |

---

## Game Flow

### Phase 1: Setup (Wide View)

```
┌─────────────────────────────────────┐
│                                     │
│         一期一会                     │
│         ↑                           │
│       Gray  Black Black Black       │
│                                     │
│    Camera shows full paper          │
│    Player sees entire phrase        │
│                                     │
└─────────────────────────────────────┘
```

- Camera transitions to wide view
- CalligraphyGame spawns paperPrefab
- Brief pause to let player see phrase

### Phase 2: Zoom to Stroke

```
┌─────────────────────────────────────┐
│                                     │
│              一                      │
│         ↑                           │
│       Gray character                │
│                                     │
│    Camera zoomed in                 │
│    Points hidden until hover        │
│                                     │
└─────────────────────────────────────┘
```

- Camera zooms to stroke area
- Only current character visible
- Input enabled

### Phase 3: Hover Near Start

```
┌─────────────────────────────────────┐
│                                     │
│              一                      │
│         ●                           │
│         ↑                           │
│    StartPoint appears (green)       │
│                                     │
└─────────────────────────────────────┘
```

- Raycast detects cursor near start position
- CalligraphyPaper.ShowStartHighlight(true)
- Signifies "click here to begin"

### Phase 4: Drawing

```
┌─────────────────────────────────────┐
│                                     │
│              一                      │
│                                     │
│         ●━━━━━━━━━━                 │
│         ↑          ↵               │
│    StartPoint   Cursor moving       │
│                                     │
│    Green LineRenderer follows       │
│                                     │
└─────────────────────────────────────┘
```

- Mouse down on start → drawing begins
- LineRenderer follows cursor (green)
- CalligraphyGame.Update() does raycast every frame

### Phase 5: Stroke Complete

```
┌─────────────────────────────────────┐
│                                     │
│              一                      │
│         ↑                           │
│       Gold character                │
│                                     │
│         ━━━━━━━━━━━━━━━━━           │
│         ↑                           │
│    Black line (permanent)           │
│                                     │
└─────────────────────────────────────┘
```

- Mouse released near end point
- Line turns black (permanent)
- Character 一 changes to gold
- CalligraphyPaper fires OnStrokeCompleted

### Phase 6: Success Display (Wide View)

```
┌─────────────────────────────────────┐
│                                     │
│         一期一会                     │
│         ↑                           │
│       Gold  Black Black Black       │
│                                     │
│    ┌─────────────────────────┐      │
│    │      一期一会            │      │
│    │     Ichigo Ichie        │      │
│    │ "Once-in-a-lifetime..." │      │
│    └─────────────────────────┘      │
│                                     │
└─────────────────────────────────────┘
```

- Camera returns to wide view
- Success UI panel appears
- Shows phrase, reading, meaning

### Phase 7: Magic Reveal

```
┌─────────────────────────────────────┐
│                                     │
│      ✨ 一期一会 ✨                  │
│         ↑                           │
│     All Gold + Glow Effect          │
│                                     │
│    Particles, shimmer               │
│                                     │
└─────────────────────────────────────┘
```

- Success UI hides
- Full phrase reveals with gold + magic effect
- CalligraphyGame fires OnGameCompleted

### Phase 8: Return to Room

```
- Camera transitions back to room
- MiniGameController places scroll
- Player continues exploring
```

---

## Script Details

### CalligraphyDesign.cs (ScriptableObject)

```csharp
[CreateAssetMenu(fileName = "NewCalligraphyDesign", menuName = "MiniGames/Calligraphy Design")]
public class CalligraphyDesign : ScriptableObject
{
    [Header("Design Info")]
    public string phraseName;        // "一期一会"
    public string phraseReading;     // "Ichigo Ichie"
    public string phraseMeaning;     // "Once-in-a-lifetime encounter"

    [Header("Prefabs")]
    public GameObject paperPrefab;   // Contains CalligraphyPaper + all visuals
    public GameObject scrollPrefab;  // Room item after completion
}
```

### CalligraphyPaper.cs (On Prefab)

```csharp
public class CalligraphyPaper : MonoBehaviour
{
    [Header("Stroke Data")]
    public List<StrokeData> strokes;  // Configured per-prefab

    [Header("Visual References")]
    public Transform charactersParent;
    public Transform strokeLinesParent;

    // Events
    public event Action<int> OnStrokeCompleted;
    public event Action OnAllStrokesCompleted;

    // State
    private int currentStrokeIndex = 0;
    private bool isDrawing = false;

    // Methods
    public Vector3 GetCurrentStrokeStart();
    public Vector3 GetCurrentStrokeEnd();
    public void StartDrawing();
    public void UpdateLine(Vector3 worldPoint);
    public void CompleteStroke();
    public void CancelStroke();
    public void ShowStartHighlight(bool show);
}

[Serializable]
public class StrokeData
{
    public Vector3 startPoint;    // Local position
    public Vector3 endPoint;      // Local position
    public float tolerance;       // Distance threshold
    public int characterIndex;    // Which character this stroke awakens
    public LineRenderer lineRenderer;
}
```

### CalligraphyGame.cs (IMiniGame)

```csharp
public class CalligraphyGame : MonoBehaviour, IMiniGame
{
    [Header("Design")]
    [SerializeField] private CalligraphyDesign currentDesign;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private LayerMask paperLayer;

    [Header("Camera Positions")]
    [SerializeField] private Transform wideViewPosition;
    [SerializeField] private Transform zoomedViewPosition;

    [Header("Settings")]
    [SerializeField] private float hoverRadius = 0.5f;
    [SerializeField] private float endTolerance = 0.3f;

    [Header("Timing")]
    [SerializeField] private float initialPauseTime = 1.0f;
    [SerializeField] private float successDisplayTime = 2.0f;
    [SerializeField] private float revealEffectTime = 1.0f;

    [Header("References")]
    [SerializeField] private CameraController cameraController;
    [SerializeField] private CalligraphyUI calligraphyUI;

    // Events
    public event Action<CalligraphyResult> OnGameCompleted;

    // State
    private CalligraphyPaper activePaper;
    private GameObject spawnedPaper;
    private Camera mainCamera;
    private GameState state = GameState.Inactive;

    private enum GameState
    {
        Inactive,
        TransitioningToWide,
        ShowingFullPaper,
        TransitioningToZoomed,
        WaitingToStart,
        Drawing,
        TransitioningToWideAfterStroke,
        ShowingSuccess,
        RevealEffect,
        TransitioningToRoom
    }

    public void StartGame()
    {
        StartCoroutine(GameSequence());
    }

    private IEnumerator GameSequence()
    {
        // Phase 1: Transition to wide view
        state = GameState.TransitioningToWide;
        cameraController.MoveToPosition(wideViewPosition);
        yield return new WaitForSeconds(cameraController.TransitionDuration);

        // Phase 2: Spawn paper, show full phrase
        state = GameState.ShowingFullPaper;
        SpawnPaper();
        yield return new WaitForSeconds(initialPauseTime);

        // Phase 3: Zoom to stroke
        state = GameState.TransitioningToZoomed;
        cameraController.MoveToPosition(zoomedViewPosition);
        yield return new WaitForSeconds(cameraController.TransitionDuration);

        // Phase 4: Enable input
        state = GameState.WaitingToStart;
    }

    private void Update()
    {
        if (state != GameState.WaitingToStart && state != GameState.Drawing)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, paperLayer))
            return;

        Vector3 cursorWorld = hit.point;

        if (state == GameState.WaitingToStart)
        {
            HandleWaitingState(cursorWorld);
        }
        else if (state == GameState.Drawing)
        {
            HandleDrawingState(cursorWorld);
        }
    }

    private IEnumerator PostStrokeSequence()
    {
        // Transition back to wide view
        state = GameState.TransitioningToWideAfterStroke;
        cameraController.MoveToPosition(wideViewPosition);
        yield return new WaitForSeconds(cameraController.TransitionDuration);

        // Show success UI
        state = GameState.ShowingSuccess;
        calligraphyUI.ShowSuccess(currentDesign);
        yield return new WaitForSeconds(successDisplayTime);

        // Play reveal effect
        state = GameState.RevealEffect;
        calligraphyUI.HideSuccess();
        activePaper.PlayRevealEffect();
        yield return new WaitForSeconds(revealEffectTime);

        // Transition back to room
        state = GameState.TransitioningToRoom;
        cameraController.MoveToRoom();
        yield return new WaitForSeconds(cameraController.TransitionDuration);

        // Complete
        CompleteGame();
    }

    public void StopGame()
    {
        StopAllCoroutines();
        state = GameState.Inactive;
        if (spawnedPaper != null) Destroy(spawnedPaper);
    }
}
```

### CalligraphyUI.cs (Screen Space UI)

```csharp
public class CalligraphyUI : MonoBehaviour
{
    [Header("Success Panel")]
    [SerializeField] private GameObject successPanel;
    [SerializeField] private TextMeshProUGUI phraseText;
    [SerializeField] private TextMeshProUGUI readingText;
    [SerializeField] private TextMeshProUGUI meaningText;

    public void ShowSuccess(CalligraphyDesign design)
    {
        phraseText.text = design.phraseName;
        readingText.text = design.phraseReading;
        meaningText.text = design.phraseMeaning;
        successPanel.SetActive(true);
    }

    public void HideSuccess()
    {
        successPanel.SetActive(false);
    }
}
```

### CalligraphyResult.cs

```csharp
public class CalligraphyResult
{
    public CalligraphyDesign design;
    public GameObject roomItemPrefab;
}
```

---

## Unity Hierarchy (Per-Prefab)

```
IchigoIchie_Paper.prefab
├── CalligraphyPaper.cs
├── Paper (Quad + BoxCollider) ← For raycast hit
│   └── Material: Scroll texture
├── Characters
│   ├── Char_一 (TextMeshPro, gray → gold)
│   ├── Char_期 (TextMeshPro, black)
│   ├── Char_一2 (TextMeshPro, black)
│   └── Char_会 (TextMeshPro, black)
├── StrokeGuides (optional visual guides)
│   └── Guide_Stroke1 (faint line showing path)
└── StrokeLines
    └── Line_Stroke1 (LineRenderer, initially hidden)
```

**Key:** Only Paper has a collider - used for all raycast detection.

---

## Event Flow

```
User hovers near start
    → CalligraphyGame.Update() detects proximity
    → CalligraphyPaper.ShowStartHighlight(true)

User clicks
    → CalligraphyGame starts drawing mode
    → CalligraphyPaper.StartDrawing()

User drags
    → CalligraphyGame.Update() raycasts
    → CalligraphyPaper.UpdateLine(point)

User releases near end
    → CalligraphyGame detects valid end
    → CalligraphyPaper.CompleteStroke()
    → CalligraphyPaper fires OnStrokeCompleted

All strokes done
    → CalligraphyPaper fires OnAllStrokesCompleted
    → CalligraphyGame fires OnGameCompleted
    → MiniGameController handles result
```

---

## Visual Feedback

| State        | LineRenderer | Start Point | Character |
| ------------ | ------------ | ----------- | --------- |
| Idle         | Hidden       | Hidden      | Gray      |
| Hover Start  | Hidden       | Green glow  | Gray      |
| Drawing      | Green line   | Visible     | Gray      |
| Complete     | Black line   | Hidden      | Gold      |
| All Complete | Black lines  | Hidden      | All Gold  |

---

## Development Phases

### Phase 1: Minimal Testable Core

**Goal:** See paper spawn and raycast working

**Files:**

1. `CalligraphyDesign.cs` - ScriptableObject (data only)
2. `CalligraphyPaper.cs` - Stub (just holds references)
3. `CalligraphyGame.cs` - Minimal (spawn paper, raycast debug)

**Test:** Click play → paper spawns → Debug.Log shows raycast hit position

---

### Phase 2: Line Drawing

**Goal:** Draw line from start to cursor

**Files:**

1. `CalligraphyPaper.cs` - Add StartDrawing(), UpdateLine()

**Test:** Click on paper → green line follows cursor → release resets

---

### Phase 3: Stroke Completion

**Goal:** Complete stroke when releasing near end point

**Files:**

1. `CalligraphyPaper.cs` - Add CompleteStroke(), CancelStroke(), events
2. `CalligraphyResult.cs` - Result data class

**Test:** Drag from start to end → line turns black → event fires

---

### Phase 4: Visual Feedback

**Goal:** Character color change, point highlights

**Files:**

1. `CalligraphyPaper.cs` - Add ShowStartHighlight(), character references

**Test:** Hover near start → green circle appears → complete stroke → character turns gold

---

### Phase 5: Camera Transitions

**Goal:** Full camera flow working

**Files:**

1. `CalligraphyGame.cs` - Add coroutine sequence, camera positions
2. `CameraController.cs` - Add MoveToPosition() if not exists

**Test:** Start game → wide view → zoom → draw → wide view → room

---

### Phase 6: Success UI

**Goal:** Show phrase info after completion

**Files:**

1. `CalligraphyUI.cs` - New script for UI panel

**Unity Setup:**

- Create Canvas with SuccessPanel

**Test:** Complete stroke → UI shows phrase/reading/meaning → hides after delay

---

### Phase 7: Integration

**Goal:** Connect to MiniGameController

**Files:**

1. `MiniGameController.cs` - Add calligraphy handling

**Test:** Click placement spot → full flow → scroll placed in room

---

### Phase 8: Polish

**Goal:** Effects and final touches

**Files:**

1. `CalligraphyPaper.cs` - Add PlayRevealEffect()

**Unity Setup:**

- Particle effects
- Sound effects

**Test:** Full flow with all visual/audio polish

---

## File Creation Order

| Order | File                             | Depends On    | Testable After        |
| ----- | -------------------------------- | ------------- | --------------------- |
| 1     | `CalligraphyDesign.cs`           | Nothing       | Create asset in Unity |
| 2     | `CalligraphyResult.cs`           | Nothing       | Compile check         |
| 3     | `CalligraphyPaper.cs` (stub)     | Nothing       | Attach to prefab      |
| 4     | `CalligraphyGame.cs` (minimal)   | Design, Paper | Spawn + raycast       |
| 5     | `CalligraphyPaper.cs` (line)     | -             | Line drawing          |
| 6     | `CalligraphyPaper.cs` (complete) | Result        | Stroke completion     |
| 7     | `CalligraphyUI.cs`               | Design        | UI display            |
| 8     | `CalligraphyGame.cs` (full)      | UI, Camera    | Full sequence         |

---

## Testing Checkpoints

### Checkpoint 1: Basic Setup ✅

```
☑ CalligraphyDesign asset created
☑ Paper prefab with collider
☑ CalligraphyGame spawns paper
☑ Raycast hits paper (Debug.Log)
```

### Checkpoint 2: Drawing ✅

```
☑ Click near start point detected
☑ LineRenderer appears on click
☑ Line follows cursor position
☑ Line resets on release
```

### Checkpoint 3: Completion ✅

```
☑ Release near end point detected
☑ Line changes to black
☑ OnStrokeCompleted event fires
☑ Character changes to gold
```

### Checkpoint 4: Camera ✅

```
☑ Camera moves to wide view
☑ Camera moves to zoomed view
☑ Camera returns to wide after stroke
☑ Camera returns to room
```

### Checkpoint 5: Full Flow ✅

```
☑ Placement spot triggers mini-game
☑ Complete stroke shows success UI
☐ Reveal effect plays (Phase 8)
☑ Scroll placed in room
```

---

## Implementation Status

| Phase   | Description                                      | Status         |
| ------- | ------------------------------------------------ | -------------- |
| Phase 1 | Basic Setup - Spawn paper, raycast               | ✅ Complete    |
| Phase 2 | Line Drawing - Green line follows cursor         | ✅ Complete    |
| Phase 3 | Stroke Completion - Line turns black, events     | ✅ Complete    |
| Phase 4 | Visual Feedback - Highlights, character color    | ✅ Complete    |
| Phase 5 | Camera Transitions - Wide → Zoom → Wide → Room   | ✅ Complete    |
| Phase 6 | Success UI - Show panel after completion         | ✅ Complete    |
| Phase 7 | Integration - MiniGameController, room placement | ✅ Complete    |
| Phase 8 | Polish - Effects and final touches               | 🔲 Not Started |

### Phase 8 Remaining Tasks:

- [ ] Magic reveal effect (particles when stroke completes)
- [ ] Sound effects (brush stroke, completion chime)
- [ ] Character animation (gold shimmer/glow)
- [ ] Line polish (brush stroke appearance)
