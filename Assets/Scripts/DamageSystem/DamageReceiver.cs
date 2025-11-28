using UnityEngine;

public class DamageReceiver : MonoBehaviour
{

    public Faction faction;

    public System.Action<float> onHurt;  // For UI, hit flashes, etc.

    public void TakeDamage(DamageInfo info)
    {
        onHurt?.Invoke(info.damage);

        // Optional: apply knockback if there is a Rigidbody2D
        if (TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.AddForce(info.knockback, ForceMode2D.Impulse);
        }

    }


//move to component's main script 
    // private void Die()
    // {
    //     // Here's where you tell your FSM:
    //     SendMessage("Die", SendMessageOptions.DontRequireReceiver);
    // }
}
