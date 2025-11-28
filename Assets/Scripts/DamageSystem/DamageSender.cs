using UnityEngine;

public class DamageSender : MonoBehaviour
{

    public Faction faction;
    public float damage = 10f;
    // knockback.x = directional strength, knockback.y = additional vertical push
    public Vector2 knockback = new Vector2(2f, 0f);
    [Tooltip("When true, compute knockback direction from attacker -> receiver using knockback.x as magnitude and knockback.y as extra vertical push.")]
    public bool directionalKnockback = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        DamageReceiver receiver = other.GetComponent<DamageReceiver>();
        if (!receiver) return;

        // Ignore if same faction
        if (receiver.faction == this.faction) return;

        Vector2 appliedKnockback = knockback;
        if (directionalKnockback)
        {
            // Direction from attacker to receiver
            Vector2 dir = ((Vector2)other.transform.position - (Vector2)transform.position).normalized;

            // Use knockback.x as directional magnitude, add knockback.y as extra vertical push
            appliedKnockback = dir * Mathf.Abs(knockback.x);
            appliedKnockback.y += knockback.y;
        }

        receiver.TakeDamage(new DamageInfo(damage, appliedKnockback));
    }

}
