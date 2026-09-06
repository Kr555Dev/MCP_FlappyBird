using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Controls the player bird. Handles input, physics tilt, lives-based death,
/// safe bounds clamping, and the mercy invincibility flash coroutine.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class BirdController : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------
    [Header("Feel")]
    public float baseScale     = 0.5f;
    public float jumpVelocity  = 6.5f;
    public float tiltUpAngle   = 30f;
    public float tiltDownAngle = -80f;
    public float tiltSpeed     = 8f;

    [Header("Screen Bounds (world units)")]
    public float topBound    =  5.2f;
    public float bottomBound = -5.0f;

    [Header("Audio")]
    public AudioClip jumpSound;
    public AudioClip crashSound;

    // Static events for particle triggers
    public static event System.Action OnBirdFlap;
    public static event System.Action OnBirdHit;

    // -------------------------------------------------------------------------
    // Private
    // -------------------------------------------------------------------------
    private Rigidbody2D      rb;
    private CircleCollider2D col;
    private SpriteRenderer   sr;
    private AudioSource      audioSource;
    private bool             isDead;
    private bool             isInvincible;

    // =========================================================================
    void Awake()
    {
        rb          = GetComponent<Rigidbody2D>();
        col         = GetComponent<CircleCollider2D>();
        sr          = GetComponent<SpriteRenderer>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (isDead) return;
        if (FlappyGameManager.instance == null ||
            FlappyGameManager.instance.CurrentState != GameState.Playing) return;

        // --- Input (ignore mouse clicks if over UI buttons) ---
        bool flapPressed = Input.GetKeyDown(KeyCode.Space);
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
                flapPressed = true;
        }

        if (flapPressed)
            Flap();

        // --- Screen bounds handling ---
        if (transform.position.y > topBound)
        {
            // Ceiling clamp: prevent flying off screen top
            transform.position = new Vector3(transform.position.x, topBound, transform.position.z);
            if (rb.linearVelocity.y > 0f)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }
        else if (transform.position.y < bottomBound)
        {
            // Bottom pit: deduct life and recover bird to playable area
            HandleHit(fromPit: true);
        }

        // --- Smooth tilt ---
        float t = Mathf.InverseLerp(-10f, 5f, rb.linearVelocity.y);
        float targetAngle  = Mathf.Lerp(tiltDownAngle, tiltUpAngle, t);
        float currentAngle = transform.eulerAngles.z;
        // Convert to signed angle for smooth lerp
        if (currentAngle > 180f) currentAngle -= 360f;
        float smoothAngle  = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * tiltSpeed);
        transform.rotation = Quaternion.Euler(0f, 0f, smoothAngle);

        // --- Squash and Stretch ---
        float stretch = Mathf.Clamp(1f + rb.linearVelocity.y * 0.03f, 0.7f, 1.3f);
        float squash  = Mathf.Clamp(1f - rb.linearVelocity.y * 0.015f, 0.8f, 1.2f);
        transform.localScale = new Vector3(squash * baseScale, stretch * baseScale, 1f);
    }

    // =========================================================================
    void Flap()
    {
        rb.linearVelocity = Vector2.up * jumpVelocity;
        if (jumpSound != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(jumpSound);
        }
        OnBirdFlap?.Invoke();
    }

    // =========================================================================
    // Collision Detection
    // =========================================================================
    void OnCollisionEnter2D(Collision2D collision2D) => HandleHit(fromPit: false);

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("ScoreZone"))
            FlappyGameManager.instance?.PipePassed();
        else if (other.gameObject.name.Contains("DeadZone"))
            HandleHit(fromPit: true);
        // All other triggers (coins, etc.) are handled by their own scripts — ignore here.
    }

    // =========================================================================
    // Hit & Lives
    // =========================================================================
    void HandleHit(bool fromPit = false)
    {
        if (isDead || isInvincible) return;

        OnBirdHit?.Invoke();
        PlayCrashSound();
        FlappyGameManager.instance?.BirdHitObstacle();

        // Check remaining lives AFTER decrement
        bool hasLivesLeft = FlappyGameManager.instance != null &&
                            FlappyGameManager.instance.Lives > 0;

        if (hasLivesLeft)
        {
            if (fromPit)
            {
                // Recover bird to lower-middle screen with an upward boost
                transform.position = new Vector3(transform.position.x, -2f, transform.position.z);
                rb.linearVelocity = Vector2.up * (jumpVelocity * 0.75f);
            }
            else
            {
                // Give small recoil away from obstacle
                rb.linearVelocity = Vector2.up * (jumpVelocity * 0.5f);
            }

            StartCoroutine(InvincibilityRoutine());
        }
        else
        {
            isDead = true; // GameOver was triggered — freeze the bird
            rb.linearVelocity = Vector2.zero;
        }
    }

    // =========================================================================
    // Invincibility Flash
    // =========================================================================
    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        col.enabled  = false; // disable collider so no more hits land

        float duration  = FlappyGameManager.instance?.config?.invincibilityDuration ?? 1.5f;
        float elapsed   = 0f;
        const float blinkInterval = 0.1f;

        while (elapsed < duration)
        {
            sr.enabled = !sr.enabled; // rapid blink
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        sr.enabled   = true;
        col.enabled  = true;
        isInvincible = false;
    }

    // =========================================================================
    // Audio — uses ignoreListenerPause so it works when timeScale = 0
    // =========================================================================
    void PlayCrashSound()
    {
        if (crashSound == null) return;
        GameObject tmp = new GameObject("CrashSFX");
        AudioSource tmpSrc = tmp.AddComponent<AudioSource>();
        tmpSrc.ignoreListenerPause = true;
        tmpSrc.pitch = Random.Range(0.85f, 1.05f);
        tmpSrc.clip  = crashSound;
        tmpSrc.Play();
        Destroy(tmp, crashSound.length + 0.2f);
    }
}
