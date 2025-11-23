using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{

    //save a copy of the player data alongside location

    //manage Clarity expenditure
    [SerializeField] private float clarityAmount = 10f;
    [SerializeField] private float decayMultiplier = 1.5f;



    enum GameMode
    {
        Normal,
        Clarity
    }

    //helper class to manage clarity levels
    class Clarity
    {
        public float ClarityAmount { get; set; }
        public float DecayMultiplier { get; set; }

        public Clarity(float c, float d)
        {
            ClarityAmount = c;
            DecayMultiplier = d;
        }

    }


    [Header("Checkpoints")]
    private GameMode currentGameMode;
    private Checkpoint currentCheckpoint;

    private GameTimer timer;
    private Clarity clarityRecord;




    void Start()
    {
        timer = new GameTimer(this);
        clarityRecord = new Clarity(clarityAmount, decayMultiplier);

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


    #region GameMode switching logic
    private bool flag = false;

    public void CheckClarityTrigger(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ToggleGameMode();
        }
    }
    public void ToggleGameMode()
    {


        if (flag == false)
        {
            Debug.Log("context sent, Clarity mode");
            currentGameMode = GameMode.Clarity;
            StartTimer();
            flag = true;
        }
        else
        {
            Debug.Log("context sent, Normal mode");
            currentGameMode = GameMode.Normal;
            StopTimer();
            flag = false;
        }


    }

    #endregion


    #region Clarity Timer
    private void StartTimer()
    {

        //if the player is in this mode for > allowed, force ToggleGameMode
        timer.Start(clarityRecord.ClarityAmount, clarityRecord.DecayMultiplier, ToggleGameMode);

    }

    private void StopTimer()
    {
        timer.Stop();
    }




    #endregion





}