using UnityEngine;

/// <summary>
/// A collectible Pikachu coin floating in pipe gaps.
/// Moves with the world. On bird contact → awards bonus score and self-destructs.
/// Tag the bird GameObject as "Player" for detection.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class CoinPickup : MonoBehaviour
{
    public static event System.Action<Vector3> OnCoinCollectedStatic;

    [Header("Audio")]
    public AudioClip collectSound;

    private bool collected;

    // =========================================================================
    void Awake()
    {
        // Ensure trigger is set up correctly
        CircleCollider2D c = GetComponent<CircleCollider2D>();
        c.isTrigger = true;
    }

    void Update()
    {
        if (FlappyGameManager.instance == null ||
            FlappyGameManager.instance.CurrentState != GameState.Playing) return;

        // If parented to a pipe, the pipe handles movement
        if (transform.parent != null) return;

        // Move left with the world
        transform.position += Vector3.left * FlappyGameManager.instance.CurrentSpeed * Time.deltaTime;

        // Self-clean if off screen
        if (transform.position.x < -20f)
            Destroy(gameObject);
    }

    // =========================================================================
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"CoinPickup OnTriggerEnter2D called with: {other.gameObject.name}");
        if (collected) return;
        
        // Foolproof check: if it's not the bird, ignore it
        if (other.GetComponent<BirdController>() == null)
        {
            Debug.Log($"Ignoring collision with {other.gameObject.name} because it doesn't have a BirdController");
            return;
        }
        
        Debug.Log("Coin collected by Bird!");
        collected = true;

        OnCoinCollectedStatic?.Invoke(transform.position);

        int val = FlappyGameManager.instance?.config?.coinScoreValue ?? 3;
        FlappyGameManager.instance?.AddBonusScore(val);

        if (collectSound != null)
        {
            GameObject tmp = new GameObject("CoinSFX");
            tmp.transform.position = transform.position;
            AudioSource tmpSrc = tmp.AddComponent<AudioSource>();
            tmpSrc.pitch = Random.Range(0.9f, 1.15f);
            tmpSrc.clip = collectSound;
            tmpSrc.Play();
            Destroy(tmp, collectSound.length + 0.1f);
        }

        Destroy(gameObject);
    }
}
