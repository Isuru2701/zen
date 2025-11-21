using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.InputSystem;


public class GameManager : MonoBehaviour
{

    //save a copy of the player data alongside location

    enum GameMode
    {
        Normal,
        Clarity
    }


    [Header("Checkpoints")]
    private GameMode currentGameMode;
    private Checkpoint currentCheckpoint;


    void Start()
    {

    }

    void Update()
    {
        switch (currentGameMode)
        {
            case GameMode.Normal:
                break;

            case GameMode.Clarity:
                break;
        }
    }



    public void EnterGameMode(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("context sent, Clarity mode");
            currentGameMode = GameMode.Clarity;
        }
        else
        {
            Debug.Log("context sent, Normal mode");
            currentGameMode = GameMode.Normal;
        }

    }





}