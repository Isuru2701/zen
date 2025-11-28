using UnityEngine;

public struct DamageInfo
{
    public float damage;
    public Vector2 knockback;

    public DamageInfo(float damage, Vector2 knockback)
    {
        this.damage = damage;
        this.knockback = knockback;
    }
}
