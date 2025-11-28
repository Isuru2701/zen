using UnityEngine;

public class DamageReceiver : MonoBehaviour
{

    public Faction faction;
    public float maxHealth = 100f;
    private float currentHealth;

    public System.Action<float> onHurt;  // For UI, hit flashes, etc.

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(DamageInfo info)
    {
        currentHealth -= info.damage;
        onHurt?.Invoke(info.damage);

        // Optional: apply knockback if there is a Rigidbody2D
        if (TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.AddForce(info.knockback, ForceMode2D.Impulse);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Here's where you tell your FSM:
        SendMessage("Die", SendMessageOptions.DontRequireReceiver);
    }
}
