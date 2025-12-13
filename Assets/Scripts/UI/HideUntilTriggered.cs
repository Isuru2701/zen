using System;
using UnityEngine;

public class HideUntilTriggered : MonoBehaviour
{

    [SerializeField] GameObject uiObject;

    public Action<bool> isTriggered;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (uiObject != null)
        {
            uiObject.SetActive(false);
        }

        isTriggered += onTriggered;
    }

    void onTriggered(bool active = true)
    {
        if(active)
            uiObject.SetActive(true);
        else
            uiObject.SetActive(false);
    }
    
}
