using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;



public class PlayerController : MonoBehaviour
{

    enum State
    {
        Normal,
        Dodging
    }

    [Header("Player Component References")]
    [SerializeField] Rigidbody2D rb;

    [Header("Player Settings")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpForce = 2f;
    [SerializeField] float fallMultiplier = 2f;

    [Header("Ground")]
    [SerializeField] LayerMask layerMask;
    [SerializeField] Transform groundCheck;

    [Header("Attack")]
    [SerializeField] GameObject attackPrefab;
    [SerializeField] float spawnDistance = 1f;

    [Header("Dodge")]
    [SerializeField] float dodgeForce = 20f;

    private float horizontal;
    private float direction = -1;

    private State state = State.Normal;

    private Animator animator;
    private SpriteRenderer sprite;

    private void Start()
    {
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case State.Normal:
                rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocityY);
                if (rb.linearVelocityY < 0)
                {
                    rb.linearVelocity += Vector2.up * Physics2D.gravity.y * fallMultiplier * Time.fixedDeltaTime;
                }
                break;

            case State.Dodging:
                direction = horizontal == 0 ? -1 : horizontal;
                rb.linearVelocity = new Vector2(rb.linearVelocityX + dodgeForce * direction, rb.linearVelocityY);

                state = State.Normal;
                break;
        }


    }


    private float lastHorizontal = 1f;
    #region Player Controls
    public void Move(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;

        if (lastHorizontal != horizontal && horizontal != 0) lastHorizontal = horizontal;

        sprite.flipX = lastHorizontal < 0 ? true : false;

        animator.SetBool("isWalking", true);

        if (context.canceled)
            animator.SetBool("isWalking", false);

    }


    public void Jump(InputAction.CallbackContext context)
    {
        // JUMP PRESSED
        if (context.performed)
        {
            if (IsGrounded())
            {
                rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);
                animator.SetBool("isJumping", true);
            }
        }

        // JUMP RELEASED
        if (context.canceled)
        {
            // Only cut jump if going upward
            if (rb.linearVelocityY > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocityX, rb.linearVelocityY * 0.3f);
            }
        }
    }


    public void Attack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // check mouse
            Mouse mouse = Mouse.current;
            if (mouse == null || attackPrefab == null || Camera.main == null) return;

            Vector2 mousePosition = mouse.position.ReadValue();

            // convert screen pos to world pos (use camera distance to z=0 plane)
            float z = -Camera.main.transform.position.z;
            Vector3 worldMouse = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, z));

            // direction from player to mouse
            Vector2 dir = ((Vector2)worldMouse - (Vector2)transform.position).normalized;

            // spawn position a short distance from the player towards the mouse
            Vector2 spawnPos = (Vector2)transform.position + dir * spawnDistance;

            // instantiate the sprite prefab and rotate to face the direction (optional)
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;
            Instantiate(attackPrefab, spawnPos, Quaternion.Euler(0f, 0f, angle));
            animator.SetBool("isAttacking", true);
            Debug.Log("Attack towards " + mousePosition + " -> world " + worldMouse);
        }
    }

    public void Dodge(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            state = State.Dodging;
            animator.SetBool("isDodging", true);
        }
    }





    #endregion

    #region Helper Functions
    private bool IsGrounded()
    {

        return Physics2D.OverlapCapsule(groundCheck.position, new Vector2(0.1f, 0.1f), CapsuleDirection2D.Horizontal, 0f, layerMask);

    }



    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(groundCheck.position, new Vector3(0.1f, 0.1f, 0.1f));
    }

    #endregion
}