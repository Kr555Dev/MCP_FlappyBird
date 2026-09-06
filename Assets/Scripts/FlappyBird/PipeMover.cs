using UnityEngine;

/// <summary>
/// Moves a pipe leftward at the GameManager's current speed.
/// When off-screen, returns the GameObject to PipeSpawner's pool
/// instead of destroying it (object pool pattern).
/// </summary>
public class PipeMover : MonoBehaviour
{
    private const float DeadZoneX = -20f;
    private float speed;

    /// <summary>Set by PipeSpawner on pool activation.</summary>
    public void SetSpeed(float newSpeed) => speed = newSpeed;

    void Update()
    {
        if (FlappyGameManager.instance == null ||
            FlappyGameManager.instance.CurrentState != GameState.Playing) return;

        // Always use the live GameManager speed so gym level-ups take immediate effect
        transform.position += Vector3.left * FlappyGameManager.instance.CurrentSpeed * Time.deltaTime;

        if (transform.position.x < DeadZoneX)
            ReturnToPool();
    }

    void ReturnToPool()
    {
        if (PipeSpawner.instance != null)
            PipeSpawner.instance.ReturnToPool(gameObject);
        else
            gameObject.SetActive(false); // fallback
    }
}
