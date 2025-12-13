using System.Collections;
using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    public enum State
    {
        Idle,
        Tracking,
        PrepareSword,
        SwordAttack,
        PrepareCharge,
        ChargeAttack,
        Knockback,
        Die
    }

    [Header("Health")]
    [SerializeField] private float maxHealth = 200f;
    [SerializeField] private float health;
    [SerializeField] private float damageTakenCooldown = 0.5f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float chargeSpeed = 10f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float swordAttackRange = 2f;
    [SerializeField] private float chargeAttackRange = 8f;
    [SerializeField] private float minChargeDistance = 4f;

    [Header("Sword Attack")]
    [SerializeField] private float swordPrepareTime = 0.5f;
    [SerializeField] private float swordDuration = 0.5f;
    [SerializeField] private float swordDamage = 20f;
    [SerializeField] private GameObject swordAttackPrefab;
    [SerializeField] private float swordCooldown = 2f;

    [Header("Charge Attack")]
    [SerializeField] private float chargePrepareTime = 1f;
    [SerializeField] private float chargeDuration = 1f;
    [SerializeField] private float chargeDamage = 30f;
    [SerializeField] private float chargeCooldown = 5f;

    [Header("Idle Settings")]
    [SerializeField] private float idleDuration = 2f;
    [SerializeField] private float idleChanceAfterAttack = 0.5f;


    [Header("UI")]
    [SerializeField] private UIValueBar healthBar;

    private State currentState = State.Idle;
    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private DamageReceiver damageReceiver;
    private Knockback knockback;

    // Locking during cutscenes: prevent tracking/attacks while allowing idle
    private bool locked = false;

    public void Lock()
    {
        locked = true;
        StopAllCoroutines();
        if (rb != null) rb.linearVelocity = Vector2.zero;
        currentState = State.Idle;
    }

    public void Unlock()
    {
        locked = false;
    }

    private bool isActionInProgress = false;
    private Vector2 chargeDirection;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        damageReceiver = GetComponent<DamageReceiver>();
        knockback = GetComponent<Knockback>();

        damageReceiver.onHurt += OnDamageReceived;

        knockback.knockbackAction += HandleKnockback;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;

        health = maxHealth;
        // Initialize health bar UI and validate reference
        UpdateHealthBar();
    }

    private void Update()
    {
        if (currentState == State.Die) return;

        switch (currentState)
        {
            case State.Idle:
                StateIdle();
                break;
            case State.Tracking:
                StateTracking();
                break;
            case State.PrepareSword:
                // Handled by coroutine
                break;
            case State.SwordAttack:
                // Handled by coroutine
                break;
            case State.PrepareCharge:
                // Handled by coroutine
                break;
            case State.ChargeAttack:
                StateChargeAttack();
                break;
            case State.Knockback:
                // Handled by Knockback component
                break;
        }
    }

    private void UpdateHealthBar()
    {
        Debug.Log($"Health: {health}/{maxHealth}");
        if (healthBar != null)
        {
            healthBar.UpdateBar(health, maxHealth);
        }
        else
        {
            Debug.LogWarning($"BossEnemy: healthBar not assigned on {name}. Assign a UIValueBar in the inspector.");
        }
    }



    private void StateIdle()
    {
        if (locked) return;

        if (CooldownManager.Ready($"boss{GetHashCode()}Idle"))
        {
            currentState = State.Tracking;
        }
    }

    private void StateTracking()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // Move towards player
        MoveTowards(player.position, moveSpeed);

        // Check for attacks
        if (dist <= swordAttackRange && CooldownManager.Ready($"boss{GetHashCode()}Sword"))
        {
            StartCoroutine(SwordAttackRoutine());
        }
        else if (dist <= chargeAttackRange && dist >= minChargeDistance && CooldownManager.Ready($"boss{GetHashCode()}Charge"))
        {
            StartCoroutine(ChargeAttackRoutine());
        }
    }

    private IEnumerator SwordAttackRoutine()
    {
        currentState = State.PrepareSword;
        isActionInProgress = true;

        // Stop moving
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(swordPrepareTime);

        currentState = State.SwordAttack;

        // Spawn attack prefab
        if (swordAttackPrefab != null)
        {
            Vector2 spawnPos = (Vector2)transform.position + (Vector2.right * (sprite.flipX ? -1 : 1)) * 1f;
            GameObject attack = Instantiate(swordAttackPrefab, spawnPos, Quaternion.identity);
            DamageSender sender = attack.GetComponent<DamageSender>();
            if (sender != null)
            {
                sender.faction = Faction.Enemy;
                sender.damage = swordDamage;
            }
        }

        yield return new WaitForSeconds(swordDuration);

        CooldownManager.Start($"boss{GetHashCode()}Sword", swordCooldown);
        DecideNextState();
    }

    private IEnumerator ChargeAttackRoutine()
    {
        currentState = State.PrepareCharge;
        isActionInProgress = true;

        // Stop moving and aim
        rb.linearVelocity = Vector2.zero;
        if (player != null)
        {
            chargeDirection = (player.position - transform.position).normalized;
            // Face the charge direction
            sprite.flipX = chargeDirection.x < 0;
        }

        yield return new WaitForSeconds(chargePrepareTime);

        currentState = State.ChargeAttack;

        // Start charge timer
        CooldownManager.Start($"boss{GetHashCode()}Charging", chargeDuration);

        yield return new WaitForSeconds(chargeDuration);

        // Stop charge
        rb.linearVelocity = Vector2.zero;
        CooldownManager.Start($"boss{GetHashCode()}Charge", chargeCooldown);
        DecideNextState();
    }

    private void StateChargeAttack()
    {
        // Keep moving in the charge direction
        rb.linearVelocity = chargeDirection * chargeSpeed;
    }

    private void DecideNextState()
    {
        isActionInProgress = false;
        if (Random.value < idleChanceAfterAttack)
        {
            currentState = State.Idle;
            CooldownManager.Start($"boss{GetHashCode()}Idle", idleDuration);
        }
        else
        {
            currentState = State.Tracking;
        }
    }

    private void MoveTowards(Vector2 target, float speed)
    {
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);

        // Flip sprite
        if (target.x > transform.position.x)
            sprite.flipX = false;
        else if (target.x < transform.position.x)
            sprite.flipX = true;
    }

    private void OnDamageReceived(DamageInfo info)
    {
        if (!CooldownManager.Ready($"boss{GetHashCode()}Damage")) return;

        if (knockback != null)
            knockback.CallKnockback(info.hitDirection, info.constantForceDirection, info.knockbackForce);

        health -= info.damage;
        CooldownManager.Start($"boss{GetHashCode()}Damage", damageTakenCooldown);
        UpdateHealthBar();

        if (health <= 0)
        {
            Die();
        }
    }

    private void HandleKnockback(bool isKnockedBack)
    {
        if (isKnockedBack)
        {
            currentState = State.Knockback;
            StopAllCoroutines(); // Interrupt attacks
            isActionInProgress = false;
        }
        else
        {
            // Recover from knockback
            currentState = State.Tracking;
        }
    }

    private void Die()
    {
        currentState = State.Die;
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == State.ChargeAttack && collision.gameObject.CompareTag("Player"))
        {
            // Deal damage on collision during charge
            DamageReceiver playerDamage = collision.gameObject.GetComponent<DamageReceiver>();
            if (playerDamage != null)
            {
                DamageInfo info = new DamageInfo
                {
                    damage = chargeDamage,
                    hitDirection = chargeDirection,
                    knockbackForce = 10f, // High knockback on charge
                    constantForceDirection = chargeDirection
                };
                playerDamage.TakeDamage(info);
            }
        }
    }
}
