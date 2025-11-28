using System;
using UnityEngine;


public class Weakspot : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Action<bool> weakFlag;



    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "PlayerAttack")
        {
            Debug.Log("triggered double on");
            weakFlag?.Invoke(true);
        }
    }


    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "PlayerAttack")
        {
            Debug.Log("triggered double off");
            weakFlag?.Invoke(false);
        }

    }

}
