using System;
using System.Collections;
using UnityEngine;


public class GameTimer
{
    private MonoBehaviour owner;       // Needed to run coroutine
    private float duration;            // Total duration
    private float timeRemaining;       // Time left
    private bool isRunning = false;

    private Action onTimerComplete;    // Callback

    private Coroutine timerCoroutine;

    public float TimeRemaining => timeRemaining;
    public float Duration => duration;
    public bool IsRunning => isRunning;

    public GameTimer(MonoBehaviour owner)
    {
        this.owner = owner;
    }

    public void Start(float duration, float step = 1, Action onComplete = null)
    {
        this.duration = duration;
        this.timeRemaining = duration;
        this.onTimerComplete = onComplete;

        if (timerCoroutine != null)
            owner.StopCoroutine(timerCoroutine);

        timerCoroutine = owner.StartCoroutine(TimerRoutine(step));
        isRunning = true;
    }

    public void Stop()
    {
        if (timerCoroutine != null)
            owner.StopCoroutine(timerCoroutine);

        isRunning = false;
        timeRemaining = 0f;
    }

    private IEnumerator TimerRoutine(float step)
    {
        Debug.Log("TimerRoutine");
        while (timeRemaining > 0f)
        {
            timeRemaining -= Time.deltaTime * step;
            Debug.Log("timeRemaining: " + timeRemaining);
            yield return null;
        }

        isRunning = false;
        onTimerComplete?.Invoke();
    }
}
