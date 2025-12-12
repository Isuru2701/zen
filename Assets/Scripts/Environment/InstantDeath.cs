using UnityEngine;

public class InstantDeath : MonoBehaviour
{
    [Tooltip("If true, only objects with the 'Player' tag will be affected.")]
    public bool requirePlayerTag = true;

    void OnTriggerEnter(Collider other) => TryKill(other.gameObject);
    void OnTriggerEnter2D(Collider2D other) => TryKill(other.gameObject);

    void TryKill(GameObject target)
    {
        if (requirePlayerTag && !target.CompareTag("Player")) return;

        target.GetComponent<PlayerController>()?.instantDeath();

        //TODO: add enemy instant death logic here
    }
}