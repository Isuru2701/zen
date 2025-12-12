using Unity.Cinemachine;
using UnityEngine;


public class RoomHandler : MonoBehaviour
{
    [SerializeField] private CinemachineCamera roomCam;

    [SerializeField] private MusicTrack roomMusic;


    [SerializeField] private bool customCheckpoint = false;

    private EnemySpawner spawner;

    private void Awake()
    {
        spawner = GetComponent<EnemySpawner>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        roomCam.Priority = 20;
        CameraManager.instance.SetCamera(roomCam);

        Debug.Log("Camera switched to room camera " + CameraManager.instance.GetCurrentCamera());

        if (roomMusic != MusicTrack.None)
        {
            AudioManager.Instance.PlayMusic(roomMusic);
        }


    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            roomCam.Priority = 10;

            if (customCheckpoint)
            {
                GameManager.Instance.ResetToPreviousCheckpoint();
            }
        }
    }
}
