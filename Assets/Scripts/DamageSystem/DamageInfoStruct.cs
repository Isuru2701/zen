using UnityEngine;

public struct DamageInfo
{
    public float damage;
    public Vector2 hitDirection;
    public Vector2 constantForceDirection;

    public DamageInfo(float damage, Vector2 hitDirection, Vector2 constantForceDirection)
    {
        this.damage = damage;
        this.hitDirection = hitDirection;
        this.constantForceDirection = constantForceDirection;
        
    }
}
