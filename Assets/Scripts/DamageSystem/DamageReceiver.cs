using UnityEngine;

public class DamageReceiver : MonoBehaviour
{

    public Faction faction;

    public System.Action<DamageInfo> onHurt;  // For UI, hit flashes, etc.

    public void TakeDamage(DamageInfo info)
    {
        onHurt?.Invoke(info);

    }


//move to component's main script 
    // private void Die()
    // {
    //     // Here's where you tell your FSM:
    //     SendMessage("Die", SendMessageOptions.DontRequireReceiver);
    // }
}
