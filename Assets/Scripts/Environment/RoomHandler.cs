using Unity.Cinemachine;
using UnityEngine;


public class RoomHandler : MonoBehaviour
{
    [SerializeField] private CinemachineCamera roomCam;

    [SerializeField] private MusicTrack roomMusic;

    //add custom checkpoint logic here if any is present

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            roomCam.Priority = 20;
            CameraManager.instance.SetCamera(roomCam);

            Debug.Log("Camera switched to room camera " +  CameraManager.instance.GetCurrentCamera());
        }


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
        }
    }
}
