using System.Collections.Generic;
using UnityEngine;

public class CooldownManager : MonoBehaviour
{
    private static Dictionary<string, float> cooldownTimers = new Dictionary<string, float>();

    /// <summary>
    /// Start (or restart) a cooldown for a specific action.
    /// </summary>
    public static void Start(string key, float duration)
    {
        cooldownTimers[key] = Time.time + duration;
    }

    /// <summary>
    /// Check if cooldown is ready.
    /// </summary>
    public static bool Ready(string key)
    {
        if (!cooldownTimers.ContainsKey(key))
            return true;

        return Time.time >= cooldownTimers[key];
    }

    /// <summary>
    /// Remaining cooldown time (0 if ready).
    /// </summary>
    public static float Remaining(string key)
    {
        if (!cooldownTimers.ContainsKey(key))
            return 0f;

        return Mathf.Max(0f, cooldownTimers[key] - Time.time);
    }

    /// <summary>
    /// Reset/clear a cooldown.
    /// </summary>
    public static void Reset(string key)
    {
        if (cooldownTimers.ContainsKey(key))
            cooldownTimers.Remove(key);
    }

    /// <summary>
    /// Clear all cooldowns if needed (optional).
    /// </summary>
    public static void ClearAll()
    {
        cooldownTimers.Clear();
    }
}
