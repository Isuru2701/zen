using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public enum State
    {
        Idle,
        Tracking,
        PrepareAttack,
        Attack,
        Die,
        Knockback
    }

    [Header("References")]
    [SerializeField] private Transform[] idlePoints;
    [SerializeField] private float idleTime;

    [SerializeField] Weakspot weakSpot;


    [Header("Health")]
    [SerializeField] private float health = 50f;
    [SerializeField] private float damageTakenCooldown = 2f;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float idlePointTolerance = 0.1f;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackRange = 1f;

    [Header("Attack Settings")]
    [SerializeField] private float prepareTime = 1f;
    [SerializeField] private float attackDuration = 0.3f;
    [SerializeField] private GameObject attackPrefab;
    [SerializeField] private float lightAttackDamage = 15f;
    [SerializeField] private float heavyAttackDamage = 15f; //TODO: add later

    private State currentState = State.Idle;
    private int currentIdleIndex = 0;
    private bool isPreparing = false;
    private bool isAttacking = false;

    private Transform player;
    private SpriteRenderer sprite;
    private DamageReceiver damageReceiver;

    private Rigidbody2D rb;

    private Knockback knockback;

    // Stored initial state for respawn
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float initialHealth;
    private State initialState = State.Idle;


    private void Start()
    {

        rb = GetComponent<Rigidbody2D>();
        damageReceiver = GetComponent<DamageReceiver>();

        knockback = GetComponent<Knockback>();
        knockback.knockbackAction += handleKnockback;

        damageReceiver.onHurt += OnDamageReceived;
        GameObject p = GameObject.FindGameObjectWithTag("Player");

        weakSpot.weakFlag += SetTakeDoubleDamage;


        if (p != null)
            player = p.transform;

        sprite = GetComponentInChildren<SpriteRenderer>();

        // Setup attack prefab
        if (attackPrefab != null)
        {
            DamageSender attackSender = attackPrefab.GetComponent<DamageSender>();
            if (attackSender != null)
            {
                attackSender.faction = Faction.Enemy;
                attackSender.damage = lightAttackDamage;
            }
        }

        Debug.Log("PLAYER: " + player);

        // capture initial state for respawn
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialHealth = health;
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.Idle: StateIdle(); break;
            case State.Tracking: StateTracking(); break;
            case State.PrepareAttack: StatePrepareAttack(); break;
            case State.Attack: StateAttack(); break;
            case State.Die: break;
            case State.Knockback: break;
        }
    }

    // --------------------------
    //         STATES
    // --------------------------

    private void StateIdle()
    {
        ;
        if (!CooldownManager.Ready($"enemy{this.GetHashCode()}idletime")) return;
        // move between idle points
        Transform target = idlePoints[currentIdleIndex];

        MoveTowards(target.position);

        // check if reached the point
        if (Vector2.Distance(transform.position, target.position) < idlePointTolerance)
        {
            currentIdleIndex = (currentIdleIndex + 1) % idlePoints.Length;
            CooldownManager.Start("enemyidletime", idleTime);
        }

        // check if player entered detection range
        if (PlayerInRange(detectionRange))
        {
            currentState = State.Tracking;
        }
    }


    private void StateTracking()
    {
        // move toward player
        MoveTowards(player.position);

        // if close enough, start preparing attack
        if (PlayerInRange(attackRange))
        {
            currentState = State.PrepareAttack;
            isPreparing = false;
        }

        // if player leaves detection range, return to idle
        if (!PlayerInRange(detectionRange))
        {
            currentState = State.Idle;
        }
    }


    private void StatePrepareAttack()
    {
        if (!isPreparing)
        {
            StartCoroutine(PrepareRoutine());
        }
    }

    private IEnumerator PrepareRoutine()
    {
        isPreparing = true;
        // stop moving
        yield return new WaitForSeconds(prepareTime);
        currentState = State.Attack;
    }


    private void StateAttack()
    {
        // perform attack logic
        if (!isAttacking)
            StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        // Spawn attack prefab in front of enemy
        if (attackPrefab != null)
        {
            Vector2 spawnPos = (Vector2)transform.position + (Vector2.right * (sprite.flipX ? -1 : 1)) * 0.5f;
            Instantiate(attackPrefab, spawnPos, Quaternion.Euler(0f, 0f, 90f));
        }

        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;

        // After attacking go back to tracking or idle depending on distance
        if (PlayerInRange(detectionRange))
            currentState = State.Tracking;
        else
            currentState = State.Idle;
    }

    // --------------------------
    //     HELPER FUNCTIONS
    // --------------------------

    private bool PlayerInRange(float range)
    {
        return Vector2.Distance(transform.position, player.position) <= range;
    }

    private void MoveTowards(Vector2 target)
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );

        // Flip sprite to face movement direction
        if (target.x > transform.position.x)
            sprite.flipX = false; // Moving right
        else if (target.x < transform.position.x)
            sprite.flipX = true;  // Moving left
    }

    private void OnDamageReceived(DamageInfo info)
    {
        if (!CooldownManager.Ready($"enemy{GetHashCode()}DamgeCooldown")) return;

        // Apply damage first
        float damageAmount = weakflag ? info.damage * 2f : info.damage;
        health -= damageAmount;

        Debug.Log("damage taken " + info + " remaining health: " + health);

        CooldownManager.Start($"enemy{GetHashCode()}DamgeCooldown", damageTakenCooldown);

        // If enemy died from this hit, handle death and skip knockback
        if (health <= 0f)
        {
            Die();
            return;
        }

        // Only perform knockback if the enemy is still active
        if (knockback != null && gameObject.activeInHierarchy)
        {
            knockback.CallKnockback(info.hitDirection, info.constantForceDirection, info.knockbackForce);
        }

    }

    private void handleKnockback(bool f)
    {

        if (f)
        {
            Debug.Log("knockedback");
            currentState = State.Knockback;
        }
        else
        {
            currentState = State.Idle;
        }

    }


    private bool weakflag = false;
    private void SetTakeDoubleDamage(bool f)
    {
        weakflag = f;
    }

    // Optional: call this externally
    public void Die()
    {
        currentState = State.Die;
        Debug.Log("destroyed " + gameObject.name);
        // Deactivate instead of destroying so spawner can respawn/reset this enemy
        Deactivate();
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    // Reset enemy to its initial state and optionally move to a spawn position
    public void Respawn(Vector3? spawnPosition = null)
    {
        // Reactivate object
        gameObject.SetActive(true);

        // Reset transform
        if (spawnPosition.HasValue)
            transform.position = spawnPosition.Value;
        else
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
        }

        // Reset physics
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // Reset state
        health = initialHealth;
        isPreparing = false;
        isAttacking = false;
        currentState = initialState;

        // Reset animator flags if present
        var animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = true;
            animator.SetBool("isDead", false);
            animator.SetBool("isWalking", false);
            animator.SetBool("isJumping", false);
            animator.SetBool("isDodging", false);
        }
    }
}
