using UnityEngine;

public class DamageSender : MonoBehaviour
{

    public Faction faction;
    public float damage = 10f;
    public Vector2 knockback = new Vector2(2f, 0f);
    public string attackName = "attack";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<DamageReceiver>(out var receiver))
        {
            // Ignore if same faction
            if (receiver.faction == this.faction)
                return;

            receiver.TakeDamage(new DamageInfo(damage, knockback, attackName));
        }
    }

}
