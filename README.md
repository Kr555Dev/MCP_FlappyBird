# MCP Flappy Bird (Pokemon Edition)

An arcade 2D Flappy Bird game built in **Unity 6** with an event-driven architecture, ScriptableObject-based tuning, procedural visual polish, and responsive game juice.

---

## Features

### Core Gameplay & Physics
- **Juicy Flight Mechanics**: Smooth physics tilt, velocity-driven squash and stretch, ceiling clamping, and pit recovery.
- **Lives & Mercy Invincibility**: 3-life system featuring an invulnerability coroutine with rapid sprite flashing on obstacle collision.
- **Hit-Stop Impact Feel**: An 80ms realtime frame freeze (`Time.timeScale = 0`) on impact paired with heavy camera trauma shake.
- **Collectible Coins**: Pikachu bonus coins floating between pipe gaps that grant bonus points, light camera shake, and collection audio.
- **Pipes & Object Pooling**: Pre-warmed obstacle pool (`PipeSpawner`) to prevent runtime instantiation and GC spikes.

### Progressive Difficulty (Gym System)
- **ScriptableObject Tuning (`DifficultyConfig`)**: Modify speed, spawn intervals, gap clearances, starting lives, and coin rates without touching code.
- **Gym Level Progression**: Passing sets of pipes advances the player to the next Gym level, incrementally ramping pipe speed, tightening gap heights, and accelerating spawn frequency.

### Procedural Visual Polish & Juice
- **Dynamic Time-of-Day Parallax**:
  - Sky gradient dynamically morphs from **Day** (bright blue) to **Sunset** (warm amber/magenta) to **Night** (deep navy with twinkling star particles) as Gym levels increase.
  - Multi-layered procedural clouds scrolling at variable parallax speeds.
  - Procedurally generated mountain range with Perlin-noise silhouettes and snowy peaks.
- **Particle Systems**: Procedural burst effects for flapping puffs, golden coin sparkles, and crash impacts.
- **Dual Screen Shake**: Dedicated trauma shake for hits (`0.4s, 0.5 mag`) and subtle micro-shakes for coin pickups (`0.1s, 0.15 mag`), computed using `unscaledDeltaTime` so shake persists during hit-stop.

---

## Architecture Overview

The project is structured with decoupled, single-responsibility modules communicating via static C# events:

```
                      +-------------------+
                      | DifficultyConfig  | (ScriptableObject)
                      +---------+---------+
                                |
                                v
                     +---------------------+
                     |  FlappyGameManager  | (State Machine & Singleton)
                     +----------+----------+
                                |
        +-----------------------+-----------------------+
        | Static C# Events:                             |
        | - OnScoreChanged                              |
        | - OnLivesChanged                              |
        | - OnGymLevelUp                                |
        | - OnGameStateChanged                          |
        v                                               v
+---------------+                               +----------------+
| HUDController |                               |  PipeSpawner   |
| (UI & Menus)  |                               | (Object Pool)  |
+---------------+                               +-------+--------+
                                                        |
                                                        v
+------------------+     Static Actions         +----------------+
|  BirdController  | -------------------------> |  PipeMover /   |
| (Player Physics) |   OnBirdFlap, OnBirdHit    |   CoinPickup   |
+--------+---------+                            +----------------+
         |
         +----------------------------+
         v                            v
+-----------------------+    +------------------+
|  CameraController     |    | VisualEffects    |
| (Trauma Screen Shake) |    | (Parallax & VFX) |
+-----------------------+    +------------------+
```

### Key Scripts (`Assets/Scripts/FlappyBird/`)
- [FlappyGameManager.cs](file:///f:/Unity%20Projects/2D%20projects/BYOG_JAM/Assets/Scripts/FlappyBird/FlappyGameManager.cs): Central game state machine (`MainMenu`, `Playing`, `GameOver`), lives management, gym progression, and hit-stop coroutines.
- [DifficultyConfig.cs](file:///f:/Unity%20Projects/2D%20projects/BYOG_JAM/Assets/Scripts/FlappyBird/DifficultyConfig.cs): ScriptableObject configuration defining gameplay balance parameters.
- [BirdController.cs](file:///f:/Unity%20Projects/2D%20projects/BYOG_JAM/Assets/Scripts/FlappyBird/BirdController.cs): Player physics, flap velocities, squash & stretch, collision detection, and invincibility flashing.
- [PipeSpawner.cs](file:///f:/Unity%20Projects/2D%20projects/BYOG_JAM/Assets/Scripts/FlappyBird/PipeSpawner.cs): Object pool manager for pipes and randomized coin placement.
- [PipeMover.cs](file:///f:/Unity%20Projects/2D%20projects/BYOG_JAM/Assets/Scripts/FlappyBird/PipeMover.cs): Moves pipe obstacles across the screen and recycles them back to the pool.
- [CoinPickup.cs](file:///f:/Unity%20Projects/2D%20projects/BYOG_JAM/Assets/Scripts/FlappyBird/CoinPickup.cs): Collectible trigger awarding bonus score on contact.
- [HUDController.cs](file:///f:/Unity%20Projects/2D%20projects/BYOG_JAM/Assets/Scripts/FlappyBird/HUDController.cs): UI listener driving score counters, heart icons, gym level banners, and game over screens.
- [CameraController.cs](file:///f:/Unity%20Projects/2D%20projects/BYOG_JAM/Assets/Scripts/FlappyBird/CameraController.cs): Frame-rate independent trauma camera shake for impact and pickup feedback.
- [VisualEffectsManager.cs](file:///f:/Unity%20Projects/2D%20projects/BYOG_JAM/Assets/Scripts/FlappyBird/VisualEffectsManager.cs): Procedural sky gradients, parallax clouds, Perlin mountains, and particle emissions.
- [GameState.cs](file:///f:/Unity%20Projects/2D%20projects/BYOG_JAM/Assets/Scripts/FlappyBird/GameState.cs): Enum defining `MainMenu`, `Playing`, and `GameOver` states.

---

## Controls

| Action | Input |
|---|---|
| **Flap / Jump** | `Spacebar` or `Left Mouse Click` / `Tap` |
| **Start Game** | `Start Game` button on Main Menu |
| **Restart Game** | `Play Again` button on Game Over screen |

---

## Project Structure

```
BYOG_JAM/
├── Assets/
│   ├── Config/              # DifficultyConfig.asset (ScriptableObject)
│   ├── Prefabs/             # CoinPickup.prefab, PipePrefab.prefab
│   ├── Scenes/
│   │   └── Flappy.unity     # Main game scene
│   ├── Scripts/
│   │   └── FlappyBird/      # Core game C# scripts
│   ├── sounds/              # Flappy audio clips (flap, hit, coin, bgm)
│   ├── sprites/             # Flappy 2D sprites (Pidgeotto, Diglett, Pikachu)
│   └── TextMesh Pro/        # UI fonts, materials, shaders, and settings
├── Packages/                # Unity package manifest & lock files
├── ProjectSettings/         # Project tags, physics, layers, and build settings
├── .gitignore               # Strict filter for Flappy assets & dependencies
├── AGENTS.md                # Project guidelines & memory for AI agents
├── GEMINI.md                # Project guidelines & memory for AI agents
└── README.md                # Project documentation
```

---

## Getting Started

### Prerequisites
- **Unity 6** (recommended: `6000.3.19f1` or newer)
- **Universal RP / Built-in 2D Pipeline** (standard 2D packages configured via `Packages/manifest.json`)

### How to Run
1. Clone the repository:
   ```bash
   git clone https://github.com/Kr555Dev/MCP_FlappyBird.git
   ```
2. Open **Unity Hub** and click **Add** -> **Add project from disk**.
3. Select the repository root folder.
4. In the Unity Project window, navigate to `Assets/Scenes/` and double-click **`Flappy.unity`**.
5. Press the **Play** button in the Unity Editor to play.
