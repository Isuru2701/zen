using UnityEngine;

public class PhaseThrough : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private Collider2D collider;
    void Start()
    {

        GameEvents.OnGameModeChanged += TurnOffCollider;
        collider = GetComponent<Collider2D>();
    }

    void TurnOffCollider(GameManager.GameMode mode)
    {
        if(mode == GameManager.GameMode.Clarity)
            collider.enabled = false;
        else
            collider.enabled = true;
    }





    

}
