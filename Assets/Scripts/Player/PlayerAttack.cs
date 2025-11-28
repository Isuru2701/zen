using UnityEngine;
using UnityEngine.InputSystem;

public enum Faction
{
    Player,
    Enemy
}

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private GameObject attackPrefab;   // assign your weapon/hitbox object
    private float spawnDistance;

    [Header("Attack Settings")]
    [SerializeField]
    private float lightDamage = 10f;

    [SerializeField]
    private Vector2 lightKnockback = new Vector2(2, 0);


    [Header("Cooldowns (seconds)")]
    [SerializeField]
    private float attackCooldown = 0.2f;
    [SerializeField]
    private float finalAttackCooldown = 0.5f;


    private DamageSender hitbox;

    private void Start()
    {
        // Set player faction
        hitbox = attackPrefab.GetComponent<DamageSender>();

        hitbox.faction = Faction.Player;
    }

    private int counter = 0;

    public void DoAttack(InputAction.CallbackContext context)
    {

        if (context.performed)
        {
            if (counter < 2)
            {
                DoLightAttack();
                CooldownManager.Start("attack", attackCooldown);
                counter++;
            }
            else
            {
                DoLightAttack();
                counter = 0;
                CooldownManager.Start("finalAttack", finalAttackCooldown);

            }


        }
    }


    private void DoLightAttack()
    {

        if (!CooldownManager.Ready("attack")) return;
        if (!CooldownManager.Ready("finalAttack")) return;


        // Set damage and knockback on the hitbox
        hitbox.damage = lightDamage;
        hitbox.knockback = lightKnockback;

        // Spawn prefab in front of the player
        Vector2 spawnPos = (Vector2)transform.position + Vector2.right * spawnDistance;
        float angle = 90f;

        Instantiate(attackPrefab, spawnPos, Quaternion.Euler(0f, 0f, angle));

    }

}
