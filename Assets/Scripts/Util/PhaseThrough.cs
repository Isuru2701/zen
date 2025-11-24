using UnityEngine;

public class PhaseThrough : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private Collider2D colliderHandle;
    void Start()
    {

        GameEvents.OnGameModeChanged += TurnOffCollider;
        colliderHandle = GetComponent<Collider2D>();
    }

    void TurnOffCollider(GameManager.GameMode mode)
    {
        Debug.Log("orchid: " + Abilities.Orchid);
        if(mode == GameManager.GameMode.Clarity && Abilities.Orchid)
            colliderHandle.enabled = false;
        else
            colliderHandle.enabled = true;
    }





    

}
