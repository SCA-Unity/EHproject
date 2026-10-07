using TwoBitMachines.FlareEngine;
using TwoBitMachines.FlareEngine.ThePlayer;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Puts Health on the Player root so AIFSM AIDamage (trigger overlap)
/// and melee/projectiles can find HP even when they hit a child collider.
/// </summary>
public static class PlayerHealthSetup
{
    public const string VariableName = "PlayerHealth";
    public const float DefaultMax = 10f;
    public const float DefaultRecovery = 0.5f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        Install();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Install();
    }

    public static void Install()
    {
        foreach (Player player in Object.FindObjectsOfType<Player>(true))
        {
            Ensure(player.gameObject);
        }
    }

    public static Health Ensure(GameObject player)
    {
        if (player == null)
        {
            return null;
        }

        Health health = player.GetComponent<Health>();
        if (health == null)
        {
            health = player.AddComponent<Health>();
        }

        health.isHealth = true;
        if (string.IsNullOrEmpty(health.variableName) || health.variableName == "name")
        {
            health.variableName = VariableName;
        }
        health.minValue = 0f;
        if (health.maxValue <= 0f)
        {
            health.maxValue = DefaultMax;
        }
        if (health.currentValue <= health.minValue)
        {
            health.currentValue = health.maxValue;
        }
        if (health.recoveryTime <= 0f)
        {
            health.recoveryTime = DefaultRecovery;
        }

        health.Register();
        return health;
    }
}
