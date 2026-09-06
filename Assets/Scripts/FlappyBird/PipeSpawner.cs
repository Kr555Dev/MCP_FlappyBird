using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the pipe object pool and spawning.
/// - Pre-warms a pool of N pipes to avoid runtime Instantiate/GC spikes.
/// - Reads spawn rate and gap size from FlappyGameManager each gym level-up.
/// - Adjusts each pipe's gap dynamically on spawn using exact collider geometry.
/// </summary>
public class PipeSpawner : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Singleton (lightweight — only one spawner ever exists)
    // -------------------------------------------------------------------------
    private static PipeSpawner _instance;
    public static PipeSpawner instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<PipeSpawner>();
            }
            return _instance;
        }
        private set => _instance = value;
    }

    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------
    [Header("References")]
    public GameObject pipePrefab;
    public GameObject coinPrefab;

    [Header("Pool")]
    [Tooltip("Pre-spawned pipe count. Should be > max pipes visible at once.")]
    public int poolSize = 8;

    // -------------------------------------------------------------------------
    // Constants — exact collider offsets from child origins to inner edges
    // (Measured from BottomPipe / TopPipe BoxCollider2D bounds and scales)
    // -------------------------------------------------------------------------
    private const float BottomColliderOffset = 3.106f;
    private const float TopColliderOffset    = 2.821f;

    // -------------------------------------------------------------------------
    // Private
    // -------------------------------------------------------------------------
    private Queue<GameObject> pool = new Queue<GameObject>();
    private float             timer;
    private float             currentSpawnRate;

    // =========================================================================
    void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
    }

    void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    void OnEnable()  => FlappyGameManager.OnGymLevelUp += HandleGymLevelUp;
    void OnDisable() => FlappyGameManager.OnGymLevelUp -= HandleGymLevelUp;

    void Start()
    {
        // Pre-warm the pool
        for (int i = 0; i < poolSize; i++)
        {
            GameObject go = Instantiate(pipePrefab);
            go.SetActive(false);
            pool.Enqueue(go);
        }

        currentSpawnRate = FlappyGameManager.instance != null
            ? FlappyGameManager.instance.CurrentSpawnRate
            : 1.7f;

        if (currentSpawnRate <= 0.1f)
            currentSpawnRate = 1.7f;
        
        // Start timer at 0 so first pipe spawns after currentSpawnRate seconds of Playing
        timer = 0f;
    }

    void Update()
    {
        if (FlappyGameManager.instance == null ||
            FlappyGameManager.instance.CurrentState != GameState.Playing) return;

        timer += Time.deltaTime;
        if (timer >= currentSpawnRate)
        {
            timer = 0f;
            SpawnPipe();
        }
    }

    // =========================================================================
    // Spawning
    // =========================================================================
    void SpawnPipe()
    {
        if (pool.Count == 0)
        {
            Debug.LogWarning("[PipeSpawner] Pool exhausted! Consider increasing poolSize.");
            return;
        }

        var   gm      = FlappyGameManager.instance;
        float gapHalf = gm != null ? gm.CurrentGapHalfHeight : 3.5f;
        float speed   = gm != null ? gm.CurrentSpeed         : 4f;
        float spawnX  = transform.position.x;
        // Keep gap centered comfortably within camera viewport (-5 to +5)
        // and ensure both pipe graphics extend completely off-screen
        float randomY = Random.Range(-1.8f, 1.8f);

        GameObject pipe = pool.Dequeue();
        pipe.transform.position = new Vector3(spawnX, randomY, 0f);
        pipe.SetActive(true);

        // Clean up any remaining coins from previous pool usage of this pipe
        for (int i = pipe.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = pipe.transform.GetChild(i);
            if (child.name.StartsWith("CoinPickup"))
                Destroy(child.gameObject);
        }

        // Adjust children to match current gap
        ConfigurePipeGap(pipe, gapHalf);

        // Give the pipe its current speed
        PipeMover mover = pipe.GetComponent<PipeMover>();
        if (mover != null) mover.SetSpeed(speed);

        // Spawn coin directly in the gap center (as a child of the pipe)
        if (coinPrefab != null && gm != null && Random.value < gm.config.coinSpawnChance)
        {
            float coinOffsetY = Random.Range(-gapHalf * 0.4f, gapHalf * 0.4f);
            GameObject coin = Instantiate(coinPrefab, pipe.transform);
            coin.name = "CoinPickup";
            coin.transform.localPosition = new Vector3(0f, coinOffsetY, 0f);
        }
    }

    void ConfigurePipeGap(GameObject pipe, float gapHalf)
    {
        Transform bottomPipe   = pipe.transform.Find("BottomPipe");
        Transform topPipe      = pipe.transform.Find("TopPipe");
        Transform scoreTrigger = pipe.transform.Find("ScoreTrigger");

        if (bottomPipe != null)
            bottomPipe.localPosition = new Vector3(0f, -gapHalf - BottomColliderOffset, 0f);

        if (topPipe != null)
            topPipe.localPosition    = new Vector3(0f, +gapHalf + TopColliderOffset, 0f);

        // Make the score trigger fill exactly the gap
        if (scoreTrigger != null)
        {
            scoreTrigger.localPosition = new Vector3(0.8f, 0f, 0f);
            float rootScaleY = pipe.transform.localScale.y > 0.001f ? pipe.transform.localScale.y : 1f;
            scoreTrigger.localScale    = new Vector3(0.3f, (gapHalf * 2f) / rootScaleY, 1f);
        }
    }

    // =========================================================================
    // Pool Return (called by PipeMover when pipe exits left side)
    // =========================================================================
    public void ReturnToPool(GameObject pipe)
    {
        pipe.SetActive(false);
        pool.Enqueue(pipe);
    }

    // =========================================================================
    // Event Handlers
    // =========================================================================
    void HandleGymLevelUp(int newGymLevel)
    {
        if (FlappyGameManager.instance != null)
            currentSpawnRate = FlappyGameManager.instance.CurrentSpawnRate;

        Debug.Log($"[PipeSpawner] Updated spawnRate to {currentSpawnRate:F2}s for Gym {newGymLevel}");
    }
}
