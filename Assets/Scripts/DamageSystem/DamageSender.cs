using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class DamageSender : MonoBehaviour
{

    public Faction faction;
    public float damage = 10f;
    // knockback.x = directional strength, knockback.y = additional vertical push
    public Vector2 hitDirection = new Vector2(0,0);
    public Vector2 constantForceDirection = new Vector2(0,0);
    public float knockbackForce = 5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        DamageReceiver receiver = other.GetComponent<DamageReceiver>();
        if (!receiver) return;

        // Ignore if same faction
        if (receiver.faction == this.faction) return;

            // Direction from attacker to receiver
        Vector2 dir = ((Vector2)other.transform.position - (Vector2)transform.position).normalized;
        receiver.TakeDamage(new DamageInfo(damage,dir, constantForceDirection, knockbackForce));

        
    }

}
