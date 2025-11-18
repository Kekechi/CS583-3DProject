# 🏯 Project Summary – The Little Zen Atelier

A calm, minimalist 3D Unity simulation game inspired by Japanese arts.
The player completes three short, non-violent mini-games — Origami, Calligraphy, and Lantern — to create decorative items and bring harmony to small Zen rooms.
Each finished room becomes a relaxing space with soft lighting, gentle camera motion, and ambient sound.

## ⚙️ Core Features

Main Menu → start game, select room, settings.

Mini-games (UI-based prefabs):

Origami: press correct arrow keys for accuracy.

Calligraphy: keep brush speed consistent (hold/release rhythm).

Lantern: hold/release to keep a brightness bar in the green “harmony” zone.

Guided Placement: each crafted item is placed in a fixed highlighted spot within the room.

Harmony Meter: fills as crafts are placed; when full, triggers room completion animation.

Menus: pause and summary screens.

Lighting: ambient + lantern glow.

Camera: smooth pans/zooms between crafting, placement, and completion scenes.

Animation: folding, brush flow, lantern glow via Unity animation or simple kinematic scripts.

## 🧩 Architecture Notes

Single Unity scene for simplicity (recommended for 1-month dev).

Prefabs: each mini-game, placement spot, and UI panel.

Managers: GameManager, UIManager, AudioManager, RoomController, MiniGameController.

Shared Input/UI: reusable “SuccessBar” script (green zone logic).

Data-driven rooms: ScriptableObjects define lighting, theme, and mini-game difficulty.

Art: free CC0 textures (tatami, fusuma, wood) and simple 3D geometry.

Sound: gentle ambience, chimes, paper/ink SFX.

## 🗓️ Development Focus

Build main menu + GameManager state flow.

Implement one mini-game (Lantern) with SuccessBar UI → test full loop.

Clone logic for Origami and Calligraphy.

Add placement + room completion sequence.

Integrate audio, polish lighting and camera transitions.
