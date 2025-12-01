using System;
using UnityEngine;

public class WaterTriggerHandler : MonoBehaviour
{

    [SerializeField] private LayerMask _waterMask;
    [SerializeField] private GameObject _splashParticles;

    private EdgeCollider2D _edgeColl;

    private Water _water;

    private PlayerController player;

    public void Awake()
    {
        _edgeColl = GetComponent<EdgeCollider2D>();
        _water = GetComponent<Water>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("in trigger");
        if ((_waterMask.value & (1 << collision.gameObject.layer)) != 0)
        {
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            Debug.Log("in layer");
            if (rb != null)
            {

                Debug.Log("in rb");
                //particles
                Instantiate(_splashParticles, collision.transform.position, Quaternion.identity);

                int multiplier = 1;
                if (rb.linearVelocityY < 0)
                {
                    multiplier = -1;
                }

                else
                {
                    multiplier = 1;
                }

                float vel = rb.linearVelocityY * _water.ForceMultiplier;
                vel = Mathf.Clamp(Math.Abs(vel), 0f, _water.MaxForce);
                vel *= multiplier;

                _water.Splash(collision, vel);

            }
        }
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("player found");

            player = collision.gameObject.GetComponent<PlayerController>();
            player.state = PlayerController.PlayerState.Swimming;
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(player!= null)
        {
            player.state = PlayerController.PlayerState.Normal;
        }
    }



}

