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
    private float spawnDistance = 1f;

    [Header("Cooldowns (seconds)")]
    [SerializeField]
    private float attackCooldown = 0.2f;
    [SerializeField]
    private float finalAttackCooldown = 0.5f;


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

        // Spawn prefab in front of the player
        Vector2 spawnPos = (Vector2)transform.position + Vector2.right * spawnDistance;

        

        Instantiate(attackPrefab, spawnPos, Quaternion.Euler(0f, 0f, 90f));

    }

}
