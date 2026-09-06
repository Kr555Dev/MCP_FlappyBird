using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Controls ALL UI screens and interactions:
/// - In-game HUD (Score, Hearts/Lives, Coin Counter with juice animations)
/// - Main Menu (Game Title, High Score, Total Coin savings, Start Button, Shop Button)
/// - Game Over Card (Sleek dark card panel, Final Score, Trophy Best Score, Coins Earned, Play Again, Shop Button)
/// - Shop Menu Integration (Access to Bird Customization Wardrobe)
/// </summary>
public class HUDController : MonoBehaviour
{
    public static HUDController instance { get; private set; }

    [Header("In-Game HUD")]
    public TextMeshProUGUI scoreText;
    public GameObject[]    lifeIcons;
    public GameObject      coinCounterHUD;
    public TextMeshProUGUI runCoinText;

    [Header("Gym Level Flash")]
    public GameObject      gymLevelPanel;
    public TextMeshProUGUI gymLevelText;

    [Header("Main Menu")]
    public GameObject      mainMenuPanel;
    public Button          startButton;
    public Button          menuShopButton;
    public TextMeshProUGUI menuBestScoreText;
    public TextMeshProUGUI menuTotalCoinsText;

    [Header("Game Over")]
    public GameObject      gameOverPanel;
    public TextMeshProUGUI gameOverTitleText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI bestScoreText;
    public TextMeshProUGUI coinsEarnedText;
    public Button          restartButton;
    public Button          gameOverShopButton;

    [Header("Shop Integration")]
    public ShopUIController shopController;

    [Header("UI Sprites")]
    public Sprite cardBackgroundSprite;
    public Sprite pillButtonSprite;
    public Sprite coinSprite;
    public Sprite heartSprite;

    private Coroutine coinPulseRoutine;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;

        AutoLoadSprites();
        EnsureVisualPolish();
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    void AutoLoadSprites()
    {
        if (cardBackgroundSprite == null) 
            cardBackgroundSprite = Resources.Load<Sprite>("ui_card_bg") ?? Resources.Load<Sprite>("Cosmetics/ui_card_bg");
        if (pillButtonSprite == null) 
            pillButtonSprite = Resources.Load<Sprite>("ui_button_pill") ?? Resources.Load<Sprite>("Cosmetics/ui_button_pill");

        if (coinSprite == null)
            coinSprite = Resources.Load<Sprite>("UI/coin") ?? Resources.Load<Sprite>("coin");

        if (heartSprite == null)
            heartSprite = Resources.Load<Sprite>("UI/heart") ?? Resources.Load<Sprite>("heart");

#if UNITY_EDITOR
        if (cardBackgroundSprite == null)
            cardBackgroundSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Cosmetics/ui_card_bg.png");
        if (pillButtonSprite == null)
            pillButtonSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Cosmetics/ui_button_pill.png");
        if (coinSprite == null)
            coinSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/coin.png") ?? UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/coin.png");
        if (heartSprite == null)
            heartSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/heart.png") ?? UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/heart.png");
#endif
    }

    void OnEnable()
    {
        FlappyGameManager.OnScoreChanged     += UpdateScore;
        FlappyGameManager.OnLivesChanged     += UpdateLives;
        FlappyGameManager.OnCoinsChanged     += UpdateCoins;
        FlappyGameManager.OnGymLevelUp       += ShowGymFlash;
        FlappyGameManager.OnGameStateChanged += OnGameStateChanged;
    }

    void OnDisable()
    {
        FlappyGameManager.OnScoreChanged     -= UpdateScore;
        FlappyGameManager.OnLivesChanged     -= UpdateLives;
        FlappyGameManager.OnCoinsChanged     -= UpdateCoins;
        FlappyGameManager.OnGymLevelUp       -= ShowGymFlash;
        FlappyGameManager.OnGameStateChanged -= OnGameStateChanged;
    }

    void Start()
    {
        // Wire buttons
        if (startButton != null)
            startButton.onClick.AddListener(() => FlappyGameManager.instance?.StartGame());

        if (restartButton != null)
            restartButton.onClick.AddListener(() => FlappyGameManager.instance?.RestartGame());

        if (menuShopButton != null)
            menuShopButton.onClick.AddListener(OpenShop);

        if (gameOverShopButton != null)
            gameOverShopButton.onClick.AddListener(OpenShop);

        // Initial UI state
        SetPanelActive(gymLevelPanel, false);
        SetPanelActive(gameOverPanel, false);
        ShowMainMenu();

        if (scoreText != null) scoreText.text = "0";

        int startLives = FlappyGameManager.instance?.config?.startingLives ?? 3;
        UpdateLives(startLives);
        UpdateCoins(0, FlappyGameManager.instance != null ? FlappyGameManager.instance.TotalCoins : 0);
    }

    public void ShowMainMenu()
    {
        SetPanelActive(mainMenuPanel, true);
        SetPanelActive(gameOverPanel, false);
        if (coinCounterHUD != null) coinCounterHUD.SetActive(false);
        if (scoreText != null) scoreText.gameObject.SetActive(false);

        if (menuBestScoreText != null && FlappyGameManager.instance != null)
            menuBestScoreText.text = $"BEST: {FlappyGameManager.instance.BestScore}";

        if (menuTotalCoinsText != null && FlappyGameManager.instance != null)
            menuTotalCoinsText.text = $"COINS: {FlappyGameManager.instance.TotalCoins}";
    }

    public void OpenShop()
    {
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(gameOverPanel, false);

        if (shopController != null)
        {
            shopController.OpenShop();
        }
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
            {
                lifeIcons[i].SetActive(i < lives);
                Image img = lifeIcons[i].GetComponent<Image>();
                if (img != null && heartSprite != null)
                {
                    img.sprite = heartSprite;
                    img.color = Color.white;
                }
            }
        }
    }

    void UpdateCoins(int runCoins, int totalCoins)
    {
        if (runCoinText != null)
        {
            runCoinText.text = runCoins.ToString();

            // Trigger juice punch scale on pickup
            if (coinCounterHUD != null && coinCounterHUD.activeInHierarchy)
            {
                if (coinPulseRoutine != null) StopCoroutine(coinPulseRoutine);
                coinPulseRoutine = StartCoroutine(CoinCounterPunchRoutine());
            }
        }

        if (menuTotalCoinsText != null)
            menuTotalCoinsText.text = $"COINS: {totalCoins}";
    }

    IEnumerator CoinCounterPunchRoutine()
    {
        Transform t = coinCounterHUD.transform;
        Vector3 orig = Vector3.one;
        Vector3 target = new Vector3(1.3f, 1.3f, 1f);

        float elapsed = 0f;
        while (elapsed < 0.1f)
        {
            elapsed += Time.unscaledDeltaTime;
            t.localScale = Vector3.Lerp(orig, target, elapsed / 0.1f);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < 0.15f)
        {
            elapsed += Time.unscaledDeltaTime;
            t.localScale = Vector3.Lerp(target, orig, elapsed / 0.15f);
            yield return null;
        }

        t.localScale = orig;
    }

    void ShowGymFlash(int gymLevel)
    {
        if (gymLevelPanel == null || gymLevelText == null) return;
        gymLevelText.text = $"GYM  {gymLevel}!";
        StopCoroutine(nameof(GymFlashRoutine));
        StartCoroutine(nameof(GymFlashRoutine));
    }

    IEnumerator GymFlashRoutine()
    {
        gymLevelPanel.SetActive(true);
        yield return new WaitForSecondsRealtime(2.0f);
        gymLevelPanel.SetActive(false);
    }

    void OnGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:
                ShowMainMenu();
                break;

            case GameState.Playing:
                SetPanelActive(mainMenuPanel, false);
                SetPanelActive(gameOverPanel, false);
                if (scoreText != null) scoreText.gameObject.SetActive(true);
                if (coinCounterHUD != null) coinCounterHUD.SetActive(true);
                break;

            case GameState.GameOver:
                SetPanelActive(mainMenuPanel, false);
                SetPanelActive(gameOverPanel, true);
                if (scoreText != null) scoreText.gameObject.SetActive(false);
                if (coinCounterHUD != null) coinCounterHUD.SetActive(false);

                if (FlappyGameManager.instance != null)
                {
                    if (finalScoreText != null)
                        finalScoreText.text = $"SCORE: {FlappyGameManager.instance.Score}";

                    if (bestScoreText != null)
                        bestScoreText.text = $"BEST: {FlappyGameManager.instance.BestScore}";

                    if (coinsEarnedText != null)
                        coinsEarnedText.text = $"COINS: +{FlappyGameManager.instance.RunCoins}";
                }
                break;
        }
    }

    // =========================================================================
    // Dynamic Layout & Polish Engine (Guarantees zero overlap & gorgeous cards)
    // =========================================================================
    void EnsureVisualPolish()
    {
        // 1. Polish GameOverPanel layout
        if (gameOverPanel != null)
        {
            RectTransform panelRT = gameOverPanel.GetComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.5f, 0.5f);
            panelRT.anchorMax = new Vector2(0.5f, 0.5f);
            panelRT.pivot     = new Vector2(0.5f, 0.5f);
            panelRT.sizeDelta = new Vector2(460f, 440f);
            panelRT.anchoredPosition = Vector2.zero;

            Image panelBg = gameOverPanel.GetComponent<Image>();
            if (panelBg == null) panelBg = gameOverPanel.AddComponent<Image>();
            panelBg.color = new Color(0.08f, 0.11f, 0.18f, 0.95f); // Deep frosted midnight slate
            if (cardBackgroundSprite != null)
            {
                panelBg.sprite = cardBackgroundSprite;
                panelBg.type   = Image.Type.Sliced;
            }

            // Ensure "GAME OVER" title
            if (gameOverTitleText == null)
            {
                Transform titleT = gameOverPanel.transform.Find("GameOverTitle");
                if (titleT != null) gameOverTitleText = titleT.GetComponent<TextMeshProUGUI>();
                else
                {
                    GameObject titleGO = new GameObject("GameOverTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
                    titleGO.transform.SetParent(gameOverPanel.transform, false);
                    gameOverTitleText = titleGO.GetComponent<TextMeshProUGUI>();
                    gameOverTitleText.text = "GAME OVER";
                    gameOverTitleText.fontSize = 44;
                    gameOverTitleText.fontStyle = FontStyles.Bold;
                    gameOverTitleText.color = new Color(1f, 0.25f, 0.25f, 1f); // Vibrant crimson
                    gameOverTitleText.alignment = TextAlignmentOptions.Center;
                }
            }
            RectTransform titleRT = gameOverTitleText.GetComponent<RectTransform>();
            titleRT.anchoredPosition = new Vector2(0f, 160f);
            titleRT.sizeDelta = new Vector2(400f, 60f);

            // FinalScoreText: Centered, y: +95
            if (finalScoreText != null)
            {
                RectTransform fsRT = finalScoreText.GetComponent<RectTransform>();
                fsRT.anchoredPosition = new Vector2(0f, 95f);
                fsRT.sizeDelta = new Vector2(400f, 45f);
                finalScoreText.fontSize = 32;
                finalScoreText.fontStyle = FontStyles.Bold;
                finalScoreText.color = Color.white;
                finalScoreText.alignment = TextAlignmentOptions.Center;
            }

            // BestScoreText: Centered, y: +45
            if (bestScoreText != null)
            {
                RectTransform bsRT = bestScoreText.GetComponent<RectTransform>();
                bsRT.anchoredPosition = new Vector2(0f, 45f);
                bsRT.sizeDelta = new Vector2(400f, 40f);
                bestScoreText.fontSize = 26;
                bestScoreText.fontStyle = FontStyles.Bold;
                bestScoreText.color = new Color(1f, 0.85f, 0.1f, 1f); // Golden yellow
                bestScoreText.alignment = TextAlignmentOptions.Center;
            }

            // CoinsEarnedText: Centered, y: -5
            if (coinsEarnedText == null)
            {
                Transform ceT = gameOverPanel.transform.Find("CoinsEarnedText");
                if (ceT != null) coinsEarnedText = ceT.GetComponent<TextMeshProUGUI>();
                else
                {
                    GameObject ceGO = new GameObject("CoinsEarnedText", typeof(RectTransform), typeof(TextMeshProUGUI));
                    ceGO.transform.SetParent(gameOverPanel.transform, false);
                    coinsEarnedText = ceGO.GetComponent<TextMeshProUGUI>();
                    coinsEarnedText.text = "COINS: +0";
                    coinsEarnedText.fontSize = 24;
                    coinsEarnedText.fontStyle = FontStyles.Bold;
                    coinsEarnedText.color = new Color(0.3f, 0.85f, 1f, 1f); // Bright cyan
                    coinsEarnedText.alignment = TextAlignmentOptions.Center;
                }
            }
            RectTransform ceRT = coinsEarnedText.GetComponent<RectTransform>();
            ceRT.anchoredPosition = new Vector2(0f, -5f);
            ceRT.sizeDelta = new Vector2(400f, 35f);

            // RestartButton: Centered, y: -75
            if (restartButton != null)
            {
                RectTransform rRT = restartButton.GetComponent<RectTransform>();
                rRT.anchoredPosition = new Vector2(0f, -75f);
                rRT.sizeDelta = new Vector2(280f, 54f);

                Image rImg = restartButton.GetComponent<Image>();
                if (rImg != null)
                {
                    rImg.color = new Color(0.18f, 0.72f, 0.35f, 1f); // Emerald green
                    if (pillButtonSprite != null)
                    {
                        rImg.sprite = pillButtonSprite;
                        rImg.type   = Image.Type.Sliced;
                    }
                }

                TextMeshProUGUI rText = restartButton.GetComponentInChildren<TextMeshProUGUI>();
                if (rText != null)
                {
                    rText.text = "PLAY AGAIN";
                    rText.fontSize = 24;
                    rText.fontStyle = FontStyles.Bold;
                    rText.color = Color.white;
                }
            }

            // GameOverShopButton: Centered, y: -140
            if (gameOverShopButton == null)
            {
                Transform sT = gameOverPanel.transform.Find("GameOverShopButton");
                if (sT != null) gameOverShopButton = sT.GetComponent<Button>();
                else
                {
                    GameObject sGO = new GameObject("GameOverShopButton", typeof(RectTransform), typeof(Image), typeof(Button));
                    sGO.transform.SetParent(gameOverPanel.transform, false);
                    gameOverShopButton = sGO.GetComponent<Button>();
                    Image sImg = sGO.GetComponent<Image>();
                    sImg.color = new Color(0.2f, 0.45f, 0.85f, 1f); // Blue
                    if (pillButtonSprite != null)
                    {
                        sImg.sprite = pillButtonSprite;
                        sImg.type   = Image.Type.Sliced;
                    }

                    GameObject sTextGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                    sTextGO.transform.SetParent(sGO.transform, false);
                    RectTransform sTextRT = sTextGO.GetComponent<RectTransform>();
                    sTextRT.anchorMin = Vector2.zero;
                    sTextRT.anchorMax = Vector2.one;
                    sTextRT.sizeDelta = Vector2.zero;

                    TextMeshProUGUI sText = sTextGO.GetComponent<TextMeshProUGUI>();
                    sText.text = "BIRD SHOP";
                    sText.fontSize = 20;
                    sText.fontStyle = FontStyles.Bold;
                    sText.color = Color.white;
                    sText.alignment = TextAlignmentOptions.Center;
                }
            }
            RectTransform gosbRT = gameOverShopButton.GetComponent<RectTransform>();
            gosbRT.anchoredPosition = new Vector2(0f, -140f);
            gosbRT.sizeDelta = new Vector2(280f, 48f);
        }

        // 2. In-Game Coin Counter HUD
        if (coinCounterHUD == null)
        {
            Transform existing = transform.Find("CoinCounterHUD");
            if (existing != null)
            {
                coinCounterHUD = existing.gameObject;
                runCoinText = coinCounterHUD.GetComponentInChildren<TextMeshProUGUI>();
            }
            else
            {
                coinCounterHUD = new GameObject("CoinCounterHUD", typeof(RectTransform), typeof(Image));
                coinCounterHUD.transform.SetParent(transform, false);

                RectTransform ccRT = coinCounterHUD.GetComponent<RectTransform>();
                ccRT.anchorMin = new Vector2(1f, 1f);
                ccRT.anchorMax = new Vector2(1f, 1f);
                ccRT.pivot     = new Vector2(1f, 1f);
                ccRT.anchoredPosition = new Vector2(-20f, -20f);
                ccRT.sizeDelta = new Vector2(110f, 44f);

                Image ccImg = coinCounterHUD.GetComponent<Image>();
                ccImg.color = new Color(0.12f, 0.16f, 0.26f, 0.85f); // Frosted pill
                if (pillButtonSprite != null)
                {
                    ccImg.sprite = pillButtonSprite;
                    ccImg.type   = Image.Type.Sliced;
                }

                // Coin icon
                GameObject iconGO = new GameObject("CoinIcon", typeof(RectTransform), typeof(Image));
                iconGO.transform.SetParent(coinCounterHUD.transform, false);
                RectTransform iconRT = iconGO.GetComponent<RectTransform>();
                iconRT.anchorMin = new Vector2(0f, 0.5f);
                iconRT.anchorMax = new Vector2(0f, 0.5f);
                iconRT.pivot     = new Vector2(0.5f, 0.5f);
                iconRT.anchoredPosition = new Vector2(22f, 0f);
                iconRT.sizeDelta = new Vector2(28f, 28f);

                Image iconImg = iconGO.GetComponent<Image>();
                if (coinSprite != null)
                {
                    iconImg.sprite = coinSprite;
                    iconImg.color = Color.white;
                }
                else
                {
                    iconImg.color = new Color(1f, 0.85f, 0.1f, 1f);
                }

                // Coin text
                GameObject textGO = new GameObject("CoinText", typeof(RectTransform), typeof(TextMeshProUGUI));
                textGO.transform.SetParent(coinCounterHUD.transform, false);
                RectTransform textRT = textGO.GetComponent<RectTransform>();
                textRT.anchorMin = new Vector2(0f, 0f);
                textRT.anchorMax = new Vector2(1f, 1f);
                textRT.pivot     = new Vector2(0.5f, 0.5f);
                textRT.anchoredPosition = new Vector2(16f, 0f);
                textRT.sizeDelta = new Vector2(-36f, 0f);

                runCoinText = textGO.GetComponent<TextMeshProUGUI>();
                runCoinText.text = "0";
                runCoinText.fontSize = 22;
                runCoinText.fontStyle = FontStyles.Bold;
                runCoinText.color = Color.white;
                runCoinText.alignment = TextAlignmentOptions.MidlineLeft;
            }
        }

        if (coinCounterHUD != null)
        {
            Transform existingIcon = coinCounterHUD.transform.Find("CoinIcon");
            if (existingIcon != null)
            {
                Image iconImg = existingIcon.GetComponent<Image>();
                if (iconImg != null && coinSprite != null)
                {
                    iconImg.sprite = coinSprite;
                    iconImg.color = Color.white;
                }
            }
        }

        // 3. Polish Main Menu Buttons
        if (mainMenuPanel != null)
        {
            if (startButton != null)
            {
                RectTransform sRT = startButton.GetComponent<RectTransform>();
                sRT.anchoredPosition = new Vector2(0f, -50f);
                sRT.sizeDelta = new Vector2(280f, 58f);

                Image sImg = startButton.GetComponent<Image>();
                if (sImg != null)
                {
                    sImg.color = new Color(0.18f, 0.72f, 0.35f, 1f); // Vibrant Emerald Green
                    if (pillButtonSprite != null)
                    {
                        sImg.sprite = pillButtonSprite;
                        sImg.type   = Image.Type.Sliced;
                    }
                }

                TextMeshProUGUI st = startButton.GetComponentInChildren<TextMeshProUGUI>();
                if (st != null)
                {
                    st.text = "START FLAPPING";
                    st.fontSize = 24;
                    st.fontStyle = FontStyles.Bold;
                    st.color = Color.white;
                }
            }

            if (menuShopButton == null)
            {
                Transform existingBtn = mainMenuPanel.transform.Find("MenuShopButton");
                if (existingBtn != null) menuShopButton = existingBtn.GetComponent<Button>();
                else
                {
                    GameObject msGO = new GameObject("MenuShopButton", typeof(RectTransform), typeof(Image), typeof(Button));
                    msGO.transform.SetParent(mainMenuPanel.transform, false);
                    menuShopButton = msGO.GetComponent<Button>();

                    Image msImg = msGO.GetComponent<Image>();
                    msImg.color = new Color(0.2f, 0.45f, 0.85f, 1f); // Vibrant Blue
                    if (pillButtonSprite != null)
                    {
                        msImg.sprite = pillButtonSprite;
                        msImg.type   = Image.Type.Sliced;
                    }

                    GameObject msTextGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                    msTextGO.transform.SetParent(msGO.transform, false);
                    RectTransform msTextRT = msTextGO.GetComponent<RectTransform>();
                    msTextRT.anchorMin = Vector2.zero;
                    msTextRT.anchorMax = Vector2.one;
                    msTextRT.sizeDelta = Vector2.zero;

                    TextMeshProUGUI msText = msTextGO.GetComponent<TextMeshProUGUI>();
                    msText.text = "BIRD WARDROBE";
                    msText.fontSize = 20;
                    msText.fontStyle = FontStyles.Bold;
                    msText.color = Color.white;
                    msText.alignment = TextAlignmentOptions.Center;
                }
            }
            RectTransform msbRT = menuShopButton.GetComponent<RectTransform>();
            msbRT.anchoredPosition = new Vector2(0f, -125f);
            msbRT.sizeDelta = new Vector2(280f, 50f);

            // Add Menu Stats (Total Coins + Best Score)
            if (menuTotalCoinsText == null)
            {
                Transform existing = mainMenuPanel.transform.Find("MenuTotalCoins");
                if (existing != null) menuTotalCoinsText = existing.GetComponent<TextMeshProUGUI>();
                else
                {
                    GameObject mtcGO = new GameObject("MenuTotalCoins", typeof(RectTransform), typeof(TextMeshProUGUI));
                    mtcGO.transform.SetParent(mainMenuPanel.transform, false);
                    RectTransform mtcRT = mtcGO.GetComponent<RectTransform>();
                    mtcRT.anchorMin = new Vector2(0.5f, 0.5f);
                    mtcRT.anchorMax = new Vector2(0.5f, 0.5f);
                    mtcRT.anchoredPosition = new Vector2(0f, -185f);
                    mtcRT.sizeDelta = new Vector2(300f, 35f);

                    menuTotalCoinsText = mtcGO.GetComponent<TextMeshProUGUI>();
                    menuTotalCoinsText.text = "COINS: 0";
                    menuTotalCoinsText.fontSize = 22;
                    menuTotalCoinsText.fontStyle = FontStyles.Bold;
                    menuTotalCoinsText.color = new Color(1f, 0.85f, 0.15f, 1f);
                    menuTotalCoinsText.alignment = TextAlignmentOptions.Center;
                }
            }
        }
    }

    static void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null) panel.SetActive(active);
    }
}
