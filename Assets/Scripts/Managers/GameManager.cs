using System;
using System.Collections;
using System.ComponentModel.Design;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    [System.Serializable]
    public class CheckpointData
    {
        public Vector3 playerPosition;
        public float playerHealth;
        public bool lily;
        public bool orchid;
        public bool talisman;
        public bool key;
        public GameMode gameMode;
    }

    private CheckpointData savedCheckpointData;

    [SerializeField] Transform[] checkpoints;

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


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        clarityAmount = maxClarityAmount;
        timer = new GameTimer(this);

        GameEvents.OnGameModeChanged += StopSlowMotion;

        Items.Initialize();

    }

    void Update()
    {

        if (CurrentGameMode == GameMode.Clarity)
        {
            // reduce clarity over time
            clarityAmount -= decayMultiplier * Time.unscaledDeltaTime;
            clarityAmount = Mathf.Clamp(clarityAmount, 0, maxClarityAmount);

            clarityBar.UpdateBar(clarityAmount, maxClarityAmount);

            // if depleted, auto-exit
            if (clarityAmount <= 0f)
                ToggleGameMode();
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
            // StartTimer();
            TriggerSlowMotion();
        }
        else
        {
            CurrentGameMode = GameMode.Normal;
            // StopTimer();
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


    #region Checkpoints


    public void SetCheckpoint(Checkpoint checkpoint)
    {
        currentCheckpoint = checkpoint;
        SaveCheckpoint(checkpoint);
    }

    public void SaveCheckpoint(Checkpoint checkpoint)
    {
        if (checkpoint == null) return;

        var player = FindObjectOfType<PlayerController>();

        savedCheckpointData = new CheckpointData();
        savedCheckpointData.playerPosition = checkpoint.transform.position;
        savedCheckpointData.playerHealth = player != null ? player.GetHealth() : 0f;
        savedCheckpointData.lily = Items.Lily;
        savedCheckpointData.orchid = Items.Orchid;
        savedCheckpointData.talisman = Items.Talisman;
        savedCheckpointData.key = Items.Key;
        savedCheckpointData.gameMode = CurrentGameMode;
        Debug.Log("Checkpoint saved at " + savedCheckpointData.playerPosition);
    }

    public void RespawnPlayer(PlayerController player)
    {
        if (savedCheckpointData == null)
        {
            Debug.LogWarning("No checkpoint saved; cannot respawn.");
            return;
        }

        // Restore Items state
        Items.Lily = savedCheckpointData.lily;
        Items.Orchid = savedCheckpointData.orchid;
        Items.Talisman = savedCheckpointData.talisman;
        Items.Key = savedCheckpointData.key;

        // Reset cooldowns to avoid odd timers after respawn
        CooldownManager.ClearAll();

        // Ensure normal game mode and time scale
        CurrentGameMode = GameMode.Normal;
        returnToNormalTime();

        // Find player if not provided
        if (player == null)
            player = FindObjectOfType<PlayerController>();

        if (player != null)
        {
            player.RestoreFromCheckpoint(savedCheckpointData);
        }

        // Notify listeners that mode changed (ensures UI updates)
        GameEvents.OnGameModeChanged?.Invoke(CurrentGameMode);

        Debug.Log("Player respawned to checkpoint.");
    }


    #endregion



}