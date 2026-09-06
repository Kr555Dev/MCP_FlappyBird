using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton: Single source of truth for all game state.
/// Communicates via static C# events — zero direct UI references.
/// UI listens via HUDController; Gameplay listens via PipeSpawner etc.
/// </summary>
public class FlappyGameManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Singleton
    // -------------------------------------------------------------------------
    public static FlappyGameManager instance { get; private set; }

    // -------------------------------------------------------------------------
    // Config
    // -------------------------------------------------------------------------
    [Header("Configuration (assign DifficultyConfig asset)")]
    public DifficultyConfig config;

    // -------------------------------------------------------------------------
    // Events — subscribe with += in OnEnable, unsubscribe in OnDisable
    // -------------------------------------------------------------------------
    public static event Action<int>       OnScoreChanged;
    public static event Action<int>       OnLivesChanged;
    public static event Action<int>       OnGymLevelUp;       // passes new gym level number
    public static event Action<GameState> OnGameStateChanged;
    public static event Action<int, int>  OnCoinsChanged;     // passes (runCoins, totalCoins)

    // -------------------------------------------------------------------------
    // Public read-only state (safe to read from any script)
    // -------------------------------------------------------------------------
    public GameState CurrentState        { get; private set; } = GameState.MainMenu;
    public float     CurrentSpeed        { get; private set; }
    public float     CurrentSpawnRate    { get; private set; }
    public float     CurrentGapHalfHeight{ get; private set; }
    public int       GymLevel            { get; private set; } = 1;
    public int       Score               { get; private set; }
    public int       Lives               { get; private set; }
    public int       RunCoins            { get; private set; }
    public int       TotalCoins          => PlayerPrefs.GetInt("TotalCoins", 0);
    public int       BestScore           => PlayerPrefs.GetInt("BestScore", 0);

    // -------------------------------------------------------------------------
    // Private
    // -------------------------------------------------------------------------
    private int pipesPassedThisGym;

    // =========================================================================
    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;

        Application.runInBackground = true;

        if (config != null)
        {
            ResetDifficulty();
            Lives = config.startingLives;
        }
    }

    void Start()
    {
        if (config == null)
        {
            Debug.LogError("[GameManager] DifficultyConfig is not assigned! Please assign it in the Inspector.");
            return;
        }

        TransitionTo(GameState.MainMenu);
    }

    // =========================================================================
    // State Machine
    // =========================================================================
    void TransitionTo(GameState next)
    {
        CurrentState   = next;
        Time.timeScale = (next == GameState.Playing) ? 1f : 0f;
        OnGameStateChanged?.Invoke(next);
    }

    public void StartGame()
    {
        if (CurrentState != GameState.MainMenu) return;
        TransitionTo(GameState.Playing);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        string activeScene = SceneManager.GetActiveScene().name;
        if (!string.IsNullOrEmpty(activeScene))
            SceneManager.LoadScene(activeScene);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // =========================================================================
    // Score & Pipe Passing
    // =========================================================================
    /// <summary>Called by BirdController's ScoreZone trigger.</summary>
    public void PipePassed()
    {
        if (CurrentState != GameState.Playing) return;

        Score++;
        OnScoreChanged?.Invoke(Score);

        pipesPassedThisGym++;
        if (pipesPassedThisGym >= config.pipesPerGym)
        {
            pipesPassedThisGym = 0;
            GymLevel++;
            ApplyDifficultyStep();
        }
    }

    /// <summary>Called when player collects a coin.</summary>
    public void AddBonusScore(int points)
    {
        if (CurrentState != GameState.Playing) return;
        Score += points;
        OnScoreChanged?.Invoke(Score);
    }

    /// <summary>Called when player collects a coin currency pickup.</summary>
    public void AddCoin(int amount = 1)
    {
        RunCoins += amount;
        int newTotal = TotalCoins + amount;
        PlayerPrefs.SetInt("TotalCoins", newTotal);
        PlayerPrefs.Save();
        OnCoinsChanged?.Invoke(RunCoins, newTotal);
    }

    /// <summary>Called when purchasing cosmetics from the Shop.</summary>
    public bool SpendCoins(int amount)
    {
        if (TotalCoins >= amount)
        {
            int newTotal = TotalCoins - amount;
            PlayerPrefs.SetInt("TotalCoins", newTotal);
            PlayerPrefs.Save();
            OnCoinsChanged?.Invoke(RunCoins, newTotal);
            return true;
        }
        return false;
    }

    // =========================================================================
    // Lives & Death
    // =========================================================================
    /// <summary>
    /// Called by BirdController on obstacle hit.
    /// Decrements a life. If 0 lives remain → GameOver.
    /// BirdController handles the invincibility flash itself.
    /// </summary>
    public void BirdHitObstacle()
    {
        if (CurrentState != GameState.Playing) return;

        // Trigger Hit Stop effect for AAA game feel
        StartCoroutine(HitStopRoutine());

        Lives--;
        OnLivesChanged?.Invoke(Lives);

        if (Lives <= 0)
            TriggerGameOver();
    }

    private System.Collections.IEnumerator HitStopRoutine()
    {
        // Freeze time completely
        Time.timeScale = 0f;
        // Wait for a fraction of a second in real time
        yield return new WaitForSecondsRealtime(0.08f);
        
        // Restore time scale if we are still playing (not game over)
        if (CurrentState == GameState.Playing)
        {
            Time.timeScale = 1f;
        }
    }

    void TriggerGameOver()
    {
        if (Score > BestScore)
        {
            PlayerPrefs.SetInt("BestScore", Score);
            PlayerPrefs.Save();
        }
        TransitionTo(GameState.GameOver);
    }

    // =========================================================================
    // Difficulty
    // =========================================================================
    void ResetDifficulty()
    {
        CurrentSpeed          = config.baseSpeed;
        CurrentSpawnRate      = config.baseSpawnRate;
        CurrentGapHalfHeight  = config.baseGapHalfHeight;
        pipesPassedThisGym    = 0;
        GymLevel              = 1;
        RunCoins              = 0;
        OnCoinsChanged?.Invoke(RunCoins, TotalCoins);
    }

    void ApplyDifficultyStep()
    {
        CurrentSpeed          = Mathf.Min(config.maxSpeed,     CurrentSpeed          + config.speedIncrement);
        CurrentSpawnRate      = Mathf.Max(config.minSpawnRate,  CurrentSpawnRate      - config.spawnRateDecrement);
        CurrentGapHalfHeight  = Mathf.Max(config.minGapHalf,    CurrentGapHalfHeight  - config.gapHalfDecrement);

        OnGymLevelUp?.Invoke(GymLevel);

        Debug.Log($"[GameManager] ★ Gym {GymLevel} reached! " +
                  $"Speed={CurrentSpeed:F1} | SpawnRate={CurrentSpawnRate:F2}s | GapHalf={CurrentGapHalfHeight:F1}");
    }
}
