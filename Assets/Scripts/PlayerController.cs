using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{

    enum State
    {
        Normal,
        Dodging,
        Jumping
    }

    [Header("Player Component References")]
    [SerializeField] Rigidbody2D rb;

    [Header("Player Settings")]
    [SerializeField] float moveSpeed = 5f;

    [Header("Jump Settings")]
    [SerializeField] float jumpForce = 10f;
    [SerializeField] float fallMultiplier = 2f;
    [SerializeField] float lowJumpMultiplier = 3f;    // variable jump
    [SerializeField] float coyoteTime = 0.1f;

    private float coyoteCounter;
    private bool jumpHeld; // for variable jump

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

    private float _fallSpeedYDampingChangeThreshold;



    private void Start()
    {
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();

        _fallSpeedYDampingChangeThreshold = CameraManager.instance._fallSpeedDampingChangeThreshold;
    }



    private void FixedUpdate()
    {
        // -----------------------
        // COYOTE TIME UPDATE
        // -----------------------
        if (IsGrounded())
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;



        switch (state)
        {
            // -----------------------
            // NORMAL STATE
            // -----------------------
            case State.Normal:
                rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocityY);

                // Apply jump gravity logic every frame
                ApplyJumpPhysics();

                // Camera fall damping
                HandleCameraDamping();

                break;



            // -----------------------
            // DODGE STATE
            // -----------------------
            case State.Dodging:
                direction = horizontal == 0 ? -1 : horizontal;
                rb.AddForce(new Vector2(rb.linearVelocityX + dodgeForce * direction, rb.linearVelocityY), ForceMode2D.Impulse);

                state = State.Normal;
                break;



            // -----------------------
            // JUMP REQUEST STATE
            // -----------------------
            case State.Jumping:

                if (IsGrounded() || coyoteCounter > 0f)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);
                    animator.SetBool("isJumping", true);

                    // Consume coyote time
                    coyoteCounter = 0;
                }

                state = State.Normal;
                break;
        }
    }



    // -----------------------
    // VARIABLE JUMP + FAST FALL
    // -----------------------
    void ApplyJumpPhysics()
    {
        float vy = rb.linearVelocityY;

        // Higher gravity when falling
        if (vy < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        // Lower gravity when going up AND player released jump
        else if (vy > 0 && !jumpHeld)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }



    // -----------------------
    // CAMERA FALL HANDLING
    // -----------------------
    private void HandleCameraDamping()
    {
        if (rb.linearVelocityY < _fallSpeedYDampingChangeThreshold &&
            !CameraManager.instance.IsLerpingYDamping &&
            !CameraManager.instance.LerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpYDamping(true);
        }

        if (rb.linearVelocityY >= 0f &&
            !CameraManager.instance.IsLerpingYDamping &&
            CameraManager.instance.LerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpedFromPlayerFalling = false;
            CameraManager.instance.LerpYDamping(false);
        }
    }



    // ---------------------------------------------------------
    // INPUT SYSTEM
    // ---------------------------------------------------------

    private float lastHorizontal = 1f;

    public void Move(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;

        if (lastHorizontal != horizontal && horizontal != 0)
            lastHorizontal = horizontal;

        sprite.flipX = lastHorizontal < 0;

        animator.SetBool("isWalking", horizontal != 0);
    }



    public void Jump(InputAction.CallbackContext context)
    {
        // JUMP PRESSED
        if (context.performed)
        {
            jumpHeld = true;

            if (IsGrounded() || coyoteCounter > 0f)
                state = State.Jumping;
        }

        // JUMP RELEASED
        if (context.canceled)
        {
            jumpHeld = false;
        }
    }



    public void Attack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        Mouse mouse = Mouse.current;
        if (mouse == null || attackPrefab == null || Camera.main == null) return;

        Vector2 mousePosition = mouse.position.ReadValue();

        float z = -Camera.main.transform.position.z;
        Vector3 worldMouse = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, z));

        Vector2 dir = ((Vector2)worldMouse - (Vector2)transform.position).normalized;

        Vector2 spawnPos = (Vector2)transform.position + dir * spawnDistance;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;

        Instantiate(attackPrefab, spawnPos, Quaternion.Euler(0f, 0f, angle));
        animator.SetBool("isAttacking", true);
    }



    public void Dodge(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            state = State.Dodging;
            animator.SetBool("isDodging", true);
        }
    }




    // ---------------------------------------------------------
    // HELPERS
    // ---------------------------------------------------------

    private bool IsGrounded()
    {
        return Physics2D.OverlapCapsule(groundCheck.position, new Vector2(0.1f, 0.1f), CapsuleDirection2D.Horizontal, 0f, layerMask);
    }



    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(groundCheck.position, new Vector3(0.1f, 0.1f, 0.1f));
    }
}
