using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorHandler : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] bool isOpen = false;
    [SerializeField] bool requiresTalisman = false;
    [SerializeField] bool requiresKey = false;


    [Header("Animation")]
    [SerializeField] Sprite closedDoorSprite;
    [SerializeField] Sprite openDoorSprite;

    private BoxCollider2D boxCollider;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();

    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (requiresTalisman)
        {
            if (!Items.Talisman)
                return;
        }

        if (requiresKey)
        {
            if (!Items.Key)
                return;
        }
        OpenDoor();
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        CloseDoor();
    }

    void OpenDoor()
    {
        GetComponent<SpriteRenderer>().sprite = openDoorSprite;
        boxCollider.enabled = false;
        //TODO: add SFX 
    }

    void CloseDoor()
    {

        GetComponent<SpriteRenderer>().sprite = closedDoorSprite;
        boxCollider.enabled = true;
        //TODO: add SFX 
    }



}
