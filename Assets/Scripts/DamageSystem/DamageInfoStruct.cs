using UnityEngine;

public struct DamageInfo
{
    public float damage;
    public Vector2 knockback;
    public string attackName;

    public DamageInfo(float damage, Vector2 knockback, string attackName)
    {
        this.damage = damage;
        this.knockback = knockback;
        this.attackName = attackName;
    }
}
