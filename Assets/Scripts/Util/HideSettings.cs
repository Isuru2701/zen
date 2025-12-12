using System;
using UnityEngine;

public class HideSettings : MonoBehaviour
{

    [SerializeField]
    [Tooltip("Tick this box only if component should be hidden in normal mode.")]
    private bool hideNormally = false;
    private bool flag = false;


    private void Start()
    {

        GameEvents.OnGameModeChanged += OnGameModeChanged;
        if (hideNormally)
            gameObject.SetActive(false);
    }

    private void OnGameModeChanged(GameManager.GameMode mode)
    {
        if (hideNormally)
        {
            if (mode == GameManager.GameMode.Clarity)
            {
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        else
        {
            if (mode == GameManager.GameMode.Clarity)
            {
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
            }
        }

    }


    [Obsolete]
    private void ToggleVisibility()
    {

        gameObject.SetActive(flag);
        flag = !flag;

    }
}
