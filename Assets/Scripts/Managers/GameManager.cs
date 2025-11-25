using System;
using System.Collections;
using System.ComponentModel.Design;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{


    [SerializeField] private UIValueBar clarityBar;

    //manage Clarity expenditure
    [SerializeField] private float maxClarityAmount;
    [SerializeField] private float decayMultiplier;
    [SerializeField] private float recoveryRate;
    private float clarityAmount;



    public enum GameMode
    {
        Normal,
        Clarity
    }

    public static GameMode CurrentGameMode { get; set; }
    private Checkpoint currentCheckpoint;

    private GameTimer timer;


    void Start()
    {
        clarityAmount = maxClarityAmount;
        timer = new GameTimer(this);

        GameEvents.OnGameModeChanged += StopSlowMotion;

        Abilities.Initialize();

    }

    void Update()
    {

        if (CurrentGameMode == GameMode.Clarity)
        {
            //add any code here   
            clarityBar.UpdateBar(timer.TimeRemaining, maxClarityAmount);

        }

        //if in normal mode, make sure to increment clarity
        else if (CurrentGameMode == GameMode.Normal)
        {
            if (clarityAmount < maxClarityAmount)
            {
                clarityAmount += recoveryRate * Time.deltaTime;
            }
            clarityBar.UpdateBar(clarityAmount, maxClarityAmount);
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
        timer.Start(clarityAmount, decayMultiplier, ToggleGameMode);

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


    [SerializeField] private float slowTimeScale = 0.2f;
    [SerializeField] private float slowDurationRealTime = 0.25f;

    IEnumerator SlowMotionRoutine()
    {
        // slow down
        Time.timeScale = slowTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;   // keep physics stable

        // wait 1 second in REAL TIME
        yield return new WaitForSecondsRealtime(slowDurationRealTime);

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