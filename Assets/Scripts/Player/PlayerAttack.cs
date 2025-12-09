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
    [SerializeField]
    private SpriteRenderer playerSprite;
    [SerializeField]
    private float spawnDistance = 1f;

    [Header("Cooldowns (seconds)")]
    [SerializeField]
    private float attackCooldown = 0.2f;
    private float resetTime = 0.2f;
    [SerializeField]
    private float finalAttackCooldown = 0.5f;


    private int counter = 0;

    public void DoAttack(InputAction.CallbackContext context)
    {

        if (context.performed)
        {

            //if player hasnt attacked in a while, reset counter
            if (CooldownManager.Ready("resetTime"))
            {
                counter = 0;
            }
            if (counter < 3)
            {
                AudioManager.Instance.PlaySFX(PlayerSFX.Attack);
                DoLightAttack();
                // CooldownManager.Start("attack", attackCooldown);
                counter++;
            }
            else
            {
                AudioManager.Instance.PlaySFX(PlayerSFX.AttackCombo);
                DoLightAttack();
                counter = 0;
                CooldownManager.Start("finalAttack", finalAttackCooldown);

            }
            CooldownManager.Start("resetTime", resetTime);


        }
    }

    private void DoLightAttack()
    {

        // if (!CooldownManager.Ready("attack")) return;
        if (!CooldownManager.Ready("finalAttack")) return;

        Vector2 direction = new Vector2(playerSprite.flipX? -1: 1, 0);
        // Spawn prefab in front of the player
        Vector2 spawnPos = (Vector2)transform.position + direction  * spawnDistance;

        

        Instantiate(attackPrefab, spawnPos, Quaternion.Euler(0f, 0f, 90f));

    }

}
