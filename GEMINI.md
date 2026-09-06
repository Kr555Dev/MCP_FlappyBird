# Project Rules & Memory: BYOG_JAM (Flappy Bird Game)

## Scope & Git Tracking Policy
- **Primary Game Focus**: This project tracks **only** the Flappy Bird scene game and its dependencies in Git.
- **Excluded Content**: All other games and scenes (such as the legacy Pokemon quiz scenes `L01`–`L11`, `StartMenu`, `MCP`), legacy modules (`Assets/Editor/`, `Assets/OptimizedArchitecture/`), unused scripts (`buttonManager.cs`, `Audiomanager.cs`, `Sound.cs`), and unused audio/sprites must remain ignored in `.gitignore`.

## Dynamic .gitignore Maintenance Rule
- Whenever any new asset (sprite, audio clip, prefab, scriptable object, shader, script, etc.) is created, added, or newly referenced from existing folders for the **Flappy scene game**:
  1. Inspect the new file or reference.
  2. **Immediately update `.gitignore`** to explicitly allow/un-ignore the file and its corresponding `.meta` file (using the pattern `!Assets/...` and `!Assets/...meta`).
  3. Ensure no unrelated non-Flappy files are unintentionally un-ignored.
