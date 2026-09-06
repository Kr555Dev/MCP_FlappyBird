using UnityEngine;

/// <summary>
/// Handles all visual cosmetic attachments for the bird:
/// - Headwear (Hats, Sunglasses, Crowns)
/// - Plumage Skins (Shiny Gold, Shadow Phantom, Cyber Cyan)
/// - Flight Trails (Sparkle, Rainbow, Phoenix Fire)
/// 
/// Attach to both the active player bird and the shop preview bird.
/// </summary>
public class BirdCosmetics : MonoBehaviour
{
    [Header("Sprite References (optional, assign in Inspector)")]
    public Sprite hatTrainerSprite;
    public Sprite hatShadesSprite;
    public Sprite hatCrownSprite;
    public Sprite hatTopHatSprite;

    [Header("Cosmetic Offsets")]
    public Vector3 hatOffset = new Vector3(0.18f, 0.58f, 0f);
    public Vector3 hatScale  = new Vector3(0.65f, 0.65f, 1f);

    private SpriteRenderer birdRenderer;
    private GameObject     hatObject;
    private SpriteRenderer hatRenderer;
    private TrailRenderer  trailRenderer;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        EnsureExists();
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, mode) => EnsureExists();
    }

    static void EnsureExists()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && player.GetComponent<BirdCosmetics>() == null)
        {
            player.AddComponent<BirdCosmetics>();
        }
    }

    void Awake()
    {
        AutoLoadSprites();
        birdRenderer = GetComponent<SpriteRenderer>();

        // Create hat attachment child
        Transform existingHat = transform.Find("HatAttachment");
        if (existingHat != null)
        {
            hatObject = existingHat.gameObject;
            hatRenderer = hatObject.GetComponent<SpriteRenderer>();
        }
        else
        {
            hatObject = new GameObject("HatAttachment");
            hatObject.transform.SetParent(transform, false);
            hatObject.transform.localPosition = hatOffset;
            hatObject.transform.localScale = hatScale;

            hatRenderer = hatObject.AddComponent<SpriteRenderer>();
            hatRenderer.sortingOrder = (birdRenderer != null) ? birdRenderer.sortingOrder + 2 : 15;
            hatRenderer.material = (birdRenderer != null) ? birdRenderer.material : new Material(Shader.Find("Sprites/Default"));
        }

        // Create flight trail attachment child
        Transform existingTrail = transform.Find("FlightTrail");
        if (existingTrail != null)
        {
            trailRenderer = existingTrail.GetComponent<TrailRenderer>();
        }
        else
        {
            GameObject trailGO = new GameObject("FlightTrail");
            trailGO.transform.SetParent(transform, false);
            trailGO.transform.localPosition = new Vector3(-0.3f, 0f, 0f);

            trailRenderer = trailGO.AddComponent<TrailRenderer>();
            trailRenderer.time = 0.35f;
            trailRenderer.startWidth = 0.35f;
            trailRenderer.endWidth = 0.02f;
            trailRenderer.sortingOrder = (birdRenderer != null) ? birdRenderer.sortingOrder - 1 : 9;
            trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
            trailRenderer.emitting = true;
        }

        hatObject.SetActive(false);
        trailRenderer.enabled = false;
    }

    void OnEnable()
    {
        ShopManager.OnCosmeticEquipped += HandleCosmeticEquipped;
    }

    void OnDisable()
    {
        ShopManager.OnCosmeticEquipped -= HandleCosmeticEquipped;
    }

    void Start()
    {
        ApplyAllEquipped();
    }

    public void ApplyAllEquipped()
    {
        string hatId   = PlayerPrefs.GetString("Equipped_Hat",   "hat_none");
        string skinId  = PlayerPrefs.GetString("Equipped_Skin",  "skin_default");
        string trailId = PlayerPrefs.GetString("Equipped_Trail", "trail_none");

        ApplyHat(hatId);
        ApplySkin(skinId);
        ApplyTrail(trailId);
    }

    public void ApplyHat(string hatId)
    {
        if (string.IsNullOrEmpty(hatId) || hatId == "hat_none")
        {
            if (hatObject != null) hatObject.SetActive(false);
            return;
        }

        Sprite spriteToUse = null;
        Vector3 offset = hatOffset;
        Vector3 scale = hatScale;

        switch (hatId)
        {
            case "hat_trainer":
                spriteToUse = hatTrainerSprite;
                offset = new Vector3(0.18f, 0.62f, 0f);
                scale = new Vector3(0.68f, 0.68f, 1f);
                break;
            case "hat_shades":
                spriteToUse = hatShadesSprite;
                offset = new Vector3(0.28f, 0.40f, 0f); // across eyes
                scale = new Vector3(0.55f, 0.55f, 1f);
                break;
            case "hat_crown":
                spriteToUse = hatCrownSprite;
                offset = new Vector3(0.15f, 0.70f, 0f);
                scale = new Vector3(0.65f, 0.65f, 1f);
                break;
            case "hat_tophat":
                spriteToUse = hatTopHatSprite;
                offset = new Vector3(0.15f, 0.72f, 0f);
                scale = new Vector3(0.70f, 0.70f, 1f);
                break;
        }

        if (spriteToUse != null && hatRenderer != null)
        {
            hatRenderer.sprite = spriteToUse;
            hatObject.transform.localPosition = offset;
            hatObject.transform.localScale = scale;
            hatObject.SetActive(true);
        }
        else
        {
            if (hatObject != null) hatObject.SetActive(false);
        }
    }

    public void ApplySkin(string skinId)
    {
        if (birdRenderer == null) return;

        switch (skinId)
        {
            case "skin_shiny":
                // Golden Shiny Pidgeotto
                birdRenderer.color = new Color(1f, 0.88f, 0.35f, 1f);
                break;
            case "skin_shadow":
                // Dark Violet Phantom
                birdRenderer.color = new Color(0.48f, 0.35f, 0.78f, 0.95f);
                break;
            case "skin_neon":
                // Cyber Cyan
                birdRenderer.color = new Color(0.3f, 0.92f, 1f, 1f);
                break;
            case "skin_default":
            default:
                birdRenderer.color = Color.white;
                break;
        }
    }

    public void ApplyTrail(string trailId)
    {
        if (trailRenderer == null) return;

        if (string.IsNullOrEmpty(trailId) || trailId == "trail_none")
        {
            trailRenderer.enabled = false;
            return;
        }

        trailRenderer.enabled = true;
        Gradient gradient = new Gradient();

        switch (trailId)
        {
            case "trail_sparkle":
                // Golden sparkles
                gradient.SetKeys(
                    new GradientColorKey[] {
                        new GradientColorKey(new Color(1f, 0.9f, 0.2f), 0f),
                        new GradientColorKey(new Color(1f, 0.6f, 0f), 1f)
                    },
                    new GradientAlphaKey[] {
                        new GradientAlphaKey(0.85f, 0f),
                        new GradientAlphaKey(0f, 1f)
                    }
                );
                break;

            case "trail_rainbow":
                // Rainbow streamer
                gradient.SetKeys(
                    new GradientColorKey[] {
                        new GradientColorKey(Color.red, 0.0f),
                        new GradientColorKey(Color.yellow, 0.25f),
                        new GradientColorKey(Color.green, 0.5f),
                        new GradientColorKey(Color.cyan, 0.75f),
                        new GradientColorKey(Color.magenta, 1.0f)
                    },
                    new GradientAlphaKey[] {
                        new GradientAlphaKey(0.9f, 0f),
                        new GradientAlphaKey(0.1f, 1f)
                    }
                );
                break;

            case "trail_flame":
                // Fire flame trail
                gradient.SetKeys(
                    new GradientColorKey[] {
                        new GradientColorKey(new Color(1f, 1f, 0.4f), 0f),
                        new GradientColorKey(new Color(1f, 0.35f, 0f), 0.5f),
                        new GradientColorKey(new Color(0.8f, 0.05f, 0f), 1f)
                    },
                    new GradientAlphaKey[] {
                        new GradientAlphaKey(0.95f, 0f),
                        new GradientAlphaKey(0f, 1f)
                    }
                );
                break;

            default:
                trailRenderer.enabled = false;
                return;
        }

        trailRenderer.colorGradient = gradient;
    }

    void HandleCosmeticEquipped(CosmeticType type, string id)
    {
        switch (type)
        {
            case CosmeticType.Hat:   ApplyHat(id);   break;
            case CosmeticType.Skin:  ApplySkin(id);  break;
            case CosmeticType.Trail: ApplyTrail(id); break;
        }
    }

    void AutoLoadSprites()
    {
        if (hatTrainerSprite == null || hatShadesSprite == null || hatCrownSprite == null || hatTopHatSprite == null)
        {
            foreach (var s in Resources.FindObjectsOfTypeAll<Sprite>())
            {
                if (s.name == "hat_trainer") hatTrainerSprite = s;
                else if (s.name == "hat_shades") hatShadesSprite = s;
                else if (s.name == "hat_crown") hatCrownSprite = s;
                else if (s.name == "hat_tophat") hatTopHatSprite = s;
            }
        }
    }
}
