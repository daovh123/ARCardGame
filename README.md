<div align="center">

# AR Card Arena

### Augmented Reality Multiplayer Card Game Platform

![Unity](https://img.shields.io/badge/Unity%206-6000.4-black?style=for-the-badge&logo=unity)
![Platform](https://img.shields.io/badge/Platform-Android-3DDC84?style=for-the-badge&logo=android)
![AR Foundation](https://img.shields.io/badge/AR%20Foundation-6.4-blue?style=for-the-badge)
![Photon](https://img.shields.io/badge/Photon%20PUN%202-Multiplayer-FF6F00?style=for-the-badge)
![URP](https://img.shields.io/badge/URP%2017.4-Rendering-8B5CF6?style=for-the-badge)
![License](https://img.shields.io/badge/License-Proprietary-red?style=for-the-badge)

**Team 10** | `com.yourteam.arcardarena`

*Place a marker on your table, point your phone camera, and play UNO or Tien Len with friends — cards float on your real table in augmented reality.*

</div>

---

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Screenshots](#screenshots)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Game Modes](#game-modes)
  - [UNO](#uno)
  - [Tien Len Mien Nam](#tien-len-mien-nam)
- [Multiplayer](#multiplayer)
- [AR System](#ar-system)
- [UI & Visual System](#ui--visual-system)
- [Audio System](#audio-system)
- [Project Structure](#project-structure)
- [Scene Flow](#scene-flow)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Setup](#setup)
  - [Build](#build)
- [Configuration](#configuration)
- [Controls](#controls)
- [How to Play](#how-to-play)
- [Team](#team)

---

## Overview

AR Card Arena is a mobile-first Unity application that brings classic card games into augmented reality. Using AR Foundation image tracking, the app detects a table marker through your phone camera and renders an interactive 3D card table directly on your real-world surface. Players can swipe cards from their hand to play them, tap the draw pile to draw, and watch cards animate through the air with parabolic arcs.

The platform supports two complete card games — **UNO** (2-4 players with bot AI) and **Tien Len Mien Nam** (4-player Vietnamese card game) — both playable offline and online via real-time multiplayer through Photon PUN 2.

Key design philosophy: **zero prefab dependency for UI**. Every panel, button, label, sprite, and texture is generated procedurally at runtime, enabling rapid theming and minimal asset overhead.

---

## Features

| Category | Feature |
|:---|:---|
| **Card Games** | Full UNO ruleset (2-4 players + bots) |
| | Full Tien Len Mien Nam ruleset (4 players) |
| | UNO stacking (+2/+4 chain mechanics) |
| | Tien Len bomb/chop mechanics |
| | Tien Len instant win conditions (four 2s, dragon straight, monochrome hand) |
| | Bot AI with configurable delay and strategy |
| **Augmented Reality** | AR image marker table detection |
| | 3D card spawning with BoxCollider interactions |
| | Swipe-to-play and swipe-to-draw gestures |
| | Camera-centered or marker-anchored placement |
| | Pose stabilization and smooth recentering |
| | Parabolic card play animations |
| | Turn indicator with floating/bobbing animation |
| | Victory confetti particles and billboard text |
| **Multiplayer** | Real-time online via Photon PUN 2 |
| | Room creation with 4-digit codes |
| | Ready system with host-controlled bot slots |
| | Full game state sync via room custom properties |
| | Visual event replay for late-joining clients |
| **UI System** | Fully runtime-generated UI (no prefabs) |
| | Procedural rounded rectangles, gradients, circles |
| | Dynamic color theming with cached sprite generation |
| | Card hover/press/select micro-interactions |
| | Toast notification system |
| | Pause menu with volume slider |
| **Audio** | 12 procedural SFX types (no audio file dependencies in scene) |
| | Volume and mute persistence via PlayerPrefs |
| | Context-sensitive sound effects per game action |

---

## Tech Stack

| Component | Version / Detail |
|:---|:---|
| **Engine** | Unity 6000.4.6f1 (Unity 6) |
| **Render Pipeline** | Universal Render Pipeline (URP) 17.4.0 |
| **Template** | URP Blank 17.0.14 |
| **AR Framework** | AR Foundation 6.4.3 + ARCore 6.4.3 |
| **Networking** | Photon Unity Networking 2 (PUN 2.19+) |
| **UI** | Unity UI (uGUI) + TextMesh Pro |
| **Input** | Unity Input System 1.19.0 |
| **Scripting** | C# / IL2CPP (Android) |
| **Target Platform** | Android (min SDK 26, OpenGL ES 3.0) |
| **App Identifier** | `com.yourteam.arcardarena` |

---

## Architecture

The codebase follows an **event-driven layered architecture** with clear separation between game logic, visual systems, and network synchronization.

```
+-----------------------------------------------------+
|                     UI Layer                         |
|  MainMenuManager, GameUIManager, TienLenUIManager,  |
|  PhotonLobbyManager, RuntimeUITheme, RuntimeSfx,    |
|  RuntimePauseMenu, CardUI, TienLenCardView           |
+-----------------------------------------------------+
|               Game Logic Layer                       |
|  GameManager (UNO), TienLenGameManager,              |
|  DeckManager, RuleChecker, TienLenRuleChecker,      |
|  CardData, PlayingCardData, PlayerData,              |
|  TienLenPlayerData, GameStateData                    |
+-----------------------------------------------------+
|               Event Bus Layer                        |
|  GameEvents (UNO), TienLenGameEvents,               |
|  GameVisualEventBridge, GameEventDebugListener       |
+-----------------------------------------------------+
|                  AR Layer                            |
|  ARImageTableTracker, ARTableController,             |
|  ARCardVisual, ARPlayingCardVisual,                  |
|  ARHandController, ARHandCard, ARDrawPileGesture,   |
|  ARCardSpawner, SpecialEffectSpawner,               |
|  UnoARGameEventBridge, TienLenARGameEventBridge      |
+-----------------------------------------------------+
|             Multiplayer Layer                        |
|  PhotonLobbyManager (PUN2),                          |
|  GameManager (Photon sync via room properties)       |
+-----------------------------------------------------+
```

### Design Patterns

| Pattern | Implementation |
|:---|:---|
| **Event Bus** | `GameEvents` and `TienLenGameEvents` use static `Action` delegates to decouple game logic from visual systems |
| **Bridge** | `UnoARGameEventBridge` and `TienLenARGameEventBridge` translate game events into AR visual calls |
| **Bootstrapper** | `GameSceneBootstrapper` activates UNO or Tien Len mode based on `GameModeSelection.CurrentMode` |
| **Singleton** | `RuntimeSfx` and `RuntimePauseMenu` use `DontDestroyOnLoad` for cross-scene persistence |
| **ScriptableObject Database** | `CardSpriteDatabase`, `TienLenCardAssetDatabase`, `TienLenThemeAssetDatabase` store card sprites and textures |
| **Procedural Generation** | `RuntimeUITheme` creates all UI elements as `Texture2D` at runtime with caching |

---

## Game Modes

### UNO

Classic UNO card game with full rule implementation.

| Rule | Detail |
|:---|:---|
| **Deck** | 108 cards: 4 colors x 13 (0-9, Skip, Reverse, Draw Two) + 4 Wild + 4 Wild Draw Four |
| **Matching** | Match by color, number, or symbol; Wild cards always playable |
| **Stacking** | +2 and +4 can be stacked; next player must stack or draw the full accumulated penalty |
| **Skip** | Skips the next player's turn |
| **Reverse** | Reverses play direction; acts as Skip in 2-player mode |
| **UNO Call** | Must declare UNO when hand reaches 2 cards; forgetting draws 2 penalty cards |
| **Elimination** | Player exceeding 25 cards is eliminated |
| **Win Condition** | First player to empty their hand wins; game continues to rank all remaining players |
| **Bot AI** | Master client runs bots with 0.85s turn delay; prefers non-wild cards first |

### Tien Len Mien Nam

Vietnamese card game (also known as "Thirteen" or "Southern Tien Len") with complete rule set.

| Rule | Detail |
|:---|:---|
| **Deck** | 52 standard cards |
| **Rank Order** | 3, 4, 5, 6, 7, 8, 9, 10, J, Q, K, A, 2 (2 is highest) |
| **Suit Order** | Spades < Clubs < Diamonds < Hearts |
| **Starting** | Player holding 3 of Spades leads first; may play any valid set |
| **Valid Sets** | Single, Pair, Triple, Straight (3+ consecutive, no 2s), Four-of-a-kind, Consecutive Pairs (3+ pairs) |
| **Beating** | Same type and count at higher rank; ties broken by suit |
| **Four-kind Bomb** | Four-of-a-kind beats a single 2 or a pair of 2s |
| **Three Pairs Chop** | 3 consecutive pairs beat a single 2 |
| **Four+ Pairs Chop** | 4+ consecutive pairs beat single 2, pair of 2s, or four-of-a-kind |
| **Instant Win** | Four 2s in hand, all-red/all-black 13-card hand, or dragon straight (A through K) |

---

## Multiplayer

Real-time multiplayer powered by **Photon PUN 2**.

### Connection Flow

```
MainMenu -> LobbyScene -> Photon Connect -> Create/Join Room -> Ready -> Start -> ARMultiplayerGameScene
```

### Room System

- Rooms created with random **4-digit codes** for easy sharing
- Maximum **4 players** per room (human + bot combination)
- Host controls bot count with +/- buttons
- Ready system via player custom properties
- Copy room code to clipboard

### State Synchronization

- Master client serializes complete `GameStateData` to **room custom properties** as JSON
- All clients deserialize state on `OnRoomPropertiesUpdate`
- `visualEventSequence` counter ensures consistent card play/draw/game-over animations across all clients
- Player seat mapping: network-aware slot assignment with local player always at position 0

---

## AR System

### Image Marker Tracking (`ARImageTableTracker`)

The AR system uses AR Foundation's `ARTrackedImageManager` to detect a designated table marker.

| Parameter | Value | Description |
|:---|:---|:---|
| `TableMarker` | Named reference | The tracked image target |
| `stableLockTime` | 0.2s | Time before pose locks to prevent jitter |
| `settleTime` | 0.65s | Camera idle time before recentering |
| `tableScale` | 0.86 | Scale multiplier for the AR table |
| `cameraDistance` | 0.82m | Distance from camera to table |
| `tiltAngle` | 20 degrees | Table tilt for comfortable viewing |

### Placement Modes

- **Camera-centered** (default): Table spawns relative to camera position
- **Marker-anchored**: Table locks to detected image marker position

### AR Table (`ARTableController`)

- 4 player slots: South, West, North, East
- Draw pile and discard pile with stacked card visualization (max 8 visible)
- **Card play animation**: Parabolic arc from player slot to discard pile
- **Draw animation**: Card flies from draw pile to player slot with size shrink
- **Turn indicator**: Floating ring that bobs and rotates, smoothly transitions between slots
- **Victory effects**: Confetti particle system + billboard winner text

### Hand Interaction (`ARHandController`)

- Hand anchored to AR camera (follows device movement)
- Cards spawned as 3D objects with `BoxCollider` triggers
- **Swipe-to-play**: Touch/mouse swipe up on a card plays it
- **Swipe-to-draw**: Swipe on draw pile draws a card
- **Visual states**: Normal (dimmed), Playable (highlighted), Selected (bright yellow)
- **Wild card**: Color choice delegated to `GameUIManager` overlay
- Auto-refreshes hand display every 0.25s

### Card Visuals

| Component | Game | Scale | Detail |
|:---|:---|:---|:---|
| `ARCardVisual` | UNO | 0.09m x 0.135m | Front/back SpriteRenderers |
| `ARPlayingCardVisual` | Tien Len | 0.06m x 0.09m | Sprite with fallback TextMesh |

---

## UI & Visual System

### Runtime UI Theme (`RuntimeUITheme`)

All UI is generated procedurally at runtime — no prefab dependencies.

**Color Palette:**

| Name | Hex | Usage |
|:---|:---|:---|
| Ink | `#030810` | Dark backgrounds, text |
| Panel | `#051015F0` | Overlay panels |
| Felt | `#055240F5` | Table surface |
| Gold | `#FFC23E` | Highlights, accents |
| Cyan | `#2EF3DB` | Interactive elements |
| Red | `#DC141F` | Danger, error states |
| Blue | `#1466EB` | Information |

**Generated Elements:** Rounded rectangles, gradients, circles, shadows — all cached as `Texture2D` sprites in a dictionary.

### Game UI (`GameUIManager` / `TienLenUIManager`)

- **UNO HUD**: Background, table surface, top card display, draw pile stack, 4 seat panels, hand tray, action rail, UNO button, color choice panel, toast notifications, game over panel with rankings
- **Tien Len HUD**: Title bar, table with face-up cards, 4 seat panels with card counts, hand panel with tap-to-select, play/pass buttons, ranking panel with gold/red/blue rows

### Card Interactions

| Component | Game | Interactions |
|:---|:---|:---|
| `CardUI` | UNO | Hover scale 1.055x, press scale 0.97x |
| `TienLenCardView` | Tien Len | Tap-to-select with 18px rise animation, hover shadow, invalid flash shake |

---

## Audio System

### Runtime SFX (`RuntimeSfx`)

Fully procedural audio system — loads clips from `Resources/Audio/SFX/` at runtime.

| SFX Type | Trigger | Primary Clip |
|:---|:---|:---|
| Click | UI interaction | `sfx-card-select.mp3` |
| Draw | Card drawn | `sfx-card-draw-comm-2-new.mp3` |
| Play | Card played | `sfx-card-opendeck.mp3` |
| Error | Invalid action | `sfx-card-pick.mp3` |
| Special | Skip/Reverse/Draw | `uno-sfx-arrowswitch.mp3` |
| Turn | Turn change | `uno-sfx-card-deal-comm.mp3` |
| Win | Game won | `sfx-ui-victory-token.mp3` |
| Uno | UNO declared | `uno.wav` |
| Pass | Player passes | `sfx-card-pick.mp3` |
| Bomb | Four-kind bomb | `sfx-gamestart-end.mp3` |
| Lose | Player loses | `sfx-gamestart-end.mp3` |
| RoundComplete | Round end | `sfx-gamestart.mp3` |

**Persistence**: SFX enabled/volume saved via `PlayerPrefs` with keys `RuntimeSfx.Enabled` and `RuntimeSfx.Volume`.

---

## Project Structure

```
Assets/
├── Animations/                    # Animation clips and controllers
├── AR/                            # AR tracking scripts and bridges
├── Audio/                         # (SFX loaded from Resources)
├── Materials/ARVisual/            # Deck, TableSurface, TurnIndicator materials
├── Photon/                        # Photon PUN 2 library
├── Prefabs/
│   ├── AR/                        # ARCardVisualPrefab
│   ├── ARTrackingTest/            # Test tracking prefabs
│   ├── ARVisual/                  # ARCardPrefab, ARTableRoot
│   └── UI/                        # CardUIPrefab
├── Resources/
│   ├── Audio/SFX/                 # 12 sound effect files
│   ├── CardSpriteDatabase.asset   # UNO card sprites (54 entries)
│   └── TienLen/                   # TienLenCardAssetDatabase, TienLenThemeAssetDatabase
├── Scenes/
│   ├── MainMenuScene.unity        # Main menu with game mode selection
│   ├── GameScene.unity            # Offline game (UNO + Tien Len)
│   ├── LobbyScene.unity           # Multiplayer lobby
│   ├── ARMultiplayerGameScene.unity # AR + multiplayer combined scene
│   └── ARVisualTestScene.unity    # Editor AR visual testing
├── Scripts/
│   ├── AR/                        # AR tracking, event bridges, spawners
│   │   ├── Tracking/              # ARImageTableTracker
│   │   ├── ARCardSpawner.cs       # 3D card instantiation
│   │   ├── ARGameplayOverlayController.cs
│   │   ├── SpecialEffectSpawner.cs
│   │   ├── UnoARGameEventBridge.cs
│   │   └── TienLenARGameEventBridge.cs
│   ├── ARGameplay/                # AR hand interaction
│   │   ├── ARHandController.cs    # Hand management + swipe input
│   │   ├── ARHandCard.cs          # Individual hand card state
│   │   └── ARDrawPileGesture.cs   # Draw pile swipe detection
│   ├── ARVisual/                  # AR visual components
│   │   ├── ARTableController.cs   # Table layout + animations
│   │   ├── ARCardVisual.cs        # UNO 3D card
│   │   ├── ARPlayingCardVisual.cs # Tien Len 3D card
│   │   ├── ARGameEventBridge.cs   # Base bridge class
│   │   ├── ARVisualTestHelper.cs  # Editor test utility
│   │   └── Editor/                # Custom editor tools
│   ├── Audio/
│   │   └── RuntimeSfx.cs          # Procedural SFX system
│   ├── Game/                      # Core game logic (UNO)
│   │   ├── GameManager.cs         # Main UNO game loop (1501 lines)
│   │   ├── DeckManager.cs         # Deck shuffle/deal
│   │   ├── RuleChecker.cs         # UNO move validation
│   │   ├── CardData.cs            # UNO card data model
│   │   ├── PlayerData.cs          # Player state
│   │   ├── GameStateData.cs       # Serializable game state
│   │   ├── GameEvents.cs          # UNO event bus
│   │   ├── GameModeSelection.cs   # Game mode enum + static state
│   │   ├── GameSceneBootstrapper.cs # Mode-based scene activation
│   │   ├── GameVisualEventBridge.cs # UI event bridge
│   │   └── GameEventDebugListener.cs # Debug logging
│   ├── Multiplayer/
│   │   └── PhotonLobbyManager.cs  # Photon room management (819 lines)
│   ├── TienLen/                   # Tien Len game logic
│   │   ├── TienLenGameManager.cs  # Main Tien Len loop (747 lines)
│   │   ├── TienLenRuleChecker.cs  # Combination validation (253 lines)
│   │   ├── TienLenCombination.cs  # Combination data model
│   │   ├── PlayingCardData.cs     # Standard card data model
│   │   ├── TienLenPlayerData.cs   # Tien Len player state
│   │   ├── TienLenGameEvents.cs   # Tien Len event bus
│   │   └── TienLenFeedbackSnapshot.cs # Visual feedback state
│   └── UI/                        # UI managers and utilities
│       ├── MainMenuManager.cs     # Main menu logic
│       ├── GameUIManager.cs       # UNO game HUD (1300+ lines)
│       ├── TienLenUIManager.cs    # Tien Len HUD (1275 lines)
│       ├── LobbyManager.cs        # Offline lobby mock
│       ├── RuntimeUITheme.cs      # Procedural UI generator (382 lines)
│       ├── RuntimePauseMenu.cs    # Pause/settings overlay (287 lines)
│       ├── CardSpriteDatabase.cs  # UNO sprite database (194 lines)
│       ├── TienLenCardAssetDatabase.cs
│       ├── TienLenCardSpriteDatabase.cs
│       ├── TienLenThemeAssetDatabase.cs
│       ├── CardUI.cs              # UNO card prefab component
│       └── TienLenCardView.cs     # Tien Len card view component
├── Sprites/Card Asset/            # Backgrounds, Characters, Standard 52, Tables, Uno
├── TextMesh Pro/                  # TMP essentials
└── XR/                            # ARCore loader, Simulation settings
```

**Total custom scripts: ~45 files** | **Total lines of game logic: ~5,000+**

---

## Scene Flow

```
┌─────────────────────┐
│    MainMenuScene     │
│                     │
│  ┌───────────────┐  │
│  │  UNO Offline   │──────> GameScene (GameMode.Uno)
│  ├───────────────┤  │
│  │   Tien Len     │──────> GameScene (GameMode.TienLenMienNam)
│  ├───────────────┤  │
│  │  Multiplayer   │──────> LobbyScene ──> ARMultiplayerGameScene
│  ├───────────────┤  │
│  │    Rules       │  │
│  ├───────────────┤  │
│  │   Settings     │  │
│  ├───────────────┤  │
│  │     Quit       │  │
│  └───────────────┘  │
└─────────────────────┘
```

| Scene | Purpose |
|:---|:---|
| `MainMenuScene` | Game mode selection, settings, rules display |
| `GameScene` | Offline play for both UNO and Tien Len (bootstrapped by `GameSceneBootstrapper`) |
| `LobbyScene` | Multiplayer room creation/joining, ready system, bot configuration |
| `ARMultiplayerGameScene` | Combined AR + multiplayer UNO gameplay |
| `ARVisualTestScene` | Editor-only scene for testing AR card/table visuals |

---

## Getting Started

### Prerequisites

- **Unity 6000.4.6f1** (Unity 6) or later
- **Android Build Support** module installed (with IL2CPP)
- **ARCore-capable Android device** (ARCore SDK 26+)
- **Photon PUN 2** App ID (free at [photonengine.com](https://www.photonengine.com))

### Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   ```

2. **Open in Unity Hub**
   - Add the project folder to Unity Hub
   - Ensure Unity 6000.4.x is selected as the editor version

3. **Configure Photon**
   - Open `Assets/Photon/PhotonUnityNetworking/Resources/PhotonServerSettings`
   - Enter your **Photon PUN App ID** from the Photon Dashboard
   - Set the **App Version** if needed

4. **Configure AR Marker**
   - Prepare a table marker image (e.g., QR code or custom image)
   - Add the reference image to your AR Tracked Image library in `Assets/XR/`
   - Ensure the image name matches `TableMarker` in `ARImageTableTracker`

5. **Open the Main Scene**
   - Navigate to `Assets/Scenes/MainMenuScene.unity`

### Build

1. **Switch Platform**: `File > Build Settings > Android > Switch Platform`
2. **Player Settings** are pre-configured:
   - IL2CPP scripting backend
   - OpenGL ES 3.0 graphics API
   - Minimum API level 26 (Android 8.0)
   - Orientation: Auto Rotation
3. **Build**: `File > Build And Run`

---

## Configuration

| Setting | Location | Default |
|:---|:---|:---|
| Player Count | `GameManager.playerCount` | 4 |
| Starting Cards | `GameManager.startCardCount` | 7 |
| Max Hand Before Loss | `GameManager` (const) | 25 |
| Bot Turn Delay | `GameManager` (const) | 0.85s |
| AR Table Scale | `ARImageTableTracker` | 0.86 |
| AR Camera Distance | `ARImageTableTracker` | 0.82m |
| AR Tilt Angle | `ARImageTableTracker` | 20 degrees |
| Pose Lock Time | `ARImageTableTracker` | 0.2s |
| Camera Settle Time | `ARImageTableTracker` | 0.65s |
| SFX Volume | `RuntimeSfx` | 0.82 |

---

## Controls

### Mobile Touch (AR Mode)

| Action | Gesture |
|:---|:---|
| **Play a card** | Swipe up on a card in your hand |
| **Draw a card** | Swipe on the draw pile |
| **Select color (Wild)** | Tap color button in the overlay panel |
| **Pause** | Tap the pause/settings button |

### Tien Len (Touch)

| Action | Gesture |
|:---|:---|
| **Select card** | Tap a card to toggle selection (selected cards rise 18px) |
| **Play selected** | Tap the Play button |
| **Pass turn** | Tap the Pass button |

---

## How to Play

### UNO

1. Each player starts with **7 cards**
2. Match the top discard card by **color**, **number**, or **symbol**
3. Play **Wild** cards anytime to change the active color
4. **Skip** cards pass the next player; **Reverse** flips direction
5. **Draw Two** forces the next player to draw 2 cards (stackable)
6. **Wild Draw Four** forces the next player to draw 4 (stackable)
7. Press the **UNO** button when you have 2 cards remaining
8. First to empty their hand wins; game continues to rank all players

### Tien Len Mien Nam

1. Each player receives **13 cards**
2. Player with the **3 of Spades** leads the first round
3. Play valid combinations: **Single, Pair, Triple, Straight, Four-of-a-kind, Consecutive Pairs**
4. Each play must beat the previous play with a **higher rank of the same type**
5. **Pass** if you cannot or choose not to play
6. When all other players pass, the last player who played leads the next round
7. **Bomb**: Four-of-a-kind beats 2s; 3+ consecutive pairs chop 2s
8. **Instant Win**: Holding four 2s, all same-color hand, or dragon straight (A-K)
9. First to empty their hand wins; remaining players ranked by card count

---

## Team

**Team 10** — AR Card Arena Development

| Role | Contribution |
|:---|:---|
| Game Logic | UNO engine, Tien Len engine, rule checkers, bot AI |
| AR Systems | Image tracking, table controller, hand interaction, 3D card visuals |
| Multiplayer | Photon PUN 2 integration, room management, state synchronization |
| UI/UX | Runtime theme generation, card views, HUD managers, animations |
| Audio | Procedural SFX system with 12 sound types |

---

<div align="center">

**AR Card Arena** — Where cards come alive on your table.

Built with Unity 6, AR Foundation, and Photon PUN 2.

</div>
