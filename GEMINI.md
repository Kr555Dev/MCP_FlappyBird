# Project Rules & Memory: BYOG_JAM (Flappy Bird Game)

## Scope & Git Tracking Policy
- **Primary Game Focus**: This project tracks **only** the Flappy Bird scene game and its dependencies in Git.
- **Excluded Content**: All other games and scenes (such as the legacy Pokemon quiz scenes `L01`–`L11`, `StartMenu`, `MCP`), legacy modules (`Assets/Editor/`, `Assets/OptimizedArchitecture/`), unused scripts (`buttonManager.cs`, `Audiomanager.cs`, `Sound.cs`), and unused audio/sprites must remain ignored in `.gitignore`.

## Dynamic .gitignore Maintenance Rule
- Whenever any new asset (sprite, audio clip, prefab, scriptable object, shader, script, etc.) is created, added, or newly referenced from existing folders for the **Flappy scene game**:
  1. Inspect the new file or reference.
  2. **Immediately update `.gitignore`** to explicitly allow/un-ignore the file and its corresponding `.meta` file (using the pattern `!Assets/...` and `!Assets/...meta`).
  3. Ensure no unrelated non-Flappy files are unintentionally un-ignored.

## Unity Scene Architecture & In-Editor Persistence Rule
- **Scene-First / In-Editor Visibility First**:
  - UI elements (Canvases, Panels, Menus, Buttons, HUDs), core scene managers, and component attachments must **always be permanently placed, styled, and saved directly inside the scene asset (or prefabs)** so they are visible and editable in the Unity Editor before entering Play mode.
  - **Do not use runtime-only injection** (e.g. `[RuntimeInitializeOnLoadMethod]`, `new GameObject()` in `Awake()`) for core scene hierarchy, UI, or persistent scene managers. Procedural instantiation at runtime is strictly reserved for dynamic gameplay entities (e.g. spawned obstacles, projectile/pipe pools, floating particle bursts).
- **Explicit Inspector Wiring**:
  - Wire serialized component references in the scene/Inspector rather than relying on runtime `Find()` or dynamic component attachment.
- **Clean Singleton Lifecycle**:
  - All scene singletons must implement `OnDestroy()` cleanup (`if (instance == this) instance = null;`) so that reloading scenes (`SceneManager.LoadScene()`) or hitting "Play Again" never creates stale or broken state.

## Skill Integration & Industry Standards Policy
- **Consult Workspace Skills First**: Before designing, refactoring, or writing Unity systems (architecture, UI, physics, shaders, tweens, or performance-critical loops), always consult the corresponding skill guide under `.agents/skills/` (e.g., `unity-architecture`, `unity-ui`, `unity-performance`, `unity-primetween-design`, etc.) and strictly adhere to its conventions and pitfalls.
