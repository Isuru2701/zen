using System;
using System.Collections;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    //manage Clarity expenditure
    [SerializeField] private float clarityAmount;
    [SerializeField] private float decayMultiplier;



    public enum GameMode
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

    public static GameMode CurrentGameMode { get; set; }
    private Checkpoint currentCheckpoint;

    private GameTimer timer;
    private Clarity clarityRecord;




    void Start()
    {
        timer = new GameTimer(this);
        clarityRecord = new Clarity(clarityAmount, decayMultiplier);

        GameEvents.OnGameModeChanged += StopSlowMotion;

        Abilities.Initialize();

    }

    void Update()
    {
        switch (CurrentGameMode)
        {
            case GameMode.Normal:
                break;

            case GameMode.Clarity:
                break;
        }
    }


    #region Switching Logic
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
        bool switchingToClarity = CurrentGameMode == GameMode.Normal;

        if (switchingToClarity)
        {
            CurrentGameMode = GameMode.Clarity;
            StartTimer();
            TriggerSlowMotion();
        }
        else
        {
            CurrentGameMode = GameMode.Normal;
            StopTimer();
        }

        GameEvents.OnGameModeChanged?.Invoke(CurrentGameMode);
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




    #region Slow Motion

    private Coroutine slowMotionCoroutineHandler;
    public void TriggerSlowMotion()
    {
        slowMotionCoroutineHandler = StartCoroutine(SlowMotionRoutine());
    }

    IEnumerator SlowMotionRoutine()
    {
        // slow down
        Time.timeScale = 0.2f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;   // keep physics stable

        // wait 1 second in REAL TIME
        yield return new WaitForSecondsRealtime(1f);

        returnToNormalTime();

    }

    void returnToNormalTime()
    {
        // return to normal
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

    }

    void StopSlowMotion(GameManager.GameMode mode)
    {
        //pre-emptively stop corountine if player returns back to Normal
        //cuz slow motion might keep playing otherwise even though its 1 second

        if (mode == GameMode.Normal)
        {
            StopCoroutine(slowMotionCoroutineHandler);
            returnToNormalTime();
        }
    }

    #endregion




}