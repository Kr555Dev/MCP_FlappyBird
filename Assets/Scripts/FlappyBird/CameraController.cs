using UnityEngine;
using System.Collections;

/// <summary>
/// Adds juice to the game by shaking the camera on impacts and coin collections.
/// </summary>
public class CameraController : MonoBehaviour
{
    private Vector3 originalPosition;
    private Coroutine shakeRoutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        EnsureExists();
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, mode) => EnsureExists();
    }

    static void EnsureExists()
    {
        if (Camera.main != null && Camera.main.GetComponent<CameraController>() == null)
        {
            Camera.main.gameObject.AddComponent<CameraController>();
        }
    }

    void Awake()
    {
        originalPosition = transform.localPosition;
    }

    void OnEnable()
    {
        BirdController.OnBirdHit += HandleHeavyShake;
        CoinPickup.OnCoinCollectedStatic += HandleLightShake;
    }

    void OnDisable()
    {
        BirdController.OnBirdHit -= HandleHeavyShake;
        CoinPickup.OnCoinCollectedStatic -= HandleLightShake;
    }

    void HandleHeavyShake()
    {
        Shake(0.4f, 0.5f);
    }

    void HandleLightShake(Vector3 pos)
    {
        Shake(0.1f, 0.15f);
    }

    public void Shake(float duration, float magnitude)
    {
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            transform.localPosition = originalPosition;
        }
        shakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);

            // Use WaitForEndOfFrame to tie shake to rendering rate rather than fixed time
            // Also it will continue to shake during timeScale = 0 if we use unscaledTime
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
        shakeRoutine = null;
    }
}
