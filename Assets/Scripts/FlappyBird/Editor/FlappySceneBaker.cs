#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class FlappySceneBaker
{
    [MenuItem("Tools/Flappy Bird/Bake Scene UI and Managers")]
    public static void Bake()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "Flappy")
        {
            scene = EditorSceneManager.OpenScene("Assets/Scenes/Flappy.unity", OpenSceneMode.Single);
        }

        Debug.Log("[FlappySceneBaker] Starting Bake for scene: " + scene.path);

        // 1. Load Assets
        Sprite cardBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Cosmetics/ui_card_bg.png");
        Sprite pillBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Cosmetics/ui_button_pill.png");
        Sprite coinSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/coin.png") 
                         ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/coin.png");
        Sprite heartSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/heart.png") 
                          ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/heart.png");

        // 2. Find Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[FlappySceneBaker] Canvas not found in scene!");
            return;
        }

        HUDController hud = canvas.GetComponent<HUDController>();
        if (hud == null) hud = canvas.gameObject.AddComponent<HUDController>();

        // 3. Find or Create Managers in Scene
        GameObject pidgeotto = GameObject.FindWithTag("Player");
        BirdCosmetics birdCosmetics = null;
        if (pidgeotto != null)
        {
            birdCosmetics = pidgeotto.GetComponent<BirdCosmetics>();
            if (birdCosmetics == null) birdCosmetics = pidgeotto.AddComponent<BirdCosmetics>();
            EditorUtility.SetDirty(pidgeotto);
        }

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            CameraController camCtrl = mainCam.GetComponent<CameraController>();
            if (camCtrl == null) camCtrl = mainCam.gameObject.AddComponent<CameraController>();
            EditorUtility.SetDirty(mainCam.gameObject);
        }

        VisualEffectsManager vfxMgr = Object.FindFirstObjectByType<VisualEffectsManager>();
        if (vfxMgr == null)
        {
            GameObject vfxGO = new GameObject("VisualEffectsManager");
            vfxMgr = vfxGO.AddComponent<VisualEffectsManager>();
            Undo.RegisterCreatedObjectUndo(vfxGO, "Create VisualEffectsManager");
        }

        ShopManager shopMgr = Object.FindFirstObjectByType<ShopManager>();
        if (shopMgr == null)
        {
            GameObject shopGO = new GameObject("ShopManager");
            shopMgr = shopGO.AddComponent<ShopManager>();
            Undo.RegisterCreatedObjectUndo(shopGO, "Create ShopManager");
        }

        // 4. Style Main Menu Panel
        Transform mmT = canvas.transform.Find("MainMenuPanel");
        GameObject mainMenuPanel = mmT != null ? mmT.gameObject : null;
        Button startButton = null;
        Button menuShopButton = null;
        TextMeshProUGUI menuTotalCoinsText = null;

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);

            // Start Button
            Transform sbT = mainMenuPanel.transform.Find("StartButton");
            if (sbT != null)
            {
                startButton = sbT.GetComponent<Button>();
                RectTransform sbRT = sbT.GetComponent<RectTransform>();
                sbRT.anchoredPosition = new Vector2(0f, -50f);
                sbRT.sizeDelta = new Vector2(280f, 58f);

                Image sbImg = sbT.GetComponent<Image>();
                if (sbImg != null)
                {
                    sbImg.sprite = pillBtnSprite;
                    sbImg.type = Image.Type.Sliced;
                    sbImg.color = new Color(0.18f, 0.72f, 0.35f, 1f); // Vibrant Emerald Green
                }

                TextMeshProUGUI sbText = sbT.GetComponentInChildren<TextMeshProUGUI>();
                if (sbText != null)
                {
                    sbText.text = "START FLAPPING";
                    sbText.fontSize = 24;
                    sbText.fontStyle = FontStyles.Bold;
                    sbText.color = Color.white;
                    sbText.alignment = TextAlignmentOptions.Center;
                }
            }

            // Menu Shop Button ("BIRD WARDROBE")
            Transform msbT = mainMenuPanel.transform.Find("MenuShopButton");
            GameObject msbGO = msbT != null ? msbT.gameObject : null;
            if (msbGO == null)
            {
                msbGO = new GameObject("MenuShopButton", typeof(RectTransform), typeof(Image), typeof(Button));
                msbGO.transform.SetParent(mainMenuPanel.transform, false);
            }
            menuShopButton = msbGO.GetComponent<Button>();
            RectTransform msbRT = msbGO.GetComponent<RectTransform>();
            msbRT.anchoredPosition = new Vector2(0f, -125f);
            msbRT.sizeDelta = new Vector2(280f, 50f);

            Image msbImg = msbGO.GetComponent<Image>();
            msbImg.sprite = pillBtnSprite;
            msbImg.type = Image.Type.Sliced;
            msbImg.color = new Color(0.2f, 0.45f, 0.85f, 1f); // Vibrant Blue

            Transform msbTextT = msbGO.transform.Find("Text");
            GameObject msbTextGO = msbTextT != null ? msbTextT.gameObject : null;
            if (msbTextGO == null)
            {
                msbTextGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                msbTextGO.transform.SetParent(msbGO.transform, false);
            }
            RectTransform msbTextRT = msbTextGO.GetComponent<RectTransform>();
            msbTextRT.anchorMin = Vector2.zero;
            msbTextRT.anchorMax = Vector2.one;
            msbTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI msbTMP = msbTextGO.GetComponent<TextMeshProUGUI>();
            msbTMP.text = "BIRD WARDROBE";
            msbTMP.fontSize = 20;
            msbTMP.fontStyle = FontStyles.Bold;
            msbTMP.color = Color.white;
            msbTMP.alignment = TextAlignmentOptions.Center;

            // Total Coins Text
            Transform mtcT = mainMenuPanel.transform.Find("MenuTotalCoins");
            GameObject mtcGO = mtcT != null ? mtcT.gameObject : null;
            if (mtcGO == null)
            {
                mtcGO = new GameObject("MenuTotalCoins", typeof(RectTransform), typeof(TextMeshProUGUI));
                mtcGO.transform.SetParent(mainMenuPanel.transform, false);
            }
            RectTransform mtcRT = mtcGO.GetComponent<RectTransform>();
            mtcRT.anchorMin = new Vector2(0.5f, 0.5f);
            mtcRT.anchorMax = new Vector2(0.5f, 0.5f);
            mtcRT.anchoredPosition = new Vector2(0f, -185f);
            mtcRT.sizeDelta = new Vector2(300f, 35f);

            menuTotalCoinsText = mtcGO.GetComponent<TextMeshProUGUI>();
            menuTotalCoinsText.text = "COINS: 0";
            menuTotalCoinsText.fontSize = 22;
            menuTotalCoinsText.fontStyle = FontStyles.Bold;
            menuTotalCoinsText.color = new Color(1f, 0.85f, 0.15f, 1f); // Golden yellow
            menuTotalCoinsText.alignment = TextAlignmentOptions.Center;
        }

        // 5. Style Game Over Panel
        Transform goT = canvas.transform.Find("GameOverPanel");
        GameObject gameOverPanel = goT != null ? goT.gameObject : null;
        TextMeshProUGUI gameOverTitleText = null;
        TextMeshProUGUI finalScoreText = null;
        TextMeshProUGUI bestScoreText = null;
        TextMeshProUGUI coinsEarnedText = null;
        Button restartButton = null;
        Button gameOverShopButton = null;

        if (gameOverPanel != null)
        {
            RectTransform gopRT = gameOverPanel.GetComponent<RectTransform>();
            gopRT.anchorMin = new Vector2(0.5f, 0.5f);
            gopRT.anchorMax = new Vector2(0.5f, 0.5f);
            gopRT.pivot = new Vector2(0.5f, 0.5f);
            gopRT.sizeDelta = new Vector2(460f, 440f);
            gopRT.anchoredPosition = Vector2.zero;

            Image gopImg = gameOverPanel.GetComponent<Image>();
            if (gopImg == null) gopImg = gameOverPanel.AddComponent<Image>();
            gopImg.sprite = cardBgSprite;
            gopImg.type = Image.Type.Sliced;
            gopImg.color = new Color(0.08f, 0.11f, 0.18f, 0.95f); // Deep frosted midnight slate

            // GameOverTitle
            Transform gotT = gameOverPanel.transform.Find("GameOverTitle");
            GameObject gotGO = gotT != null ? gotT.gameObject : null;
            if (gotGO == null)
            {
                gotGO = new GameObject("GameOverTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
                gotGO.transform.SetParent(gameOverPanel.transform, false);
            }
            gameOverTitleText = gotGO.GetComponent<TextMeshProUGUI>();
            RectTransform gotRT = gotGO.GetComponent<RectTransform>();
            gotRT.anchoredPosition = new Vector2(0f, 160f);
            gotRT.sizeDelta = new Vector2(400f, 60f);
            gameOverTitleText.text = "GAME OVER";
            gameOverTitleText.fontSize = 44;
            gameOverTitleText.fontStyle = FontStyles.Bold;
            gameOverTitleText.color = new Color(1f, 0.25f, 0.25f, 1f); // Vibrant crimson
            gameOverTitleText.alignment = TextAlignmentOptions.Center;

            // FinalScoreText
            Transform fstT = gameOverPanel.transform.Find("FinalScoreText");
            if (fstT != null)
            {
                finalScoreText = fstT.GetComponent<TextMeshProUGUI>();
                RectTransform fstRT = fstT.GetComponent<RectTransform>();
                fstRT.anchoredPosition = new Vector2(0f, 95f);
                fstRT.sizeDelta = new Vector2(400f, 45f);
                finalScoreText.text = "SCORE: 0";
                finalScoreText.fontSize = 32;
                finalScoreText.fontStyle = FontStyles.Bold;
                finalScoreText.color = Color.white;
                finalScoreText.alignment = TextAlignmentOptions.Center;
            }

            // BestScoreText
            Transform bstT = gameOverPanel.transform.Find("BestScoreText");
            if (bstT != null)
            {
                bestScoreText = bstT.GetComponent<TextMeshProUGUI>();
                RectTransform bstRT = bstT.GetComponent<RectTransform>();
                bstRT.anchoredPosition = new Vector2(0f, 45f);
                bstRT.sizeDelta = new Vector2(400f, 40f);
                bestScoreText.text = "BEST: 0";
                bestScoreText.fontSize = 26;
                bestScoreText.fontStyle = FontStyles.Bold;
                bestScoreText.color = new Color(1f, 0.85f, 0.1f, 1f);
                bestScoreText.alignment = TextAlignmentOptions.Center;
            }

            // CoinsEarnedText
            Transform cetT = gameOverPanel.transform.Find("CoinsEarnedText");
            GameObject cetGO = cetT != null ? cetT.gameObject : null;
            if (cetGO == null)
            {
                cetGO = new GameObject("CoinsEarnedText", typeof(RectTransform), typeof(TextMeshProUGUI));
                cetGO.transform.SetParent(gameOverPanel.transform, false);
            }
            coinsEarnedText = cetGO.GetComponent<TextMeshProUGUI>();
            RectTransform cetRT = cetGO.GetComponent<RectTransform>();
            cetRT.anchoredPosition = new Vector2(0f, -5f);
            cetRT.sizeDelta = new Vector2(400f, 35f);
            coinsEarnedText.text = "COINS: +0";
            coinsEarnedText.fontSize = 24;
            coinsEarnedText.fontStyle = FontStyles.Bold;
            coinsEarnedText.color = new Color(0.3f, 0.85f, 1f, 1f); // Bright cyan
            coinsEarnedText.alignment = TextAlignmentOptions.Center;

            // RestartButton ("PLAY AGAIN")
            Transform rbT = gameOverPanel.transform.Find("RestartButton");
            if (rbT != null)
            {
                restartButton = rbT.GetComponent<Button>();
                RectTransform rbRT = rbT.GetComponent<RectTransform>();
                rbRT.anchoredPosition = new Vector2(0f, -75f);
                rbRT.sizeDelta = new Vector2(280f, 54f);

                Image rbImg = rbT.GetComponent<Image>();
                if (rbImg != null)
                {
                    rbImg.sprite = pillBtnSprite;
                    rbImg.type = Image.Type.Sliced;
                    rbImg.color = new Color(0.18f, 0.72f, 0.35f, 1f); // Emerald green
                }

                TextMeshProUGUI rbText = rbT.GetComponentInChildren<TextMeshProUGUI>();
                if (rbText != null)
                {
                    rbText.text = "PLAY AGAIN";
                    rbText.fontSize = 24;
                    rbText.fontStyle = FontStyles.Bold;
                    rbText.color = Color.white;
                    rbText.alignment = TextAlignmentOptions.Center;
                }
            }

            // GameOverShopButton ("BIRD SHOP")
            Transform gosbT = gameOverPanel.transform.Find("GameOverShopButton");
            GameObject gosbGO = gosbT != null ? gosbT.gameObject : null;
            if (gosbGO == null)
            {
                gosbGO = new GameObject("GameOverShopButton", typeof(RectTransform), typeof(Image), typeof(Button));
                gosbGO.transform.SetParent(gameOverPanel.transform, false);
            }
            gameOverShopButton = gosbGO.GetComponent<Button>();
            RectTransform gosbRT = gosbGO.GetComponent<RectTransform>();
            gosbRT.anchoredPosition = new Vector2(0f, -140f);
            gosbRT.sizeDelta = new Vector2(280f, 48f);

            Image gosbImg = gosbGO.GetComponent<Image>();
            gosbImg.sprite = pillBtnSprite;
            gosbImg.type = Image.Type.Sliced;
            gosbImg.color = new Color(0.2f, 0.45f, 0.85f, 1f); // Blue

            Transform gosbTextT = gosbGO.transform.Find("Text");
            GameObject gosbTextGO = gosbTextT != null ? gosbTextT.gameObject : null;
            if (gosbTextGO == null)
            {
                gosbTextGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                gosbTextGO.transform.SetParent(gosbGO.transform, false);
            }
            RectTransform gosbTextRT = gosbTextGO.GetComponent<RectTransform>();
            gosbTextRT.anchorMin = Vector2.zero;
            gosbTextRT.anchorMax = Vector2.one;
            gosbTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI gosbTMP = gosbTextGO.GetComponent<TextMeshProUGUI>();
            gosbTMP.text = "BIRD SHOP";
            gosbTMP.fontSize = 20;
            gosbTMP.fontStyle = FontStyles.Bold;
            gosbTMP.color = Color.white;
            gosbTMP.alignment = TextAlignmentOptions.Center;

            // In edit mode keep it inactive by default so user sees main menu
            gameOverPanel.SetActive(false);
        }

        // 6. Style Coin Counter HUD
        Transform ccT = canvas.transform.Find("CoinCounterHUD");
        GameObject coinCounterHUD = ccT != null ? ccT.gameObject : null;
        TextMeshProUGUI runCoinText = null;

        if (coinCounterHUD == null)
        {
            coinCounterHUD = new GameObject("CoinCounterHUD", typeof(RectTransform), typeof(Image));
            coinCounterHUD.transform.SetParent(canvas.transform, false);
        }

        RectTransform ccRT = coinCounterHUD.GetComponent<RectTransform>();
        ccRT.anchorMin = new Vector2(1f, 1f);
        ccRT.anchorMax = new Vector2(1f, 1f);
        ccRT.pivot = new Vector2(1f, 1f);
        ccRT.anchoredPosition = new Vector2(-20f, -20f);
        ccRT.sizeDelta = new Vector2(110f, 44f);

        Image ccImg = coinCounterHUD.GetComponent<Image>();
        ccImg.sprite = pillBtnSprite;
        ccImg.type = Image.Type.Sliced;
        ccImg.color = new Color(0.12f, 0.16f, 0.26f, 0.85f); // Frosted pill

        // Coin Icon
        Transform cIconT = coinCounterHUD.transform.Find("CoinIcon");
        GameObject cIconGO = cIconT != null ? cIconT.gameObject : null;
        if (cIconGO == null)
        {
            cIconGO = new GameObject("CoinIcon", typeof(RectTransform), typeof(Image));
            cIconGO.transform.SetParent(coinCounterHUD.transform, false);
        }
        RectTransform cIconRT = cIconGO.GetComponent<RectTransform>();
        cIconRT.anchorMin = new Vector2(0f, 0.5f);
        cIconRT.anchorMax = new Vector2(0f, 0.5f);
        cIconRT.pivot = new Vector2(0.5f, 0.5f);
        cIconRT.anchoredPosition = new Vector2(22f, 0f);
        cIconRT.sizeDelta = new Vector2(28f, 28f);

        Image cIconImg = cIconGO.GetComponent<Image>();
        cIconImg.sprite = coinSprite;
        cIconImg.color = Color.white;

        // Coin Text
        Transform cTextT = coinCounterHUD.transform.Find("CoinText");
        GameObject cTextGO = cTextT != null ? cTextT.gameObject : null;
        if (cTextGO == null)
        {
            cTextGO = new GameObject("CoinText", typeof(RectTransform), typeof(TextMeshProUGUI));
            cTextGO.transform.SetParent(coinCounterHUD.transform, false);
        }
        RectTransform cTextRT = cTextGO.GetComponent<RectTransform>();
        cTextRT.anchorMin = new Vector2(0f, 0f);
        cTextRT.anchorMax = new Vector2(1f, 1f);
        cTextRT.pivot = new Vector2(0.5f, 0.5f);
        cTextRT.anchoredPosition = new Vector2(16f, 0f);
        cTextRT.sizeDelta = new Vector2(-36f, 0f);

        runCoinText = cTextGO.GetComponent<TextMeshProUGUI>();
        runCoinText.text = "0";
        runCoinText.fontSize = 22;
        runCoinText.fontStyle = FontStyles.Bold;
        runCoinText.color = Color.white;
        runCoinText.alignment = TextAlignmentOptions.MidlineLeft;

        coinCounterHUD.SetActive(true);

        // 7. Life Icons Heart Sprites
        Transform lcT = canvas.transform.Find("LivesContainer");
        List<GameObject> lifeIconsList = new List<GameObject>();
        if (lcT != null)
        {
            for (int i = 0; i < lcT.childCount; i++)
            {
                Transform child = lcT.GetChild(i);
                lifeIconsList.Add(child.gameObject);
                Image img = child.GetComponent<Image>();
                if (img != null && heartSprite != null)
                {
                    img.sprite = heartSprite;
                    img.color = Color.white;
                }
            }
        }

        // 8. Build ShopUIController and ShopPanel under Canvas
        Transform shopCtrlT = canvas.transform.Find("ShopUIController");
        GameObject shopCtrlGO = shopCtrlT != null ? shopCtrlT.gameObject : null;
        if (shopCtrlGO == null)
        {
            shopCtrlGO = new GameObject("ShopUIController");
            shopCtrlGO.transform.SetParent(canvas.transform, false);
        }
        ShopUIController shopUI = shopCtrlGO.GetComponent<ShopUIController>();
        if (shopUI == null) shopUI = shopCtrlGO.AddComponent<ShopUIController>();

        // ShopPanel
        Transform spT = canvas.transform.Find("ShopPanel");
        GameObject shopPanel = spT != null ? spT.gameObject : null;
        if (shopPanel == null)
        {
            shopPanel = new GameObject("ShopPanel", typeof(RectTransform), typeof(Image));
            shopPanel.transform.SetParent(canvas.transform, false);
        }
        RectTransform spRT = shopPanel.GetComponent<RectTransform>();
        spRT.anchorMin = new Vector2(0.5f, 0.5f);
        spRT.anchorMax = new Vector2(0.5f, 0.5f);
        spRT.pivot = new Vector2(0.5f, 0.5f);
        spRT.sizeDelta = new Vector2(460f, 560f);
        spRT.anchoredPosition = Vector2.zero;

        Image spImg = shopPanel.GetComponent<Image>();
        spImg.sprite = cardBgSprite;
        spImg.type = Image.Type.Sliced;
        spImg.color = new Color(0.08f, 0.11f, 0.18f, 0.96f);

        // Shop HeaderTitle
        Transform shtT = shopPanel.transform.Find("HeaderTitle");
        GameObject shtGO = shtT != null ? shtT.gameObject : null;
        if (shtGO == null)
        {
            shtGO = new GameObject("HeaderTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            shtGO.transform.SetParent(shopPanel.transform, false);
        }
        RectTransform shtRT = shtGO.GetComponent<RectTransform>();
        shtRT.anchorMin = new Vector2(0.5f, 1f);
        shtRT.anchorMax = new Vector2(0.5f, 1f);
        shtRT.pivot = new Vector2(0.5f, 1f);
        shtRT.anchoredPosition = new Vector2(0f, -20f);
        shtRT.sizeDelta = new Vector2(250f, 40f);

        TextMeshProUGUI shtTMP = shtGO.GetComponent<TextMeshProUGUI>();
        shtTMP.text = "WARDROBE";
        shtTMP.fontSize = 28;
        shtTMP.fontStyle = FontStyles.Bold;
        shtTMP.color = new Color(0.3f, 0.85f, 1f, 1f); // Bright cyan
        shtTMP.alignment = TextAlignmentOptions.Center;

        // Shop CloseButton
        Transform scbT = shopPanel.transform.Find("CloseButton");
        GameObject scbGO = scbT != null ? scbT.gameObject : null;
        if (scbGO == null)
        {
            scbGO = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
            scbGO.transform.SetParent(shopPanel.transform, false);
        }
        Button shopCloseBtn = scbGO.GetComponent<Button>();
        RectTransform scbRT = scbGO.GetComponent<RectTransform>();
        scbRT.anchorMin = new Vector2(0f, 1f);
        scbRT.anchorMax = new Vector2(0f, 1f);
        scbRT.pivot = new Vector2(0f, 1f);
        scbRT.anchoredPosition = new Vector2(18f, -18f);
        scbRT.sizeDelta = new Vector2(85f, 38f);

        Image scbImg = scbGO.GetComponent<Image>();
        scbImg.sprite = pillBtnSprite;
        scbImg.type = Image.Type.Sliced;
        scbImg.color = new Color(0.3f, 0.35f, 0.45f, 1f);

        Transform scbTextT = scbGO.transform.Find("Text");
        GameObject scbTextGO = scbTextT != null ? scbTextT.gameObject : null;
        if (scbTextGO == null)
        {
            scbTextGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            scbTextGO.transform.SetParent(scbGO.transform, false);
        }
        RectTransform scbTextRT = scbTextGO.GetComponent<RectTransform>();
        scbTextRT.anchorMin = Vector2.zero;
        scbTextRT.anchorMax = Vector2.one;
        scbTextRT.sizeDelta = Vector2.zero;

        TextMeshProUGUI scbTMP = scbTextGO.GetComponent<TextMeshProUGUI>();
        scbTMP.text = "< BACK";
        scbTMP.fontSize = 15;
        scbTMP.fontStyle = FontStyles.Bold;
        scbTMP.color = Color.white;
        scbTMP.alignment = TextAlignmentOptions.Center;

        // Shop CoinBadge
        Transform scBadgeT = shopPanel.transform.Find("CoinBadge");
        GameObject scBadgeGO = scBadgeT != null ? scBadgeT.gameObject : null;
        if (scBadgeGO == null)
        {
            scBadgeGO = new GameObject("CoinBadge", typeof(RectTransform), typeof(TextMeshProUGUI));
            scBadgeGO.transform.SetParent(shopPanel.transform, false);
        }
        RectTransform scbBadgeRT = scBadgeGO.GetComponent<RectTransform>();
        scbBadgeRT.anchorMin = new Vector2(1f, 1f);
        scbBadgeRT.anchorMax = new Vector2(1f, 1f);
        scbBadgeRT.pivot = new Vector2(1f, 1f);
        scbBadgeRT.anchoredPosition = new Vector2(-18f, -20f);
        scbBadgeRT.sizeDelta = new Vector2(120f, 38f);

        TextMeshProUGUI shopCoinBalanceText = scBadgeGO.GetComponent<TextMeshProUGUI>();
        shopCoinBalanceText.text = "COINS: 0";
        shopCoinBalanceText.fontSize = 20;
        shopCoinBalanceText.fontStyle = FontStyles.Bold;
        shopCoinBalanceText.color = new Color(1f, 0.85f, 0.15f, 1f);
        shopCoinBalanceText.alignment = TextAlignmentOptions.MidlineRight;

        // Shop CategoryTabs
        Transform sctT = shopPanel.transform.Find("CategoryTabs");
        GameObject sctGO = sctT != null ? sctT.gameObject : null;
        if (sctGO == null)
        {
            sctGO = new GameObject("CategoryTabs", typeof(RectTransform));
            sctGO.transform.SetParent(shopPanel.transform, false);
        }
        RectTransform sctRT = sctGO.GetComponent<RectTransform>();
        sctRT.anchorMin = new Vector2(0.5f, 1f);
        sctRT.anchorMax = new Vector2(0.5f, 1f);
        sctRT.pivot = new Vector2(0.5f, 1f);
        sctRT.anchoredPosition = new Vector2(0f, -75f);
        sctRT.sizeDelta = new Vector2(420f, 44f);

        Button tabHats = CreateOrUpdateTab(sctGO.transform, "Tab_HATS", "HATS", new Vector2(-140f, 0f), pillBtnSprite, new Color(0.2f, 0.6f, 1f, 1f));
        Button tabSkins = CreateOrUpdateTab(sctGO.transform, "Tab_SKINS", "SKINS", new Vector2(0f, 0f), pillBtnSprite, new Color(0.25f, 0.3f, 0.42f, 0.9f));
        Button tabTrails = CreateOrUpdateTab(sctGO.transform, "Tab_TRAILS", "TRAILS", new Vector2(140f, 0f), pillBtnSprite, new Color(0.25f, 0.3f, 0.42f, 0.9f));

        // Shop ScrollArea & Content
        Transform ssaT = shopPanel.transform.Find("ScrollArea");
        GameObject ssaGO = ssaT != null ? ssaT.gameObject : null;
        if (ssaGO == null)
        {
            ssaGO = new GameObject("ScrollArea", typeof(RectTransform), typeof(RectMask2D), typeof(ScrollRect));
            ssaGO.transform.SetParent(shopPanel.transform, false);
        }
        RectTransform ssaRT = ssaGO.GetComponent<RectTransform>();
        ssaRT.anchorMin = new Vector2(0.5f, 1f);
        ssaRT.anchorMax = new Vector2(0.5f, 1f);
        ssaRT.pivot = new Vector2(0.5f, 1f);
        ssaRT.anchoredPosition = new Vector2(0f, -130f);
        ssaRT.sizeDelta = new Vector2(430f, 400f);

        ScrollRect shopScrollRect = ssaGO.GetComponent<ScrollRect>();
        shopScrollRect.horizontal = false;
        shopScrollRect.vertical = true;

        Transform sContentT = ssaGO.transform.Find("Content");
        GameObject sContentGO = sContentT != null ? sContentT.gameObject : null;
        if (sContentGO == null)
        {
            sContentGO = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            sContentGO.transform.SetParent(ssaGO.transform, false);
        }
        RectTransform sContentRT = sContentGO.GetComponent<RectTransform>();
        sContentRT.anchorMin = new Vector2(0f, 1f);
        sContentRT.anchorMax = new Vector2(1f, 1f);
        sContentRT.pivot = new Vector2(0.5f, 1f);
        sContentRT.sizeDelta = Vector2.zero;

        VerticalLayoutGroup vlg = sContentGO.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 8f;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = false;
        vlg.childControlHeight = false;

        ContentSizeFitter csf = sContentGO.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        shopScrollRect.content = sContentRT;

        shopPanel.SetActive(false); // Inactive until clicked in-game

        // 9. Assign Fields on ShopUIController
        shopUI.shopPanel = shopPanel;
        shopUI.closeButton = shopCloseBtn;
        shopUI.coinBalanceText = shopCoinBalanceText;
        shopUI.tabHatsButton = tabHats;
        shopUI.tabSkinsButton = tabSkins;
        shopUI.tabTrailsButton = tabTrails;
        shopUI.itemsContainer = sContentRT;
        shopUI.cardBgSprite = cardBgSprite;
        shopUI.pillButtonSprite = pillBtnSprite;
        shopUI.pikachuCoinSprite = coinSprite;
        shopUI.previewBird = birdCosmetics;
        EditorUtility.SetDirty(shopUI);

        // 10. Assign Fields on HUDController
        hud.scoreText = canvas.transform.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
        hud.lifeIcons = lifeIconsList.ToArray();
        hud.coinCounterHUD = coinCounterHUD;
        hud.runCoinText = runCoinText;
        hud.gymLevelPanel = canvas.transform.Find("GymLevelPanel")?.gameObject;
        hud.gymLevelText = hud.gymLevelPanel?.transform.Find("GymLevelText")?.GetComponent<TextMeshProUGUI>();

        hud.mainMenuPanel = mainMenuPanel;
        hud.startButton = startButton;
        hud.menuShopButton = menuShopButton;
        hud.menuTotalCoinsText = menuTotalCoinsText;

        hud.gameOverPanel = gameOverPanel;
        hud.gameOverTitleText = gameOverTitleText;
        hud.finalScoreText = finalScoreText;
        hud.bestScoreText = bestScoreText;
        hud.coinsEarnedText = coinsEarnedText;
        hud.restartButton = restartButton;
        hud.gameOverShopButton = gameOverShopButton;

        hud.shopController = shopUI;
        hud.cardBackgroundSprite = cardBgSprite;
        hud.pillButtonSprite = pillBtnSprite;
        hud.coinSprite = coinSprite;
        hud.heartSprite = heartSprite;
        EditorUtility.SetDirty(hud);

        // 11. Bake Persistent Listeners on Buttons
        if (startButton != null)
        {
            UnityEditor.Events.UnityEventTools.RemovePersistentListener(startButton.onClick, hud.OnStartButtonClicked);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(startButton.onClick, hud.OnStartButtonClicked);
            EditorUtility.SetDirty(startButton);
        }

        if (restartButton != null)
        {
            UnityEditor.Events.UnityEventTools.RemovePersistentListener(restartButton.onClick, hud.OnRestartButtonClicked);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(restartButton.onClick, hud.OnRestartButtonClicked);
            EditorUtility.SetDirty(restartButton);
        }

        if (menuShopButton != null)
        {
            UnityEditor.Events.UnityEventTools.RemovePersistentListener(menuShopButton.onClick, hud.OnShopButtonClicked);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(menuShopButton.onClick, hud.OnShopButtonClicked);
            EditorUtility.SetDirty(menuShopButton);
        }

        if (gameOverShopButton != null)
        {
            UnityEditor.Events.UnityEventTools.RemovePersistentListener(gameOverShopButton.onClick, hud.OnShopButtonClicked);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(gameOverShopButton.onClick, hud.OnShopButtonClicked);
            EditorUtility.SetDirty(gameOverShopButton);
        }

        if (shopUI != null)
        {
            if (shopUI.closeButton != null)
            {
                UnityEditor.Events.UnityEventTools.RemovePersistentListener(shopUI.closeButton.onClick, shopUI.CloseShop);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(shopUI.closeButton.onClick, shopUI.CloseShop);
                EditorUtility.SetDirty(shopUI.closeButton);
            }
            if (shopUI.tabHatsButton != null)
            {
                UnityEditor.Events.UnityEventTools.RemovePersistentListener(shopUI.tabHatsButton.onClick, shopUI.SelectHats);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(shopUI.tabHatsButton.onClick, shopUI.SelectHats);
                EditorUtility.SetDirty(shopUI.tabHatsButton);
            }
            if (shopUI.tabSkinsButton != null)
            {
                UnityEditor.Events.UnityEventTools.RemovePersistentListener(shopUI.tabSkinsButton.onClick, shopUI.SelectSkins);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(shopUI.tabSkinsButton.onClick, shopUI.SelectSkins);
                EditorUtility.SetDirty(shopUI.tabSkinsButton);
            }
            if (shopUI.tabTrailsButton != null)
            {
                UnityEditor.Events.UnityEventTools.RemovePersistentListener(shopUI.tabTrailsButton.onClick, shopUI.SelectTrails);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(shopUI.tabTrailsButton.onClick, shopUI.SelectTrails);
                EditorUtility.SetDirty(shopUI.tabTrailsButton);
            }
        }

        // 12. Save Scene
        EditorUtility.SetDirty(canvas.gameObject);
        EditorSceneManager.MarkSceneDirty(scene);
        bool saved = EditorSceneManager.SaveScene(scene);
        Debug.Log("[FlappySceneBaker] Bake complete! Scene saved: " + saved);
    }

    static Button CreateOrUpdateTab(Transform parent, string goName, string label, Vector2 pos, Sprite sprite, Color color)
    {
        Transform t = parent.Find(goName);
        GameObject go = t != null ? t.gameObject : null;
        if (go == null)
        {
            go = new GameObject(goName, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
        }
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(130f, 40f);

        Image img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Sliced;
        img.color = color;

        Button btn = go.GetComponent<Button>();

        Transform textT = go.transform.Find("Text");
        GameObject textGO = textT != null ? textT.gameObject : null;
        if (textGO == null)
        {
            textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGO.transform.SetParent(go.transform, false);
        }
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 16;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        return btn;
    }
}
#endif
