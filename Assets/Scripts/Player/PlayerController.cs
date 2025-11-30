using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
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
        Jumping,
        Lily,
        Knockback
    }

    #region Fields

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

    [Header("Dodge")]
    [SerializeField] float dodgeForce = 20f;
    private float maxDodges = 2;
    private float dodgeTimes = 0;

    [Header("Dropdown")]
    [SerializeField] private LayerMask platformLayer;
    [SerializeField] private float dropDuration = 0.25f;



    [Header("Ability Related")]
    [SerializeField] private float lilyForce = 25f;
    [SerializeField] private Sprite indicator;
    [SerializeField] private float lilyVerticalScale = 0.75f;

    // --- LILY SETTINGS ---
    [SerializeField] private float lilyDistance = 8f;
    [SerializeField] private float lilyDuration = 0.25f;




    [Header("health")]
    [SerializeField] private float maxhHealth = 100f;
    [SerializeField] private float recoveryRate = 1f;
    private float health;

    private float horizontal;
    private float spriteDirection = -1;

    private State state = State.Normal;

    private Animator animator;
    private SpriteRenderer sprite;

    private float _fallSpeedYDampingChangeThreshold;

    [Header("Cooldowns")]
    [SerializeField] private float dodgeCooldown;
    [SerializeField] private float dodgeResetTime;
    [SerializeField] private float lilyCooldown;
    [SerializeField] private float immunityCooldown;
    [SerializeField] private float regenerateCooldown;




    [Header("UI Handles")]
    [SerializeField] private TMP_Text lilyDisplay;
    [SerializeField] private TMP_Text dodgeDisplay;
    [SerializeField] private UIValueBar healthBar;


    private DamageReceiver damageReceiver;

    private Knockback knockback;

    #endregion



    private void Start()
    {
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        damageReceiver = GetComponent<DamageReceiver>();
        knockback = GetComponent<Knockback>();

        damageReceiver.onHurt += OnDamageReceived;

        _fallSpeedYDampingChangeThreshold = CameraManager.instance._fallSpeedDampingChangeThreshold;

        GameEvents.OnGameModeChanged += SpawnIndicatorSprite;

        knockback.knockbackAction += handleKnockback;

        resetLily();

        health = maxhHealth;
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



        if (CooldownManager.Ready("lily"))
        {
            resetLily();
        }

        if (CooldownManager.Ready("dodgeResetTime"))
        {
            dodgeTimes = 0;
            UpdateTextDisplay(dodgeDisplay, "Dodges remaining: " + (maxDodges - dodgeTimes));
        }

        if (CooldownManager.Ready("dodge"))
        {
            UpdateTextDisplay(dodgeDisplay, "Dodges remaining: " + (maxDodges - dodgeTimes));
        }

        UpdateHealthBar();

        RegenerateHealth();

        #region States

        switch (state)
        {
            // -----------------------
            // NORMAL STATE
            // -----------------------
            case State.Normal:
                Vector2 v = rb.linearVelocity;
                v.x = horizontal * moveSpeed;
                rb.linearVelocity = v;
                // Apply jump gravity logic every frame
                //TODO: is this whats causing issues in LILY?
                ApplyJumpPhysics();

                // Camera fall damping
                HandleCameraDamping();

                break;



            // -----------------------
            // DODGE STATE
            // -----------------------
            case State.Dodging:

                spriteDirection = sprite.flipX ? 1 : -1;
                rb.AddForce(new Vector2(rb.linearVelocityX + dodgeForce * spriteDirection, rb.linearVelocityY), ForceMode2D.Impulse);

                if (CooldownManager.Ready("dodge"))
                {
                    UpdateTextDisplay(dodgeDisplay, "Dodges Remaining: " + (maxDodges - dodgeTimes));
                }

                state = State.Normal;
                break;



            // -----------------------
            // JUMP REQUEST STATE
            // -----------------------
            case State.Jumping:


                rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);
                animator.SetBool("isJumping", true);

                // Consume coyote time
                coyoteCounter = 0;


                state = State.Normal;
                break;

            case State.Knockback:

                break;

        }
    }

    #endregion



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

            if (IsGrounded() || IsOnPlatform() || coyoteCounter > 0f)
                state = State.Jumping;
        }

        // JUMP RELEASED
        if (context.canceled)
        {
            jumpHeld = false;
        }
    }


    //moved to playerAttack
    // public void Attack(InputAction.CallbackContext context)
    // {
    //     if (!context.performed) return;

    //     Mouse mouse = Mouse.current;
    //     if (mouse == null || attackPrefab == null || Camera.main == null) return;

    //     Vector2 mousePosition = mouse.position.ReadValue();

    //     float z = -Camera.main.transform.position.z;
    //     Vector3 worldMouse = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, z));

    //     Vector2 dir = ((Vector2)worldMouse - (Vector2)transform.position).normalized;

    //     Vector2 spawnPos = (Vector2)transform.position + dir * spawnDistance;

    //     float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;

    //     Instantiate(attackPrefab, spawnPos, Quaternion.Euler(0f, 0f, angle));
    //     animator.SetBool("isAttacking", true);
    // }



    public void Dodge(InputAction.CallbackContext context)
    {
        if (!CooldownManager.Ready("dodge")) return;

        if (context.performed)
        {

            state = State.Dodging;
            dodgeTimes += 1;
            animator.SetBool("isDodging", true);


            if (dodgeTimes == maxDodges)
            {
                CooldownManager.Start("dodge", dodgeCooldown);
                UpdateTextDisplay(dodgeDisplay, "Dodge on cooldown");
                dodgeTimes = 0;
            }
            else
            {
                UpdateTextDisplay(dodgeDisplay, "Dodges remaining: " + (maxDodges - dodgeTimes));
            }

        }
        CooldownManager.Start("dodgeResetTime", dodgeResetTime);


    }

    public void DropDown(InputAction.CallbackContext context)
    {
        Collider2D platform = Physics2D.OverlapCapsule(
    groundCheck.position, new Vector2(0.1f, 0.1f),
    CapsuleDirection2D.Horizontal,
    0f,
    platformLayer
    );
        if (platform != null)
        {
            StartCoroutine(DropTillGroundReached(platform));
        }


    }

    private IEnumerator DropTillGroundReached(Collider2D platformCollider)
    {

        platformCollider.enabled = false;

        while (!IsGrounded())
        {

            yield return null;
        }

        platformCollider.enabled = true;


    }




    #endregion



    #region Abilities
    //Lily
    //On Right Click

    private Vector2 lilyDirection;
    public void WaterLily(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (!Abilities.Lily) return;
        if (!CooldownManager.Ready("lily")) return;
        if (GameManager.CurrentGameMode != GameManager.GameMode.Clarity) return;

        // Direction from indicator
        lilyDirection = CalculateLilyDirection();

        // Start dash
        if (lilyRoutine != null) StopCoroutine(lilyRoutine);
        lilyRoutine = StartCoroutine(DoLilyDash(lilyDirection));
    }

    private void resetLily()
    {
        UpdateTextDisplay(lilyDisplay, "Lily available");
    }


    private Coroutine lilyRoutine;

    private IEnumerator DoLilyDash(Vector2 dir)
    {
        state = State.Lily;

        // Lock input for dash duration
        float t = 0f;

        // Precompute the target velocity needed to travel fixed distance
        // dist = speed * time  →  speed = dist / time
        float speed = lilyDistance / lilyDuration;

        // Apply your vertical scale tweak [removed for testing] 
        dir = new Vector2(dir.x, dir.y).normalized;

        // Begin cooldown
        CooldownManager.Start("lily", lilyCooldown);

        UpdateTextDisplay(lilyDisplay, "Lily on cooldown");

        // Player becomes fully velocity-controlled
        while (t < lilyDuration)
        {
            rb.linearVelocity = dir * speed;
            t += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        state = State.Normal;

        lilyRoutine = null;
    }




    #endregion



    #region Helpers


    private bool IsGrounded()
    {

        return Physics2D.OverlapCapsule(groundCheck.position, new Vector2(0.1f, 0.1f), CapsuleDirection2D.Horizontal, 0f, layerMask);
    }

    private bool IsOnPlatform()
    {
        bool v = Physics2D.OverlapCapsule(groundCheck.position, new Vector2(0.1f, 0.1f), CapsuleDirection2D.Horizontal, 0f, platformLayer);
        Debug.Log("platform " + v);
        return v;
    }



    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(groundCheck.position, new Vector3(0.1f, 0.1f, 0.1f));
    }


    //add to onGameModeChanged event
    //basically, spawn a triangle confined to a circle around the player, which points in the 
    //direction of mouse, and thereby the waterlily jump direction

    [Header("Indicator")]
    [SerializeField] private float indicatorRadius = 1.5f;
    private GameObject indicatorInstance;



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
                Vector2 dir = CalculateLilyDirection();

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

    public Vector2 CalculateLilyDirection()
    {
        Mouse mouse = Mouse.current;
        Vector2 mousePos = mouse.position.ReadValue();
        float z = -Camera.main.transform.position.z;

        Vector3 worldMouse = Camera.main.ScreenToWorldPoint(
            new Vector3(mousePos.x, mousePos.y, z)
        );

        // Direction vector
        return ((Vector2)worldMouse - (Vector2)transform.position).normalized;
    }


    private void handleKnockback(bool f)
    {

        if(f)
        {
            Debug.Log("knockedback");
            state = State.Knockback;
        }
        else
        {
            state = State.Normal;
        }

    }

    #endregion


    #region UI Handlers
    private void UpdateTextDisplay(TMP_Text textDisplay, string text)
    {
        textDisplay.text = text;
    }


    private void UpdateHealthBar()
    {
        healthBar.UpdateBar(health, maxhHealth);
    }


    #endregion

    #region Health

    public void OnDamageReceived(DamageInfo info)
    {
        // Respect temporary immunity
        if (!CooldownManager.Ready("immunity")) return;

        // Apply damage
        TakeDamage(info.damage);
        Debug.Log("Force on player: " + info.hitDirection + " " + info.constantForceDirection);

        knockback.CallKnockback(info.hitDirection, info.constantForceDirection, info.knockbackForce);


        // Start regenerate and immunity cooldowns
        CooldownManager.Start("immunity", immunityCooldown);
    }



    private void TakeDamage(float amount)
    {
        if (!IsDead())
            health -= amount;
    }

    private bool IsDead()
    {
        if (health <= 0) return true;
        else return false;
    }

    private void RegenerateHealth()
    {
        if (!CooldownManager.Ready("regenerate")) return;

        if (health != maxhHealth)
            health += recoveryRate * Time.deltaTime;
    }


    #endregion

    #region Collisions




    // private void OnCollisionEnter2D(Collision2D collision)
    // {
    //     Debug.Log("in Collision");

    //     Debug.Log("tag is : " + collision.gameObject.tag.ToString());

    //     if (collision.gameObject.CompareTag("Enemy"))
    //     {
    //         Debug.Log("tag compared");

    //         if (!CooldownManager.Ready("immunity")) return;
    //         Debug.Log("immunity passed");
    //         TakeDamage(10); //TODO: make this serialized or better yet, make a health manager seperately

    //         CooldownManager.Start("regenerate", regenerateCooldown);
    //         CooldownManager.Start("immunity", immunityCooldown);
    //     }
    // }


    // private void OnCollisionEnter(Collision2D other)
    // {
    //     if (other.CompareTag("Enemy"))
    //     {

    //         if (!CooldownManager.Ready("immunity")) return;

    //         TakeDamage(50);

    //         CooldownManager.Start("regenerate", regenerateCooldown);
    //         CooldownManager.Start("immunity", immunityCooldown);
    //     }
    // }

    #endregion
}

