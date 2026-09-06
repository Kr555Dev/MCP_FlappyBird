using UnityEngine;

/// <summary>
/// ScriptableObject containing all tunable difficulty values.
/// Create via: Assets → Create → FlappyBird → DifficultyConfig
/// Edit in Inspector to balance the game without touching code.
/// </summary>
[CreateAssetMenu(fileName = "DifficultyConfig", menuName = "FlappyBird/DifficultyConfig")]
public class DifficultyConfig : ScriptableObject
{
    [Header("Pipe Speed (units/sec)")]
    public float baseSpeed          = 4f;
    public float speedIncrement     = 0.5f;
    public float maxSpeed           = 9f;

    [Header("Pipe Spawn Interval (seconds)")]
    public float baseSpawnRate      = 2.0f;
    public float spawnRateDecrement = 0.12f;
    public float minSpawnRate       = 0.75f;

    [Header("Gap Size (half-distance from center to pipe inner edge, world units)")]
    public float baseGapHalfHeight  = 3.5f;
    public float gapHalfDecrement   = 0.25f;
    public float minGapHalf         = 1.8f;

    [Header("Gym Progression")]
    public int pipesPerGym          = 6;

    [Header("Player Lives")]
    public int   startingLives           = 3;
    public float invincibilityDuration   = 1.5f;

    [Header("Coin Collectible")]
    public int   coinScoreValue  = 3;
    [Range(0f, 1f)]
    public float coinSpawnChance = 0.4f;
}
