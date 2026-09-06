using System;
using System.Collections.Generic;
using UnityEngine;

public enum CosmeticType
{
    Hat,
    Skin,
    Trail
}

[System.Serializable]
public class CosmeticItem
{
    public string       id;
    public string       displayName;
    public CosmeticType type;
    public int          price;
    public Sprite       icon;
    public Color        color = Color.white;

    public bool IsUnlocked()
    {
        if (price == 0 || id.EndsWith("_none") || id == "skin_default")
            return true;
        return PlayerPrefs.GetInt($"Cosmetic_Unlocked_{id}", 0) == 1;
    }

    public void Unlock()
    {
        PlayerPrefs.SetInt($"Cosmetic_Unlocked_{id}", 1);
        PlayerPrefs.Save();
    }

    public bool IsEquipped()
    {
        string current = PlayerPrefs.GetString($"Equipped_{type}", GetDefaultId(type));
        return current == id;
    }

    public static string GetDefaultId(CosmeticType type)
    {
        switch (type)
        {
            case CosmeticType.Hat:   return "hat_none";
            case CosmeticType.Skin:  return "skin_default";
            case CosmeticType.Trail: return "trail_none";
            default:                 return "";
        }
    }
}

/// <summary>
/// Singleton manager for the Bird Customization Shop.
/// Controls inventory catalog, purchase validation, and equip state.
/// </summary>
public class ShopManager : MonoBehaviour
{
    public static ShopManager instance { get; private set; }

    public static event Action<CosmeticType, string> OnCosmeticEquipped;
    public static event Action                       OnShopUpdated;

    [Header("Sprite References (assigned via inspector or BirdCosmetics)")]
    public Sprite hatTrainerSprite;
    public Sprite hatShadesSprite;
    public Sprite hatCrownSprite;
    public Sprite hatTopHatSprite;
    public Sprite pikachuCoinSprite;

    private List<CosmeticItem> catalog = new List<CosmeticItem>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        EnsureExists();
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, mode) => EnsureExists();
    }

    static void EnsureExists()
    {
        if (FindFirstObjectByType<ShopManager>() == null)
        {
            GameObject go = new GameObject("ShopManager");
            go.AddComponent<ShopManager>();
        }
    }

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;

        AutoLoadSprites();
        BuildCatalog();
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    void AutoLoadSprites()
    {
        if (hatTrainerSprite == null) hatTrainerSprite = Resources.Load<Sprite>("Cosmetics/hat_trainer");
        if (hatShadesSprite == null) hatShadesSprite = Resources.Load<Sprite>("Cosmetics/hat_shades");
        if (hatCrownSprite == null) hatCrownSprite = Resources.Load<Sprite>("Cosmetics/hat_crown");
        if (hatTopHatSprite == null) hatTopHatSprite = Resources.Load<Sprite>("Cosmetics/hat_tophat");
        
        // Load UI sprites used by shop if not assigned
        if (pikachuCoinSprite == null) 
        {
            pikachuCoinSprite = Resources.Load<Sprite>("UI/coin") ?? Resources.Load<Sprite>("coin");
#if UNITY_EDITOR
            if (pikachuCoinSprite == null)
                pikachuCoinSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprites/coin.png");
#endif
        }
    }

    void BuildCatalog()
    {
        catalog.Clear();

        // --- HATS & ACCESSORIES ---
        catalog.Add(new CosmeticItem {
            id = "hat_none", displayName = "No Hat", type = CosmeticType.Hat, price = 0
        });
        catalog.Add(new CosmeticItem {
            id = "hat_trainer", displayName = "Trainer Cap", type = CosmeticType.Hat, price = 15, icon = hatTrainerSprite
        });
        catalog.Add(new CosmeticItem {
            id = "hat_shades", displayName = "Thug Shades", type = CosmeticType.Hat, price = 30, icon = hatShadesSprite
        });
        catalog.Add(new CosmeticItem {
            id = "hat_crown", displayName = "Royal Crown", type = CosmeticType.Hat, price = 60, icon = hatCrownSprite
        });
        catalog.Add(new CosmeticItem {
            id = "hat_tophat", displayName = "Top Hat", type = CosmeticType.Hat, price = 100, icon = hatTopHatSprite
        });

        // --- SKINS ---
        catalog.Add(new CosmeticItem {
            id = "skin_default", displayName = "Classic", type = CosmeticType.Skin, price = 0, color = Color.white
        });
        catalog.Add(new CosmeticItem {
            id = "skin_shiny", displayName = "Shiny Gold", type = CosmeticType.Skin, price = 40, color = new Color(1f, 0.88f, 0.35f, 1f)
        });
        catalog.Add(new CosmeticItem {
            id = "skin_shadow", displayName = "Phantom", type = CosmeticType.Skin, price = 80, color = new Color(0.48f, 0.35f, 0.78f, 1f)
        });
        catalog.Add(new CosmeticItem {
            id = "skin_neon", displayName = "Cyber Cyan", type = CosmeticType.Skin, price = 120, color = new Color(0.3f, 0.92f, 1f, 1f)
        });

        // --- TRAILS ---
        catalog.Add(new CosmeticItem {
            id = "trail_none", displayName = "No Trail", type = CosmeticType.Trail, price = 0
        });
        catalog.Add(new CosmeticItem {
            id = "trail_sparkle", displayName = "Gold Glitter", type = CosmeticType.Trail, price = 35, color = new Color(1f, 0.9f, 0.2f, 1f)
        });
        catalog.Add(new CosmeticItem {
            id = "trail_rainbow", displayName = "Rainbow", type = CosmeticType.Trail, price = 70, color = Color.magenta
        });
        catalog.Add(new CosmeticItem {
            id = "trail_flame", displayName = "Phoenix Fire", type = CosmeticType.Trail, price = 110, color = new Color(1f, 0.35f, 0f, 1f)
        });
    }

    public List<CosmeticItem> GetItemsByCategory(CosmeticType type)
    {
        return catalog.FindAll(item => item.type == type);
    }

    public bool Equip(CosmeticItem item)
    {
        if (!item.IsUnlocked()) return false;

        PlayerPrefs.SetString($"Equipped_{item.type}", item.id);
        PlayerPrefs.Save();

        OnCosmeticEquipped?.Invoke(item.type, item.id);
        OnShopUpdated?.Invoke();
        return true;
    }

    public bool Buy(CosmeticItem item)
    {
        if (item.IsUnlocked())
        {
            Equip(item);
            return true;
        }

        if (FlappyGameManager.instance == null) return false;

        if (FlappyGameManager.instance.SpendCoins(item.price))
        {
            item.Unlock();
            Equip(item);
            OnShopUpdated?.Invoke();
            return true;
        }

        return false;
    }

    public string GetEquippedId(CosmeticType type)
    {
        return PlayerPrefs.GetString($"Equipped_{type}", CosmeticItem.GetDefaultId(type));
    }
}
