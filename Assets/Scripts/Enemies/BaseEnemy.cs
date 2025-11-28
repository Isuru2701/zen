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
    [SerializeField]
    private float health;

    [Header("FSM")]
    public State state = State.Idle;


    [Header("Pathing")]
    [SerializeField] Transform[] points;
    [SerializeField] float lerpingSpeed = 0.2f;
    [SerializeField]private float stoppingDistance = 2f; // distance to start attack

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        health = maxHealth;

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }

    private void Update()
    {

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
