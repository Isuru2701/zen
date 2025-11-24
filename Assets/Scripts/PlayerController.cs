using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


#region Abilities Class
/// <summary>
/// Record for Tracking Abilities Obtained
/// </summary>
public static class Abilities

{
    public static bool Lily { get; set; }
    public static bool Orchid { get; set; }

    /// <summary>
    /// Set all ability flags to false
    /// </summary>
    public static void Initialize()
    {
        //TODO: change afterwards
        Lily = true;
        Orchid = true;
    }

}


#endregion

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

    [Header("Ability Related")]
    [SerializeField] private float lilyForce = 25f;
    [SerializeField] private Sprite indicator;

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

        GameEvents.OnGameModeChanged += SpawnIndicatorSprite;
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


    #region Input Handling

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

    #endregion



    #region Abilities



    //Lily
    //On Right Click
    public void WaterLily(InputAction.CallbackContext context)
    {
        
    }
        
    



    #endregion



    #region Helpers


    private bool IsGrounded()
    {
        return Physics2D.OverlapCapsule(groundCheck.position, new Vector2(0.1f, 0.1f), CapsuleDirection2D.Horizontal, 0f, layerMask);
    }



    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(groundCheck.position, new Vector3(0.1f, 0.1f, 0.1f));
    }


    //add to onGameModeChanged event
    //basically, spawn a triangle confined to a circle around the player, which points in the 
    //direction of mouse, and thereby the waterlily jump direction

    private GameObject indicatorObj;
    private Transform indicatorTransform;

    [Header("Indicator")]
    private GameObject indicatorInstance;
    [SerializeField] private float indicatorRadius = 1.5f;


    private void SpawnIndicatorSprite(GameManager.GameMode mode)
    {
        // Remove previous indicator if any
        if (indicatorInstance != null)
            Destroy(indicatorInstance);

        // Only show in Clarity mode + Lily unlocked
        if (mode != GameManager.GameMode.Clarity || !Abilities.Lily)
            return;

        // Instantiate child object to follow player
        indicatorInstance = new GameObject("LilyIndicator");
        var sr = indicatorInstance.AddComponent<SpriteRenderer>();
        sr.sprite = indicator;

        // Small fade or color tint if needed
        sr.color = new Color(1f, 1f, 1f, 0.85f);
        sr.sortingOrder = 50;

        // Parent it to player
        indicatorInstance.transform.SetParent(transform);
        indicatorInstance.transform.localScale = Vector3.one * 0.7f;

        // Start updating it
        StartCoroutine(UpdateIndicator());
    }

    private IEnumerator UpdateIndicator()
    {
        Mouse mouse = Mouse.current;

        while (indicatorInstance != null && GameManager.CurrentGameMode == GameManager.GameMode.Clarity)
        {
            if (mouse != null && Camera.main != null)
            {
                Vector2 mousePos = mouse.position.ReadValue();
                float z = -Camera.main.transform.position.z;

                Vector3 worldMouse = Camera.main.ScreenToWorldPoint(
                    new Vector3(mousePos.x, mousePos.y, z)
                );

                // Direction vector
                Vector2 dir = ((Vector2)worldMouse - (Vector2)transform.position).normalized;

                // Keep indicator at fixed radius
                indicatorInstance.transform.localPosition = dir * indicatorRadius;

                // Rotate indicator (triangle) so the tip faces mouse
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                indicatorInstance.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
            }

            yield return null;
        }

        // Clean up if mode changed suddenly
        if (indicatorInstance != null)
            Destroy(indicatorInstance);
    }



    #endregion

}

