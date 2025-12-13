using UnityEngine;
using UnityEngine.InputSystem;

public class RoomSwitcher : MonoBehaviour
{
    [Tooltip("If set, teleport will move this object to the target transform's position.")]
    public Transform targetTransform;

    [Tooltip("Also match rotation of the target transform when teleporting.")]
    public bool matchRotation = false;
    private bool teleportPossible = false;

    public InputActionReference teleportAction;

    private GameObject player;

    public bool automatic = false;


    void OnEnable()
    {
        // Prefer assigned action
        if (teleportAction != null && teleportAction.action != null)
        {
            teleportAction.action.performed += TeleportToTarget;
            teleportAction.action.Enable();
        }
    }

    /// <summary>
    /// Teleport this GameObject to the configured target (transform or position).
    /// </summary>
    public void TeleportToTarget(InputAction.CallbackContext context)
    {
        TeleportToTarget();
    }

    
    public void TeleportToTarget()
    {
        if (!teleportPossible) return;

        if (targetTransform != null && player != null)
        {
            player.transform.position = targetTransform.position;
            if (matchRotation)
                player.transform.rotation = targetTransform.rotation;

        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {

        if (automatic)
        {
            TeleportToTarget(new InputAction.CallbackContext());
        }

        if (collision.CompareTag("Player"))
        {
            teleportPossible = true;
            player = collision.gameObject;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        teleportPossible = false;
        player = null;
    }


}
