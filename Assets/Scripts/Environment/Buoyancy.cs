using System;
using System.Diagnostics;
using UnityEngine;


public class Buoyancy : MonoBehaviour
{
    public float underWaterDrag = 3;
    public float underWaterAngularDrag = 1;

    public float airDrag = 0;
    public float airAngularDrag = 0.05f;
    public float floatingPower = 15f;

    public float waterHeight = 0f;

    Rigidbody2D m_rigidbody2D;

    bool underwater;


    void Start()
    {
        m_rigidbody2D = GetComponent<Rigidbody2D>();
        airDrag = m_rigidbody2D.linearDamping;
        airAngularDrag = m_rigidbody2D.angularDamping;
    }

    void FixedUpdate()
    {
        float diff = transform.position.y - waterHeight;

        if(diff < 0)
        {
            m_rigidbody2D.AddForceAtPosition(Vector2.up * floatingPower * Math.Abs(diff), transform.position, ForceMode2D.Force);
            if(!underwater)
            {
                underwater = true;
                SwitchState(true);
            }
        }
        else if (underwater)
        {
            underwater = false;
            SwitchState(false);
        }
    }

    void SwitchState(bool isUnderwater)
    {
        if(isUnderwater)
        {
            m_rigidbody2D.linearDamping = underWaterDrag;
            m_rigidbody2D.angularDamping = underWaterAngularDrag;
        }
        else
        {
            m_rigidbody2D.linearDamping= airDrag;
            m_rigidbody2D.angularDamping = airAngularDrag;
        }
    }



}