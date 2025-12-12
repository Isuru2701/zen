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
public static class Items

{
    public static bool Lily { get; set; }
    public static bool Orchid { get; set; }

    public static bool Talisman { get; set; }
    public static bool Key { get; set; }


    /// <summary>
    /// Set all ability flags to false
    /// </summary>
    public static void Initialize()
    {
        Lily = false;
        Orchid = false;

        Talisman = false;
        Key = false;

    }

}


#endregion

public class PlayerController : MonoBehaviour
{

    public enum PlayerState
    {
        Normal,
        Dodging,
        Jumping,
        Lily,
        Knockback,
        Swimming
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

    public PlayerState state = PlayerState.Normal;

    private Animator animator;
    private SpriteRenderer sprite;

    private float _fallSpeedYDampingChangeThreshold;

    [Header("Cooldowns")]
    [SerializeField] private float dodgeCooldown;
    [SerializeField] private float dodgeResetTime;
    [SerializeField] private float lilyCooldown;
    [SerializeField] private float immunityCooldown;
    [SerializeField] private float regenerateCooldown;


    [Header("Buoyancy")]
    [SerializeField] private float underWaterDrag = 3;
    [SerializeField] private float underWaterAngularDrag = 1;
    [SerializeField] private float floatingPower = 15f;
    [SerializeField] private float buoyancyDamping = 5f; // velocity-based damping to reduce bounce
    [SerializeField] private float swimSpeedMultiplier = 0.6f; // horizontal movement multiplier while swimming

    [SerializeField] private float waterHeight = 0f;
    private float airDrag = 0;
    private float airAngularDrag = 0.05f;



    [Header("UI Handles")]
    [SerializeField] private TMP_Text lilyDisplay;
    [SerializeField] private TMP_Text dodgeDisplay;
    [SerializeField] private UIValueBar healthBar;
    
    [Header("Audio")]
    [SerializeField] private float footstepInterval = 0.4f;
    private float _footstepTimer = 0f;
    private bool _wasGrounded = false;
    private PlayerSFX _currentFootstepSurface = PlayerSFX.FootstepsGround;
    private PlayerSFX _lastPlayedFootstepSurface = PlayerSFX.None;


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


        airDrag = rb.linearDamping;
        airAngularDrag = rb.angularDamping;
        _wasGrounded = IsGrounded();
    }

    // Expose health to GameManager when saving checkpoints
    public float GetHealth()
    {
        return health;
    }

    // Restore player state from checkpoint data
    public void RestoreFromCheckpoint(GameManager.CheckpointData data)
    {
        // Restore Items (GameManager should have already set Items)

        // Position & physics
        transform.position = data.playerPosition;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = true;
        }

        // Health
        health = data.playerHealth;

        // Reset state and animator flags
        state = PlayerState.Normal;
        if (animator != null)
        {
            animator.SetBool("isDead", false);
            animator.ResetTrigger("Die");
            animator.SetBool("isJumping", false);
            animator.SetBool("isDodging", false);
            animator.SetBool("isSwimming", false);
            animator.SetBool("isWalking", false);
        }

        // Re-enable controller
        this.enabled = true;

        // Refresh UI
        UpdateHealthBar();
        resetLily();
        UpdateTextDisplay(dodgeDisplay, "Dodges remaining: " + maxDodges);
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
            case PlayerState.Normal:
                Vector2 v = rb.linearVelocity;
                v.x = horizontal * moveSpeed;
                rb.linearVelocity = v;
                // Apply jump gravity logic every frame
                // TODO: is this whats causing issues in LILY?
                ApplyJumpPhysics();

                // Camera fall damping
                HandleCameraDamping();

                break;



            // -----------------------
            // DODGE STATE
            // -----------------------
            case PlayerState.Dodging:

                // Determine facing direction from sprite flip: flipX == true => facing left (-1)
                float facingDir = sprite.flipX ? -1f : 1f;

                // If player is moving (horizontal != 0) dodge in movement direction.
                // Otherwise dodge backwards relative to facing direction.
                float moveDir = Mathf.Abs(horizontal) > 0.01f ? Mathf.Sign(horizontal) : -facingDir;

                rb.AddForce(new Vector2(dodgeForce * moveDir, 0f), ForceMode2D.Impulse);

                if (CooldownManager.Ready("dodge"))
                {
                    UpdateTextDisplay(dodgeDisplay, "Dodges Remaining: " + (maxDodges - dodgeTimes));
                }

                state = PlayerState.Normal;
                break;



            // -----------------------
            // JUMP REQUEST STATE
            // -----------------------
            case PlayerState.Jumping:


                rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);
                animator.SetBool("isJumping", true);

                // Consume coyote time
                coyoteCounter = 0;


                state = PlayerState.Normal;
                break;

            case PlayerState.Knockback:

                break;

            case PlayerState.Swimming:
                // Apply buoyancy forces
                HandleBuoyancy();

                // Allow horizontal movement while swimming (preserve vertical velocity from buoyancy)
                Vector2 swimVel = rb.linearVelocity;
                swimVel.x = horizontal * moveSpeed * swimSpeedMultiplier;
                rb.linearVelocity = swimVel;

                animator.SetBool("isSwimming", horizontal != 0);

                break;

        }

        // Landing detection & footsteps
        bool grounded = IsGrounded();

        // Landing
        if (!_wasGrounded && grounded)
        {
            AudioManager.Instance.PlaySFX(PlayerSFX.Land);
        }

        // Update current surface when grounded
        if (grounded)
        {
            Collider2D col = Physics2D.OverlapPoint(groundCheck.position, layerMask);
            _currentFootstepSurface = DetermineSurfaceFromCollider(col);
        }

        // Footsteps while walking on ground (use surface-specific SFX)
        if (animator != null && animator.GetBool("isWalking") && grounded && !underwater)
        {
            _footstepTimer += Time.fixedDeltaTime;
            if (_footstepTimer >= footstepInterval)
            {
                // Play the surface-specific footstep if available
                var toPlay = _currentFootstepSurface != PlayerSFX.None ? _currentFootstepSurface : PlayerSFX.FootstepsGround;
                AudioManager.Instance.PlaySFX(toPlay);
                _lastPlayedFootstepSurface = toPlay;
                _footstepTimer = 0f;
            }
        }
        else
        {
            _footstepTimer = footstepInterval;
        }

        _wasGrounded = grounded;
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

            // Allow jumping from ground/platform/coyote time or while swimming
            if (IsGrounded() || IsOnPlatform() || coyoteCounter > 0f || state == PlayerState.Swimming)
            {
                // If jumping from swimming, restore air drag so jump behaves normally
                if (state == PlayerState.Swimming)
                {
                    underwater = false;
                    SwitchState(false);
                }

                AudioManager.Instance.PlaySFX(PlayerSFX.Jump);
                state = PlayerState.Jumping;
            }
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
            AudioManager.Instance.PlaySFX(PlayerSFX.Dodge);
            state = PlayerState.Dodging;
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
        if (!Items.Lily) return;
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
        AudioManager.Instance.PlaySFX(PlayerSFX.Jump);
        state = PlayerState.Lily;

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
        state = PlayerState.Normal;

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

    private PlayerSFX DetermineSurfaceFromCollider(Collider2D col)
    {
        if (col == null) return PlayerSFX.FootstepsGround;


        if (col.CompareTag("Wood"))
            return PlayerSFX.FootstepsWood;
        if (col.CompareTag("Stone"))
            return PlayerSFX.FootstepsStone;

        // Default fallback
        return PlayerSFX.FootstepsGround;
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
        {
            if (mode != GameManager.GameMode.Clarity)
                AudioManager.Instance.PlaySFX(PlayerSFX.ClarityExit);
            Destroy(indicatorInstance);
        }

        // Only show in Clarity mode + Lily unlocked
        if (mode != GameManager.GameMode.Clarity || !Items.Lily)
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
        AudioManager.Instance.PlaySFX(PlayerSFX.ClarityEnter);
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

        if (f)
        {
            Debug.Log("knockedback");
            state = PlayerState.Knockback;
        }
        else
        {
            state = PlayerState.Normal;
        }

    }


    bool underwater;

    void HandleBuoyancy()
    {
        float diff = transform.position.y - waterHeight;

        // If player is below the water surface (diff < 0)
        if (diff < 0f)
        {
            // Depth (positive value)
            float depth = -diff;

            // Buoyant force proportional to depth (spring) minus damping proportional to vertical velocity
            float buoyantForce = floatingPower * depth;
            float dampingForce = buoyancyDamping * rb.linearVelocityY;

            float netForce = buoyantForce - dampingForce;

            rb.AddForce(Vector2.up * netForce, ForceMode2D.Force);

            if (!underwater)
            {
                underwater = true;
                SwitchState(true);
                AudioManager.Instance.PlaySFX(EnvironmentSFX.Water);
                // Only switch to swimming if player is in a neutral state
                if (state == PlayerState.Normal)
                    state = PlayerState.Swimming;
            }
        }
        else if (underwater)
        {
            // Left the water: restore state and clear small vertical velocity to avoid re-entry jitter
            underwater = false;
            SwitchState(false);

            // Return to normal movement if we were swimming
            if (state == PlayerState.Swimming)
                state = PlayerState.Normal;

            if (Mathf.Abs(rb.linearVelocityY) < 0.25f)
                rb.linearVelocity = new Vector2(rb.linearVelocityX, 0f);
        }
    }

    void SwitchState(bool isUnderwater)
    {
        if (isUnderwater)
        {
            rb.linearDamping = underWaterDrag;
            rb.angularDamping = underWaterAngularDrag;
        }
        else
        {
            rb.linearDamping = 0;
            rb.angularDamping = 0;
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

        AudioManager.Instance.PlaySFX(PlayerSFX.DamageTaken);
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
        {
            health -= amount;
            if (health <= 0f)
            {
                Die();
            }
        }
    }

    private void Die()
    {
        // Play death SFX and set animator state if available
        AudioManager.Instance.PlaySFX(PlayerSFX.Death);
        if (animator != null)
        {
            animator.SetTrigger("Die");
            animator.SetBool("isDead", true);
        }

        // Ask GameManager to respawn us at the last checkpoint
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RespawnPlayer(this);
        }
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

