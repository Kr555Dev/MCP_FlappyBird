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
- **Clean Singleton Lifecycle & Domain Reload Resilience**:
  - All scene singletons must implement lazy fallback getters (`if (_instance == null) _instance = FindFirstObjectByType<T>();`) so they instantly recover if Unity performs an AppDomain reload in Play Mode.
  - All scene singletons must implement `OnDestroy()` cleanup (`if (_instance == this) _instance = null;`) so that reloading scenes (`SceneManager.LoadScene()`) or hitting "Play Again" never creates stale or broken state.
  - UI button callbacks must be baked as persistent `UnityEvent` listeners in the scene via `UnityEventTools.AddPersistentListener` and/or re-wired in `OnEnable()`—never rely solely on transient `.AddListener()` delegates inside `Start()`, which Unity permanently purges upon script recompilation.

## Execution Order, Entry Guards & Architectural Invariants Rule
*(Distilled from `unity-architecture`)*
- **Make Startup Order Explicit (Pull Over Push)**:
  - Do not rely on Script Execution Order settings or accidental `Awake` order.
  - Subscribers pull data on demand (`source.GetValue()` or lazy properties) or subscribe to events in `OnEnable()`. Publishers must never push into external systems during their own `Awake()` as it creates hidden order dependencies.
- **Make Update Preconditions Explicit (Guard Clauses)**:
  - Every `Update()`, `LateUpdate()`, and `FixedUpdate()` must open with early-return guard clauses checking readiness before performing work:
    ```csharp
    void Update() {
        if (!_isInitialized) return;       // construction-time readiness
        if (_dataSource == null) return;   // dependency-level readiness
        if (_paused) return;               // gameplay-state readiness
        // real work here
    }
    ```
  - A missing guard is the difference between "doesn't run yet" (safe) and "runs with stale/null data and silently corrupts state" (debug nightmare).

## Skill Integration & Industry Standards Policy
- **Consult Workspace Skills First**: Before designing, refactoring, or writing Unity systems (architecture, UI, physics, shaders, tweens, or performance-critical loops), always consult the corresponding skill guide under `.agents/skills/` (e.g., `unity-architecture`, `unity-ui`, `unity-performance`, `unity-event`, `unity-primetween-design`, etc.) and strictly adhere to its conventions and pitfalls.
