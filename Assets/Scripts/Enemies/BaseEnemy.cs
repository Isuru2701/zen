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
        Die
    }    

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float stoppingDistance = 2f; // distance to start attack

    [Header("Attack")]
    public float attackDelay = 0.5f; // time to prepare attack
    public float attackForce = 5f;   // force applied during attack
    public float attackDuration = 0.3f;

    [Header("Health")]
    public float maxHealth = 50f;
    private float health;

    [Header("FSM")]
    public State state = State.Idle;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        health = maxHealth;

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }

    private void Update()
    {
        if (state == State.Die) return; // do nothing if dead

        switch (state)
        {
            case State.Idle:
                LookForPlayer();
                break;

            case State.Tracking:
                MoveTowardsPlayer();
                CheckAttackRange();
                break;

            case State.PrepareAttack:
                // Waiting handled by coroutine
                break;

            case State.Attack:
                // Attack movement handled by coroutine
                break;
        }
    }

    #region FSM Methods

    private void LookForPlayer()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance < 5f) // detection range
        {
            state = State.Tracking;
            animator?.SetBool("isWalking", true);
        }
    }

    private void MoveTowardsPlayer()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;

        // Flip sprite if using 2D
        if (direction.x != 0)
            transform.localScale = new Vector3(Mathf.Sign(direction.x), 1, 1);
    }

    private void CheckAttackRange()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= stoppingDistance)
        {
            rb.linearVelocity = Vector2.zero;
            state = State.PrepareAttack;
            animator?.SetTrigger("prepareAttack");
            StartCoroutine(DoAttack());
        }
    }

    private IEnumerator DoAttack()
    {
        // Wait to simulate "prepare attack"
        yield return new WaitForSeconds(attackDelay);

        if (state == State.Die) yield break;

        state = State.Attack;
        animator?.SetTrigger("attack");

        // Apply a quick lunge
        Vector2 dir = (player.position - transform.position).normalized;
        rb.AddForce(dir * attackForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(attackDuration);

        if (state == State.Die) yield break;

        // Back to tracking after attack
        state = State.Tracking;
        animator?.SetBool("isWalking", true);
    }

    #endregion

    #region Health

    public void TakeDamage(float amount)
    {
        if (state == State.Die) return;

        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        state = State.Die;
        rb.linearVelocity = Vector2.zero;
        animator?.SetTrigger("die");
        // Optionally destroy after animation
        Destroy(gameObject, 1f);
    }

    #endregion
}
