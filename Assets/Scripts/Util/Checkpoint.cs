using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint")]
    [Tooltip("Optional friendly name for this checkpoint")]
    public string checkpointName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetCheckpoint(this.transform);
            }
        }
    }
}
