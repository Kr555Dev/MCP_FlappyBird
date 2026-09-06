using System.Collections;
using UnityEngine;

/// <summary>
/// Handles all procedural visual polish:
/// 1. Dynamic Parallax Background (gradient sky + scrolling clouds).
/// 2. Particle Effects (Bird Flap puffs, Coin Collect sparkles, Hit impact burst).
/// </summary>
public class VisualEffectsManager : MonoBehaviour
{
    public static VisualEffectsManager instance { get; private set; }

    [Header("Parallax Settings")]
    public float cloudScrollSpeed = 0.5f;

    // Private procedural particle systems
    private ParticleSystem flapParticles;
    private ParticleSystem coinParticles;
    private ParticleSystem hitParticles;

    // Background elements
    private Transform cloudLayer1;
    private Transform cloudLayer2;
    private Transform mountainLayer;
    private SpriteRenderer skyRenderer;
    private ParticleSystem starParticles;
    private Coroutine skyTransitionRoutine;

    // =========================================================================
    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;

        CreateBackgroundLayers();
        CreateParticleSystems();
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        EnsureExists();
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, mode) => EnsureExists();
    }

    static void EnsureExists()
    {
        if (FindFirstObjectByType<VisualEffectsManager>() == null)
        {
            GameObject go = new GameObject("VisualEffectsManager");
            go.AddComponent<VisualEffectsManager>();
        }
    }

    void OnEnable()
    {
        BirdController.OnBirdFlap         += PlayFlapParticles;
        BirdController.OnBirdHit          += PlayHitParticles;
        CoinPickup.OnCoinCollectedStatic  += PlayCoinParticles;
        FlappyGameManager.OnGymLevelUp    += OnGymLevelUp;
    }

    void OnDisable()
    {
        BirdController.OnBirdFlap         -= PlayFlapParticles;
        BirdController.OnBirdHit          -= PlayHitParticles;
        CoinPickup.OnCoinCollectedStatic  -= PlayCoinParticles;
        FlappyGameManager.OnGymLevelUp    -= OnGymLevelUp;
    }

    void Update()
    {
        if (FlappyGameManager.instance == null ||
            FlappyGameManager.instance.CurrentState != GameState.Playing) return;

        // Scroll cloud layers slowly leftward for parallax effect
        if (cloudLayer1 != null)
        {
            cloudLayer1.position += Vector3.left * cloudScrollSpeed * Time.deltaTime;
            if (cloudLayer1.position.x < -20f)
                cloudLayer1.position = new Vector3(20f, cloudLayer1.position.y, cloudLayer1.position.z);
        }

        if (cloudLayer2 != null)
        {
            cloudLayer2.position += Vector3.left * (cloudScrollSpeed * 1.5f) * Time.deltaTime;
            if (cloudLayer2.position.x < -20f)
                cloudLayer2.position = new Vector3(20f, cloudLayer2.position.y, cloudLayer2.position.z);
        }

        if (mountainLayer != null)
        {
            mountainLayer.position += Vector3.left * (cloudScrollSpeed * 2.5f) * Time.deltaTime;
            if (mountainLayer.position.x < -32f)
                mountainLayer.position = new Vector3(0f, mountainLayer.position.y, mountainLayer.position.z);
        }
    }

    // =========================================================================
    // Procedural Background Creation
    // =========================================================================
    void CreateBackgroundLayers()
    {
        // 1. Sky Gradient Background
        GameObject sky = new GameObject("SkyBackground");
        sky.transform.SetParent(transform, false);
        sky.transform.position = new Vector3(0, 0, 10); // Far behind everything
        skyRenderer = sky.AddComponent<SpriteRenderer>();
        
        // Create a 2-pixel texture for vertical gradient (Sky blue to soft cyan)
        Texture2D tex = new Texture2D(1, 2);
        tex.SetPixel(0, 0, new Color(0.4f, 0.7f, 0.95f)); // Bottom (cyan)
        tex.SetPixel(0, 1, new Color(0.15f, 0.35f, 0.75f)); // Top (deep blue)
        tex.Apply();
        
        Sprite skySprite = Sprite.Create(tex, new Rect(0, 0, 1, 2), new Vector2(0.5f, 0.5f), 1f);
        skyRenderer.sprite = skySprite;
        skyRenderer.sortingOrder = -100;
        sky.transform.localScale = new Vector3(40f, 25f, 1f);

        // 2. Parallax Cloud Layer 1
        cloudLayer1 = CreateCloudLayer("CloudLayer_1", -90, 1.2f, new Vector3(0, 2f, 5f));
        cloudLayer2 = CreateCloudLayer("CloudLayer_2", -85, 1.8f, new Vector3(10f, -1f, 5f));

        // 3. Mountain Layer
        mountainLayer = CreateMountainLayer();
    }

    Transform CreateCloudLayer(string name, int order, float scale, Vector3 pos)
    {
        GameObject container = new GameObject(name);
        container.transform.SetParent(transform, false);
        container.transform.position = pos;

        // Make soft cloud shapes using simple circles
        for (int i = 0; i < 5; i++)
        {
            GameObject cloud = new GameObject("Cloud_" + i);
            cloud.transform.SetParent(container.transform, false);
            cloud.transform.localPosition = new Vector3(i * 8f - 16f, Random.Range(-1f, 1f), 0);
            cloud.transform.localScale = Vector3.one * Random.Range(2f, 3.5f) * scale;

            SpriteRenderer sr = cloud.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();
            sr.color = new Color(1f, 1f, 1f, 0.25f); // Soft semi-transparent white
            sr.sortingOrder = order;
        }

        return container.transform;
    }

    Sprite CreateCircleSprite()
    {
        int res = 64;
        Texture2D tex = new Texture2D(res, res);
        float center = res / 2f;
        float radius = res / 2f;

        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float alpha = Mathf.Clamp01(1f - (dist / radius));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha * alpha)); // Soft radial falloff
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), res);
    }

    Transform CreateMountainLayer()
    {
        GameObject container = new GameObject("MountainLayer");
        container.transform.SetParent(transform, false);
        container.transform.position = new Vector3(0, -6f, 8f);

        Color mountainColor = new Color(0.15f, 0.25f, 0.35f, 1f); 
        Sprite mntSprite = CreateMountainSprite(512, 128, mountainColor);

        for (int i = 0; i < 3; i++)
        {
            GameObject mnt = new GameObject("Mountain_" + i);
            mnt.transform.SetParent(container.transform, false);
            mnt.transform.localPosition = new Vector3(i * 32f - 32f, 0, 0); 
            SpriteRenderer sr = mnt.AddComponent<SpriteRenderer>();
            sr.sprite = mntSprite;
            sr.sortingOrder = -80;
        }

        return container.transform;
    }

    Sprite CreateMountainSprite(int width, int height, Color color)
    {
        Texture2D tex = new Texture2D(width, height);
        float seed = Random.Range(0f, 100f);
        Color snowColor = new Color(0.95f, 0.97f, 1f, 1f);

        for (int x = 0; x < width; x++)
        {
            float t = (float)x / (width - 1);
            float noiseStart = Mathf.PerlinNoise(seed, 0f);
            float noiseX = Mathf.PerlinNoise(t * 6f + seed, 0f);
            
            float edgeBlend = Mathf.Sin(t * Mathf.PI);
            float finalNoise = Mathf.Lerp(noiseStart, noiseX, edgeBlend);

            int mHeight = Mathf.FloorToInt(finalNoise * height * 0.7f + height * 0.1f);
            int snowLine = Mathf.FloorToInt(mHeight * 0.72f);

            for (int y = 0; y < height; y++)
            {
                if (y < mHeight)
                {
                    if (y >= snowLine && mHeight > height * 0.25f)
                        tex.SetPixel(x, y, snowColor);
                    else
                        tex.SetPixel(x, y, color);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0f), 16f);
    }

    // =========================================================================
    // Procedural Particle Systems
    // =========================================================================
    void CreateParticleSystems()
    {
        Material defaultParticleMat = new Material(Shader.Find("Sprites/Default"));

        // 1. Flap Particle System
        GameObject flapGO = new GameObject("FlapParticles");
        flapGO.transform.SetParent(transform, false);
        flapParticles = flapGO.AddComponent<ParticleSystem>();
        flapParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = flapParticles.main;
        main.duration = 0.3f;
        main.loop = false;
        main.startLifetime = 0.4f;
        main.startSpeed = 3f;
        main.startSize = 0.2f;
        main.startColor = new Color(1f, 1f, 0.9f, 0.7f);
        main.playOnAwake = false;

        var emission = flapParticles.emission;
        emission.enabled = false; // Trigger manually

        var shape = flapParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;

        // Renderer
        ParticleSystemRenderer renderer = flapGO.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = 20;
        renderer.material = defaultParticleMat;

        // 2. Coin Sparkle Burst System
        GameObject coinGO = new GameObject("CoinParticles");
        coinGO.transform.SetParent(transform, false);
        coinParticles = coinGO.AddComponent<ParticleSystem>();
        coinParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var cMain = coinParticles.main;
        cMain.duration = 0.5f;
        cMain.loop = false;
        cMain.startLifetime = 0.6f;
        cMain.startSpeed = 5f;
        cMain.startSize = 0.35f;
        cMain.startColor = new Color(1f, 0.85f, 0.1f, 1f); // Golden yellow
        cMain.playOnAwake = false;

        var cEmission = coinParticles.emission;
        cEmission.enabled = false;

        var cShape = coinParticles.shape;
        cShape.shapeType = ParticleSystemShapeType.Sphere;
        cShape.radius = 0.2f;

        ParticleSystemRenderer cRenderer = coinGO.GetComponent<ParticleSystemRenderer>();
        cRenderer.sortingOrder = 25;
        cRenderer.material = defaultParticleMat;

        // 3. Hit Explosion System
        GameObject hitGO = new GameObject("HitParticles");
        hitGO.transform.SetParent(transform, false);
        hitParticles = hitGO.AddComponent<ParticleSystem>();
        hitParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var hMain = hitParticles.main;
        hMain.duration = 0.4f;
        hMain.loop = false;
        hMain.startLifetime = 0.5f;
        hMain.startSpeed = 8f;
        hMain.startSize = 0.4f;
        hMain.startColor = new Color(1f, 0.2f, 0.1f, 0.9f); // Red crash burst
        hMain.playOnAwake = false;

        var hEmission = hitParticles.emission;
        hEmission.enabled = false;

        ParticleSystemRenderer hRenderer = hitGO.GetComponent<ParticleSystemRenderer>();
        hRenderer.sortingOrder = 30;
        hRenderer.material = defaultParticleMat;

        CreateStarParticles();
    }

    void CreateStarParticles()
    {
        GameObject starGO = new GameObject("StarParticles");
        starGO.transform.SetParent(transform, false);
        starGO.transform.position = new Vector3(0, 5f, 9f);
        starParticles = starGO.AddComponent<ParticleSystem>();
        starParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        
        var main = starParticles.main;
        main.duration = 10f;
        main.loop = true;
        main.startLifetime = 4f;
        main.startSpeed = 0f; 
        main.startSize = 0.15f;
        main.startColor = new Color(1f, 1f, 1f, 0f); // invisible initially
        main.maxParticles = 100;
        main.prewarm = true;

        var emission = starParticles.emission;
        emission.rateOverTime = 20f;

        var shape = starParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(30f, 10f, 1f);

        var colorOverLifetime = starParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.5f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = grad;

        ParticleSystemRenderer renderer = starGO.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = -95;
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        
        starParticles.Play();
    }

    // =========================================================================
    // Triggers
    // =========================================================================
    public void PlayFlapParticles()
    {
        GameObject bird = GameObject.FindWithTag("Player");
        if (bird != null && flapParticles != null)
        {
            flapParticles.transform.position = bird.transform.position + Vector3.down * 0.3f;
            flapParticles.Emit(8);
        }
    }

    public void PlayCoinParticles(Vector3 pos)
    {
        if (coinParticles != null)
        {
            coinParticles.transform.position = pos;
            coinParticles.Emit(20);
        }
    }

    public void PlayHitParticles()
    {
        GameObject bird = GameObject.FindWithTag("Player");
        if (bird != null && hitParticles != null)
        {
            hitParticles.transform.position = bird.transform.position;
            hitParticles.Emit(25);
        }
    }

    void OnGymLevelUp(int gymLevel)
    {
        if (skyTransitionRoutine != null) StopCoroutine(skyTransitionRoutine);
        skyTransitionRoutine = StartCoroutine(TransitionSkyColorRoutine(gymLevel));
    }

    IEnumerator TransitionSkyColorRoutine(int gymLevel)
    {
        if (skyRenderer == null) yield break;
        
        Texture2D tex = skyRenderer.sprite.texture;
        Color startBottom = tex.GetPixel(0, 0);
        Color startTop = tex.GetPixel(0, 1);
        
        Color targetBottom, targetTop;
        
        if (gymLevel <= 2) // Day
        {
            targetBottom = new Color(0.4f, 0.7f, 0.95f);
            targetTop = new Color(0.15f, 0.35f, 0.75f);
            SetStarsAlpha(0f);
        }
        else if (gymLevel <= 4) // Sunset
        {
            targetBottom = new Color(0.9f, 0.5f, 0.3f);
            targetTop = new Color(0.6f, 0.2f, 0.4f);
            SetStarsAlpha(0.2f);
        }
        else // Night
        {
            targetBottom = new Color(0.1f, 0.05f, 0.2f);
            targetTop = new Color(0.02f, 0.02f, 0.05f);
            SetStarsAlpha(1f);
        }

        float elapsed = 0f;
        float duration = 4f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            tex.SetPixel(0, 0, Color.Lerp(startBottom, targetBottom, t));
            tex.SetPixel(0, 1, Color.Lerp(startTop, targetTop, t));
            tex.Apply();
            yield return null;
        }
    }

    void SetStarsAlpha(float alpha)
    {
        if (starParticles != null)
        {
            var main = starParticles.main;
            main.startColor = new Color(1f, 1f, 1f, alpha);
        }
    }
}
