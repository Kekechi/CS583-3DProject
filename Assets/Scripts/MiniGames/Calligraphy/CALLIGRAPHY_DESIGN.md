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

## Game Flow

### Phase 1: Setup

```
┌─────────────────────────────────────┐
│                                     │
│         一期一会                     │
│         ↑                           │
│       Gray  Black Black Black       │
│                                     │
│    (Start/End points hidden)        │
│                                     │
└─────────────────────────────────────┘
```

- CalligraphyGame spawns paperPrefab
- Points are hidden until hover

### Phase 2: Hover Near Start

```
┌─────────────────────────────────────┐
│                                     │
│         一期一会                     │
│         ●                           │
│         ↑                           │
│    StartPoint appears (green)       │
│                                     │
└─────────────────────────────────────┘
```

- Raycast detects cursor near start position
- CalligraphyPaper.ShowStartHighlight(true)
- Signifies "click here to begin"

### Phase 3: Drawing

```
┌─────────────────────────────────────┐
│                                     │
│         一期一会                     │
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

### Phase 4: Stroke Complete

```
┌─────────────────────────────────────┐
│                                     │
│         一期一会                     │
│         ↑                           │
│       Gold  Black Black Black       │
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

### Phase 5: Magic Reveal

```
┌─────────────────────────────────────┐
│                                     │
│      ✨ 一期一会 ✨                  │
│         ↑                           │
│     All Gold + Glow Effect          │
│                                     │
│    "Ichigo Ichie" text appears      │
│    "Once-in-a-lifetime encounter"   │
│                                     │
└─────────────────────────────────────┘
```

- All strokes complete → CalligraphyPaper fires OnAllStrokesCompleted
- Full phrase reveals with gold + magic effect
- CalligraphyGame fires OnGameCompleted with CalligraphyResult

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
    [Header("Settings")]
    public float hoverRadius = 0.5f;
    public float endTolerance = 0.3f;

    [Header("Current Design")]
    private CalligraphyDesign currentDesign;
    private CalligraphyPaper activePaper;
    private Camera mainCamera;

    // Events
    public event Action<CalligraphyResult> OnGameCompleted;

    // State
    private bool isActive = false;
    private bool isDrawing = false;

    public void Initialize(CalligraphyDesign design)
    {
        currentDesign = design;
    }

    public void StartGame()
    {
        isActive = true;
        SpawnPaper();
        SubscribeToPaperEvents();
    }

    public void StopGame()
    {
        isActive = false;
        CleanupPaper();
    }

    private void Update()
    {
        if (!isActive) return;

        // Raycast for cursor position
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 cursorWorld = hit.point;

            if (!isDrawing)
            {
                // Check hover near start
                Vector3 startPos = activePaper.GetCurrentStrokeStart();
                bool nearStart = Vector3.Distance(cursorWorld, startPos) < hoverRadius;
                activePaper.ShowStartHighlight(nearStart);

                // Check click to start
                if (nearStart && Input.GetMouseButtonDown(0))
                {
                    isDrawing = true;
                    activePaper.StartDrawing();
                }
            }
            else
            {
                // Update line position
                activePaper.UpdateLine(cursorWorld);

                // Check release
                if (Input.GetMouseButtonUp(0))
                {
                    Vector3 endPos = activePaper.GetCurrentStrokeEnd();
                    if (Vector3.Distance(cursorWorld, endPos) < endTolerance)
                    {
                        activePaper.CompleteStroke();
                    }
                    else
                    {
                        activePaper.CancelStroke();
                    }
                    isDrawing = false;
                }
            }
        }
    }

    private void HandleAllStrokesCompleted()
    {
        var result = new CalligraphyResult
        {
            design = currentDesign,
            roomItemPrefab = currentDesign.scrollPrefab
        };
        OnGameCompleted?.Invoke(result);
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

### Phase 1: Core Scripts

- [ ] Create `CalligraphyDesign.cs` (ScriptableObject)
- [ ] Create `CalligraphyPaper.cs` (stub with events)
- [ ] Create `CalligraphyGame.cs` (IMiniGame stub)
- [ ] Create `CalligraphyResult.cs`

### Phase 2: Paper Mechanics

- [ ] Implement stroke state machine
- [ ] Line drawing with LineRenderer
- [ ] Start/end detection
- [ ] Character color transitions

### Phase 3: Prefab Setup

- [ ] Create paper prefab with placeholder
- [ ] Add character TextMeshPro objects
- [ ] Configure StrokeData list
- [ ] Add visual guides

### Phase 4: Game Integration

- [ ] Register in MiniGameController
- [ ] Wire up events
- [ ] Test flow end-to-end

### Phase 5: Polish

- [ ] Magic reveal effect (particles, glow)
- [ ] Sound effects
- [ ] Romanji + meaning text display
- [ ] Final calligraphy font
