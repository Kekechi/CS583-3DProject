# Game Loop Setup Guide

This guide walks through setting up the scene transition system and main menu.

---

## Step 1: Create TransitionCanvas Prefab

The TransitionCanvas is a persistent UI element that handles fade transitions between scenes.

### 1.1 Create the Canvas

1. In **any scene**, create: `GameObject > UI > Canvas`
2. Rename to **"TransitionCanvas"**
3. Configure Canvas component:
   - **Render Mode**: Screen Space - Overlay
   - **Sort Order**: 999 (render above everything else)

### 1.2 Add Fade Panel

1. Under TransitionCanvas, create: `UI > Panel`
2. Rename to **"FadePanel"**
3. Configure RectTransform:
   - Anchors: Stretch-Stretch (all corners)
   - Left/Right/Top/Bottom: 0
4. Configure Image component:
   - **Color**: Black (0, 0, 0, 255)
5. Add **CanvasGroup** component:
   - Alpha: 0
   - Blocks Raycasts: false
   - Interactable: false

### 1.3 Add TransitionManager Script

1. Select **TransitionCanvas**
2. Add **TransitionManager** script component
3. Drag **FadePanel** to the "Fade Canvas Group" field
4. Settings (defaults are fine):
   - Default Fade Duration: 0.5
   - Hold Duration: 0.2

### 1.4 Make Prefab

1. Drag TransitionCanvas from Hierarchy to `Assets/Prefabs/`
2. Delete from scene (we'll place in MainMenu scene)

---

## Step 2: Create MainMenu Scene

### 2.1 Create Scene

1. `File > New Scene > Basic (Built-in)`
2. Save as `Assets/Scenes/MainMenu.unity`

### 2.2 Add TransitionCanvas

1. Drag **TransitionCanvas** prefab into scene
2. This must be in the first loaded scene!

### 2.3 Create Menu Canvas

1. Create: `GameObject > UI > Canvas`
2. Rename to **"MenuCanvas"**
3. Configure:
   - Render Mode: Screen Space - Overlay
   - Sort Order: 0

### 2.4 Add Background (optional)

1. Under MenuCanvas, create: `UI > Image`
2. Rename to **"Background"**
3. Stretch to fill, set color or sprite

### 2.5 Add Title Text

1. Create: `UI > Text - TextMeshPro`
2. Rename to **"TitleText"**
3. Position at top-center
4. Text: "Japanese Room" (or your game title)
5. Font Size: 72+
6. Alignment: Center

### 2.6 Create Button Container

1. Create: `UI > Panel` (or Empty)
2. Rename to **"ButtonPanel"**
3. Position at center
4. Add **Vertical Layout Group**:
   - Spacing: 20
   - Child Alignment: Middle Center
   - Control Child Size: Width (checked)

### 2.7 Add Menu Buttons

For each button (Start, Store, Exit):

1. Under ButtonPanel, create: `UI > Button - TextMeshPro`
2. Rename appropriately (StartButton, StoreButton, ExitButton)
3. Configure button size: ~300x60
4. Set button text

### 2.8 Add MainMenuUI Script

1. Create empty GameObject, rename to **"MainMenuController"**
2. Add **MainMenuUI** script
3. Drag buttons to inspector fields:
   - Start Button → StartButton
   - Store Button → StoreButton
   - Exit Button → ExitButton
4. Game Scene Name should be: `SampleScene`

---

## Step 3: Configure Build Settings

### 3.1 Add Scenes to Build

1. `File > Build Settings`
2. Click "Add Open Scenes" for MainMenu
3. Open SampleScene and add it too
4. **MainMenu should be index 0** (first scene loaded)

Scene order should be:

```
0: Scenes/MainMenu
1: Scenes/SampleScene
```

---

## Step 4: Test the Flow

### 4.1 Basic Test

1. Open MainMenu scene
2. Enter Play mode
3. Click "Start" button
4. Should fade to black → load SampleScene → fade in

### 4.2 Verify in SampleScene

- TransitionCanvas should persist (DontDestroyOnLoad)
- Check Console for: `[TransitionManager] Transition complete`

---

## Future Additions

### Return to Menu from Game

To add "Return to Menu" button in-game:

```csharp
public void OnReturnToMenuClicked()
{
    SceneLoader.LoadScene("MainMenu");
}
```

### Room Complete → Menu

The RoomCompleteUI (Phase C) will use the same pattern:

```csharp
public void OnContinueClicked()
{
    SceneLoader.LoadScene("MainMenu");
}
```

---

## Troubleshooting

### Fade not working

- Check FadePanel has CanvasGroup component
- Check TransitionManager has reference to FadePanel's CanvasGroup
- Check Console for warnings

### Scene not loading

- Verify scene is in Build Settings
- Check scene name matches exactly (case-sensitive)
- Check Console for errors

### Duplicate TransitionManager

- This is normal! The singleton pattern destroys duplicates
- Console will show: `[TransitionManager] Duplicate detected, destroying self`

### Buttons not responding

- Check MainMenuUI has button references assigned
- Check buttons have correct OnClick events (or rely on AddListener in code)
