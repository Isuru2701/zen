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

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private SpriteRenderer normalSprite;

    [Header("Movement")]
    [SerializeField]
    private float moveSpeed = 2f;
    [SerializeField]
    private float idleTime = 2f;
    [SerializeField]
    private float detectionRange = 2f;
    

    [Header("Attack")]
    [SerializeField]
    private float attackDelay = 0.5f; // time to prepare attack
    [SerializeField]
    private float attackForce = 5f;   // force applied during attack
    [SerializeField]
    private float attackDuration = 0.3f;

    [Header("Health")]
    [SerializeField]
    private float maxHealth = 50f;

    private DamageReceiver damageReceiver;

    [Header("FSM")]
    public State state = State.Idle;


    [Header("Pathing")]
    [SerializeField] Transform[] points;
    [SerializeField] float lerpingSpeed = 0.2f;
    [SerializeField]private float stoppingDistance = 2f; // distance to start attack

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        damageReceiver = GetComponent<DamageReceiver>();
        damageReceiver.maxHealth = maxHealth;
        damageReceiver.faction = Faction.Enemy;

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }

    private void Update()
    {
        // Handle FSM state
        switch (state)
        {
            case State.Idle:
                HandleIdleState();
                break;
            case State.Tracking:
                HandleTrackingState();
                break;
            case State.PrepareAttack:
                HandlePrepareAttackState();
                break;
            case State.Attack:
                HandleAttackState();
                break;
            case State.Die:
                HandleDieState();
                break;
        }
    }

    private void HandleIdleState()
    {
        // Check if player is in range
        if (CalculateDistance(transform, player) < detectionRange)
        {
            state = State.Tracking;
        }
    }

    private void HandleTrackingState()
    {
        // Move towards player
        Vector2 direction = ((Vector2)player.position - rb.position).normalized;
        rb.linearVelocity = direction * moveSpeed;

        // Check if close enough to attack
        if (CalculateDistance(transform, player) < stoppingDistance)
        {
            state = State.PrepareAttack;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void HandlePrepareAttackState()
    {
        // Wait for attack delay, then attack
        // You can implement a timer here
    }

    private void HandleAttackState()
    {
        // Apply attack force/damage
        // This is handled by your attack prefab/DamageSender
    }

    private void HandleDieState()
    {
        // Play death animation, disable, etc.
        Destroy(gameObject);
    }

    public void Die()
    {
        state = State.Die;
    }

    private void TrackBetweenPoints()
    {

    }

    private IEnumerator MoveTowardsPoint(Transform b)
    {
        if (b == null) yield return null;


        while (CalculateDistance(transform, b) > 0.01f)
        {
            transform.position = Vector2.MoveTowards(transform.position, b.position, lerpingSpeed * Time.deltaTime);
        }



        yield break;
    }



    #region Helpers

    private float CalculateDistance(Transform a, Transform b)
    {
        return Vector2.Distance(a.position, b.position);
    }

    #endregion


}
