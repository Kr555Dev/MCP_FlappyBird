using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Owns ALL UI elements and updates them in response to FlappyGameManager events.
/// FlappyGameManager never touches the UI directly — this is the clean separation.
/// Attach this to the Canvas or a dedicated HUD GameObject.
/// </summary>
public class HUDController : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector references — assign in Unity Editor
    // -------------------------------------------------------------------------
    [Header("In-Game HUD")]
    public TextMeshProUGUI scoreText;
    public GameObject[]    lifeIcons; // 3 UI GameObjects (shown/hidden per life)

    [Header("Gym Level Flash")]
    public GameObject      gymLevelPanel;
    public TextMeshProUGUI gymLevelText;

    [Header("Main Menu")]
    public GameObject      mainMenuPanel;
    public Button          startButton;

    [Header("Game Over")]
    public GameObject      gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI bestScoreText;
    public Button          restartButton;

    // =========================================================================
    // Event wiring — subscribe in OnEnable, unsubscribe in OnDisable (best practice)
    // =========================================================================
    void OnEnable()
    {
        FlappyGameManager.OnScoreChanged     += UpdateScore;
        FlappyGameManager.OnLivesChanged     += UpdateLives;
        FlappyGameManager.OnGymLevelUp       += ShowGymFlash;
        FlappyGameManager.OnGameStateChanged += OnGameStateChanged;
    }

    void OnDisable()
    {
        FlappyGameManager.OnScoreChanged     -= UpdateScore;
        FlappyGameManager.OnLivesChanged     -= UpdateLives;
        FlappyGameManager.OnGymLevelUp       -= ShowGymFlash;
        FlappyGameManager.OnGameStateChanged -= OnGameStateChanged;
    }

    // =========================================================================
    void Start()
    {
        // Wire buttons
        if (startButton   != null) startButton.onClick.AddListener(  () => FlappyGameManager.instance?.StartGame());
        if (restartButton != null) restartButton.onClick.AddListener(() => FlappyGameManager.instance?.RestartGame());

        // Initial UI state
        SetPanelActive(gymLevelPanel,  false);
        SetPanelActive(gameOverPanel,  false);
        SetPanelActive(mainMenuPanel,  true);

        if (scoreText != null) scoreText.text = "0";

        // Init life icons
        int startLives = FlappyGameManager.instance?.config?.startingLives ?? 3;
        UpdateLives(startLives);
    }

    // =========================================================================
    // Event Handlers
    // =========================================================================
    void UpdateScore(int score)
    {
        if (scoreText != null) scoreText.text = score.ToString();
    }

    void UpdateLives(int lives)
    {
        if (lifeIcons == null) return;
        for (int i = 0; i < lifeIcons.Length; i++)
        {
            if (lifeIcons[i] != null)
                lifeIcons[i].SetActive(i < lives);
        }
    }

    void ShowGymFlash(int gymLevel)
    {
        if (gymLevelPanel == null || gymLevelText == null) return;
        gymLevelText.text = $"GYM  {gymLevel}!";
        StopCoroutine(nameof(GymFlashRoutine)); // prevent stacking
        StartCoroutine(nameof(GymFlashRoutine));
    }

    IEnumerator GymFlashRoutine()
    {
        gymLevelPanel.SetActive(true);
        // Use WaitForSecondsRealtime so it works even if timeScale is briefly 0
        yield return new WaitForSecondsRealtime(2.0f);
        gymLevelPanel.SetActive(false);
    }

    void OnGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:
                SetPanelActive(mainMenuPanel, true);
                SetPanelActive(gameOverPanel, false);
                if (scoreText != null) scoreText.gameObject.SetActive(false);
                break;

            case GameState.Playing:
                SetPanelActive(mainMenuPanel, false);
                SetPanelActive(gameOverPanel, false);
                if (scoreText != null) scoreText.gameObject.SetActive(true);
                break;

            case GameState.GameOver:
                SetPanelActive(gameOverPanel, true);
                if (scoreText != null) scoreText.gameObject.SetActive(false);
                if (FlappyGameManager.instance != null)
                {
                    if (finalScoreText != null)
                        finalScoreText.text = $"Score: {FlappyGameManager.instance.Score}";
                    if (bestScoreText != null)
                        bestScoreText.text  = $"Best:  {FlappyGameManager.instance.BestScore}";
                }
                break;
        }
    }

    // =========================================================================
    // Helpers
    // =========================================================================
    static void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null) panel.SetActive(active);
    }
}
