using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the UI and interactions in the Bird Customization Shop.
/// Dynamically populates items, updates coin balances, and supports live preview.
/// </summary>
public class ShopUIController : MonoBehaviour
{
    private static ShopUIController _instance;
    public static ShopUIController instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<ShopUIController>();
            }
            return _instance;
        }
        private set => _instance = value;
    }

    [Header("Panel References")]
    public GameObject      shopPanel;
    public Button          closeButton;
    public TextMeshProUGUI coinBalanceText;

    [Header("Category Navigation")]
    public Button tabHatsButton;
    public Button tabSkinsButton;
    public Button tabTrailsButton;

    [Header("Item Container")]
    public Transform itemsContainer;

    [Header("Live Bird Preview (Optional)")]
    public BirdCosmetics previewBird;

    [Header("Sprites")]
    public Sprite cardBgSprite;
    public Sprite pillButtonSprite;
    public Sprite pikachuCoinSprite;

    private CosmeticType currentCategory = CosmeticType.Hat;
    private List<GameObject> activeCards = new List<GameObject>();

    void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;

        AutoLoadSprites();
        EnsureShopPanel();
        WireButtons();
    }

    void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    void AutoLoadSprites()
    {
        if (pikachuCoinSprite == null)
            pikachuCoinSprite = Resources.Load<Sprite>("UI/coin") ?? Resources.Load<Sprite>("coin");

        if (cardBgSprite == null || pillButtonSprite == null || pikachuCoinSprite == null)
        {
            foreach (var s in Resources.FindObjectsOfTypeAll<Sprite>())
            {
                if (s.name == "ui_card_bg") cardBgSprite = s;
                else if (s.name == "ui_button_pill") pillButtonSprite = s;
                else if (s.name == "coin") pikachuCoinSprite = s;
                else if (s.name == "pikachuuu" && pikachuCoinSprite == null) pikachuCoinSprite = s;
            }
        }
    }

    void OnEnable()
    {
        FlappyGameManager.OnCoinsChanged += HandleCoinsChanged;
        ShopManager.OnShopUpdated        += RefreshUI;

        WireButtons();
    }

    void OnDisable()
    {
        FlappyGameManager.OnCoinsChanged -= HandleCoinsChanged;
        ShopManager.OnShopUpdated        -= RefreshUI;
    }

    public void WireButtons()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseShop);
            closeButton.onClick.AddListener(CloseShop);
        }

        if (tabHatsButton != null)
        {
            tabHatsButton.onClick.RemoveListener(SelectHats);
            tabHatsButton.onClick.AddListener(SelectHats);
        }

        if (tabSkinsButton != null)
        {
            tabSkinsButton.onClick.RemoveListener(SelectSkins);
            tabSkinsButton.onClick.AddListener(SelectSkins);
        }

        if (tabTrailsButton != null)
        {
            tabTrailsButton.onClick.RemoveListener(SelectTrails);
            tabTrailsButton.onClick.AddListener(SelectTrails);
        }
    }

    public void SelectHats()   => SwitchCategory(CosmeticType.Hat);
    public void SelectSkins()  => SwitchCategory(CosmeticType.Skin);
    public void SelectTrails() => SwitchCategory(CosmeticType.Trail);

    void Start()
    {
        WireButtons();

        if (shopPanel != null)
            shopPanel.SetActive(false);

        // Link with HUDController
        if (HUDController.instance != null && HUDController.instance.shopController == null)
        {
            HUDController.instance.shopController = this;
        }
    }

    private GameState openedFromState = GameState.MainMenu;

    public void OpenShop()
    {
        if (FlappyGameManager.instance != null)
            openedFromState = FlappyGameManager.instance.CurrentState;
        else
            openedFromState = GameState.MainMenu;

        if (shopPanel != null) shopPanel.SetActive(true);
        UpdateCoinBalance();
        SwitchCategory(CosmeticType.Hat);
        if (previewBird != null) previewBird.ApplyAllEquipped();
    }

    public void CloseShop()
    {
        if (shopPanel != null) shopPanel.SetActive(false);

        if (openedFromState == GameState.GameOver)
        {
            if (HUDController.instance != null)
                HUDController.instance.ShowGameOverScreen();
        }
        else
        {
            if (HUDController.instance != null)
                HUDController.instance.ShowMainMenu();
        }
    }

    public void SwitchCategory(CosmeticType type)
    {
        currentCategory = type;
        HighlightActiveTab();
        PopulateItems();
    }

    void HighlightActiveTab()
    {
        Color activeCol   = new Color(0.2f, 0.6f, 1f, 1f);      // Vibrant Blue
        Color inactiveCol = new Color(0.25f, 0.3f, 0.42f, 0.9f); // Slate Blue

        SetButtonColor(tabHatsButton,   currentCategory == CosmeticType.Hat   ? activeCol : inactiveCol);
        SetButtonColor(tabSkinsButton,  currentCategory == CosmeticType.Skin  ? activeCol : inactiveCol);
        SetButtonColor(tabTrailsButton, currentCategory == CosmeticType.Trail ? activeCol : inactiveCol);
    }

    void SetButtonColor(Button btn, Color c)
    {
        if (btn == null) return;
        Image img = btn.GetComponent<Image>();
        if (img != null) img.color = c;
    }

    void UpdateCoinBalance()
    {
        if (coinBalanceText != null && FlappyGameManager.instance != null)
        {
            coinBalanceText.text = $"COINS: {FlappyGameManager.instance.TotalCoins}";
        }
    }

    void HandleCoinsChanged(int runCoins, int totalCoins)
    {
        if (coinBalanceText != null)
            coinBalanceText.text = $"COINS: {totalCoins}";
        RefreshUI();
    }

    public void RefreshUI()
    {
        UpdateCoinBalance();
        PopulateItems();
        if (previewBird != null) previewBird.ApplyAllEquipped();
    }

    void PopulateItems()
    {
        if (itemsContainer == null || ShopManager.instance == null) return;

        // Clear previous cards
        foreach (var card in activeCards)
        {
            if (card != null) Destroy(card);
        }
        activeCards.Clear();

        List<CosmeticItem> items = ShopManager.instance.GetItemsByCategory(currentCategory);
        int totalCoins = FlappyGameManager.instance != null ? FlappyGameManager.instance.TotalCoins : 0;

        foreach (var item in items)
        {
            GameObject cardGO = CreateItemCard(item, totalCoins);
            activeCards.Add(cardGO);
        }
    }

    GameObject CreateItemCard(CosmeticItem item, int currentCoins)
    {
        GameObject card = new GameObject("ItemCard_" + item.id, typeof(RectTransform), typeof(Image));
        card.transform.SetParent(itemsContainer, false);

        RectTransform rt = card.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(410, 64);

        Image cardImg = card.GetComponent<Image>();
        cardImg.color = new Color(0.12f, 0.16f, 0.25f, 0.9f);
        if (cardBgSprite != null)
        {
            cardImg.sprite = cardBgSprite;
            cardImg.type   = Image.Type.Sliced;
        }

        // Icon preview
        GameObject iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGO.transform.SetParent(card.transform, false);
        RectTransform iconRT = iconGO.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0f, 0.5f);
        iconRT.anchorMax = new Vector2(0f, 0.5f);
        iconRT.pivot     = new Vector2(0.5f, 0.5f);
        iconRT.anchoredPosition = new Vector2(38, 0);
        iconRT.sizeDelta = new Vector2(44, 44);

        Image iconImg = iconGO.GetComponent<Image>();
        if (item.icon != null)
        {
            iconImg.sprite = item.icon;
            iconImg.color  = Color.white;
        }
        else
        {
            iconImg.color = item.color;
            if (pillButtonSprite != null)
            {
                iconImg.sprite = pillButtonSprite;
                iconImg.type   = Image.Type.Sliced;
            }
        }

        // Title text
        GameObject titleGO = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleGO.transform.SetParent(card.transform, false);
        RectTransform titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0f, 0.5f);
        titleRT.anchorMax = new Vector2(0f, 0.5f);
        titleRT.pivot     = new Vector2(0f, 0.5f);
        titleRT.anchoredPosition = new Vector2(75, 0);
        titleRT.sizeDelta = new Vector2(170, 44);

        TextMeshProUGUI titleTMP = titleGO.GetComponent<TextMeshProUGUI>();
        titleTMP.text = item.displayName;
        titleTMP.fontSize = 18;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color = Color.white;
        titleTMP.alignment = TextAlignmentOptions.MidlineLeft;

        // Action button (Buy / Equip / Equipped)
        GameObject btnGO = new GameObject("ActionButton", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(card.transform, false);
        RectTransform btnRT = btnGO.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(1f, 0.5f);
        btnRT.anchorMax = new Vector2(1f, 0.5f);
        btnRT.pivot     = new Vector2(1f, 0.5f);
        btnRT.anchoredPosition = new Vector2(-15, 0);
        btnRT.sizeDelta = new Vector2(120, 40);

        Image btnImg = btnGO.GetComponent<Image>();
        if (pillButtonSprite != null)
        {
            btnImg.sprite = pillButtonSprite;
            btnImg.type   = Image.Type.Sliced;
        }

        Button btn = btnGO.GetComponent<Button>();

        GameObject btnTextGO = new GameObject("BtnText", typeof(RectTransform), typeof(TextMeshProUGUI));
        btnTextGO.transform.SetParent(btnGO.transform, false);
        RectTransform btnTextRT = btnTextGO.GetComponent<RectTransform>();
        btnTextRT.anchorMin = Vector2.zero;
        btnTextRT.anchorMax = Vector2.one;
        btnTextRT.sizeDelta = Vector2.zero;

        TextMeshProUGUI btnTMP = btnTextGO.GetComponent<TextMeshProUGUI>();
        btnTMP.fontSize = 15;
        btnTMP.fontStyle = FontStyles.Bold;
        btnTMP.alignment = TextAlignmentOptions.Center;

        bool isUnlocked = item.IsUnlocked();
        bool isEquipped = item.IsEquipped();

        if (isEquipped)
        {
            btnImg.color = new Color(0.18f, 0.68f, 0.32f, 1f); // Emerald Green
            btnTMP.text  = "EQUIPPED";
            btnTMP.color = Color.white;
            btn.interactable = false;
        }
        else if (isUnlocked)
        {
            btnImg.color = new Color(0.15f, 0.48f, 0.88f, 1f); // Electric Blue
            btnTMP.text  = "EQUIP";
            btnTMP.color = Color.white;
            btn.interactable = true;
            btn.onClick.AddListener(() => {
                ShopManager.instance.Equip(item);
                if (previewBird != null) previewBird.ApplyAllEquipped();
            });
        }
        else
        {
            bool canAfford = currentCoins >= item.price;
            btnImg.color = canAfford ? new Color(0.92f, 0.72f, 0.12f, 1f) : new Color(0.35f, 0.35f, 0.4f, 0.8f);
            btnTMP.text  = $"{item.price} COINS";
            btnTMP.color = canAfford ? Color.black : new Color(0.7f, 0.7f, 0.7f, 1f);
            btn.interactable = canAfford;
            btn.onClick.AddListener(() => {
                if (ShopManager.instance.Buy(item))
                {
                    if (previewBird != null) previewBird.ApplyAllEquipped();
                }
            });
        }

        return card;
    }

    void EnsureShopPanel()
    {
        if (shopPanel != null) return;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        Transform existing = canvas.transform.Find("ShopPanel");
        if (existing != null)
        {
            shopPanel = existing.gameObject;
            return;
        }

        // Create ShopPanel
        shopPanel = new GameObject("ShopPanel", typeof(RectTransform), typeof(Image));
        shopPanel.transform.SetParent(canvas.transform, false);

        RectTransform spRT = shopPanel.GetComponent<RectTransform>();
        spRT.anchorMin = new Vector2(0.5f, 0.5f);
        spRT.anchorMax = new Vector2(0.5f, 0.5f);
        spRT.pivot     = new Vector2(0.5f, 0.5f);
        spRT.sizeDelta = new Vector2(460f, 560f);
        spRT.anchoredPosition = Vector2.zero;

        Image spImg = shopPanel.GetComponent<Image>();
        spImg.color = new Color(0.08f, 0.11f, 0.18f, 0.96f); // Midnight slate
        if (cardBgSprite != null)
        {
            spImg.sprite = cardBgSprite;
            spImg.type   = Image.Type.Sliced;
        }

        // Header Title
        GameObject headerGO = new GameObject("HeaderTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        headerGO.transform.SetParent(shopPanel.transform, false);
        RectTransform hRT = headerGO.GetComponent<RectTransform>();
        hRT.anchorMin = new Vector2(0.5f, 1f);
        hRT.anchorMax = new Vector2(0.5f, 1f);
        hRT.pivot     = new Vector2(0.5f, 1f);
        hRT.anchoredPosition = new Vector2(0f, -20f);
        hRT.sizeDelta = new Vector2(250f, 40f);

        TextMeshProUGUI hTMP = headerGO.GetComponent<TextMeshProUGUI>();
        hTMP.text = "WARDROBE";
        hTMP.fontSize = 28;
        hTMP.fontStyle = FontStyles.Bold;
        hTMP.color = new Color(0.3f, 0.85f, 1f, 1f);
        hTMP.alignment = TextAlignmentOptions.Center;

        // Close Button
        GameObject closeGO = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        closeGO.transform.SetParent(shopPanel.transform, false);
        RectTransform cRT = closeGO.GetComponent<RectTransform>();
        cRT.anchorMin = new Vector2(0f, 1f);
        cRT.anchorMax = new Vector2(0f, 1f);
        cRT.pivot     = new Vector2(0f, 1f);
        cRT.anchoredPosition = new Vector2(18f, -18f);
        cRT.sizeDelta = new Vector2(85f, 38f);

        Image cImg = closeGO.GetComponent<Image>();
        cImg.color = new Color(0.3f, 0.35f, 0.45f, 1f);
        if (pillButtonSprite != null)
        {
            cImg.sprite = pillButtonSprite;
            cImg.type   = Image.Type.Sliced;
        }
        closeButton = closeGO.GetComponent<Button>();

        GameObject cTextGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        cTextGO.transform.SetParent(closeGO.transform, false);
        RectTransform cTextRT = cTextGO.GetComponent<RectTransform>();
        cTextRT.anchorMin = Vector2.zero;
        cTextRT.anchorMax = Vector2.one;
        cTextRT.sizeDelta = Vector2.zero;
        TextMeshProUGUI cTMP = cTextGO.GetComponent<TextMeshProUGUI>();
        cTMP.text = "< BACK";
        cTMP.fontSize = 15;
        cTMP.fontStyle = FontStyles.Bold;
        cTMP.color = Color.white;
        cTMP.alignment = TextAlignmentOptions.Center;

        // Coin Balance Badge
        GameObject coinBadge = new GameObject("CoinBadge", typeof(RectTransform), typeof(TextMeshProUGUI));
        coinBadge.transform.SetParent(shopPanel.transform, false);
        RectTransform cbRT = coinBadge.GetComponent<RectTransform>();
        cbRT.anchorMin = new Vector2(1f, 1f);
        cbRT.anchorMax = new Vector2(1f, 1f);
        cbRT.pivot     = new Vector2(1f, 1f);
        cbRT.anchoredPosition = new Vector2(-18f, -20f);
        cbRT.sizeDelta = new Vector2(100f, 38f);

        coinBalanceText = coinBadge.GetComponent<TextMeshProUGUI>();
        coinBalanceText.text = "COINS: 0";
        coinBalanceText.fontSize = 20;
        coinBalanceText.fontStyle = FontStyles.Bold;
        coinBalanceText.color = new Color(1f, 0.85f, 0.15f, 1f);
        coinBalanceText.alignment = TextAlignmentOptions.MidlineRight;

        // Category Tabs Bar (y: -75)
        GameObject tabsGO = new GameObject("CategoryTabs", typeof(RectTransform));
        tabsGO.transform.SetParent(shopPanel.transform, false);
        RectTransform tabsRT = tabsGO.GetComponent<RectTransform>();
        tabsRT.anchorMin = new Vector2(0.5f, 1f);
        tabsRT.anchorMax = new Vector2(0.5f, 1f);
        tabsRT.pivot     = new Vector2(0.5f, 1f);
        tabsRT.anchoredPosition = new Vector2(0f, -75f);
        tabsRT.sizeDelta = new Vector2(420f, 44f);

        tabHatsButton   = CreateTabButton(tabsGO.transform, "HATS",   new Vector2(-140f, 0f));
        tabSkinsButton  = CreateTabButton(tabsGO.transform, "SKINS",  new Vector2(0f, 0f));
        tabTrailsButton = CreateTabButton(tabsGO.transform, "TRAILS", new Vector2(140f, 0f));

        // Items Container (y: -130 to bottom)
        GameObject scrollGO = new GameObject("ScrollArea", typeof(RectTransform), typeof(RectMask2D), typeof(ScrollRect));
        scrollGO.transform.SetParent(shopPanel.transform, false);
        RectTransform sRT = scrollGO.GetComponent<RectTransform>();
        sRT.anchorMin = new Vector2(0.5f, 1f);
        sRT.anchorMax = new Vector2(0.5f, 1f);
        sRT.pivot     = new Vector2(0.5f, 1f);
        sRT.anchoredPosition = new Vector2(0f, -130f);
        sRT.sizeDelta = new Vector2(430f, 400f);

        ScrollRect sr = scrollGO.GetComponent<ScrollRect>();
        sr.horizontal = false;
        sr.vertical = true;

        GameObject contentGO = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentGO.transform.SetParent(scrollGO.transform, false);
        RectTransform contentRT = contentGO.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 1f);
        contentRT.anchorMax = new Vector2(1f, 1f);
        contentRT.pivot     = new Vector2(0.5f, 1f);
        contentRT.sizeDelta = new Vector2(0f, 0f);

        VerticalLayoutGroup vlg = contentGO.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 8f;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = false;
        vlg.childControlHeight = false;

        ContentSizeFitter csf = contentGO.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        sr.content = contentRT;
        itemsContainer = contentGO.transform;
    }

    Button CreateTabButton(Transform parent, string label, Vector2 pos)
    {
        GameObject btnGO = new GameObject("Tab_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(parent, false);
        RectTransform rt = btnGO.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(130f, 40f);

        Image img = btnGO.GetComponent<Image>();
        img.color = new Color(0.25f, 0.3f, 0.42f, 0.9f);
        if (pillButtonSprite != null)
        {
            img.sprite = pillButtonSprite;
            img.type   = Image.Type.Sliced;
        }

        Button btn = btnGO.GetComponent<Button>();

        GameObject tGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        tGO.transform.SetParent(btnGO.transform, false);
        RectTransform tRT = tGO.GetComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero;
        tRT.anchorMax = Vector2.one;
        tRT.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = tGO.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 16;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        return btn;
    }
}
