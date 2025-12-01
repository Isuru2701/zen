using System;
using UnityEngine;

public class WaterTriggerHandler : MonoBehaviour
{

    [SerializeField] private LayerMask _waterMask;
    [SerializeField] private GameObject _splashParticles;

    private EdgeCollider2D _edgeColl;

    private Water _water;

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

                // Add buoyancy behaviour so the object gently floats to the surface
                BuoyancyBehaviour existing = collision.gameObject.GetComponent<BuoyancyBehaviour>();
                if (existing == null)
                {
                    BuoyancyBehaviour b = collision.gameObject.AddComponent<BuoyancyBehaviour>();
                    b.Initialize(rb, _water);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // remove buoyancy behaviour when leaving water
        BuoyancyBehaviour b = collision.gameObject.GetComponent<BuoyancyBehaviour>();
        if (b != null)
        {
            Destroy(b);
        }
    }


}

// Helper component that applies a gentle upward buoyancy force while attached.
public class BuoyancyBehaviour : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Water _water;
    private float _originalGravityScale;

    public void Initialize(Rigidbody2D rb, Water water)
    {
        _rb = rb;
        _water = water;
        if (_rb != null)
        {
            _originalGravityScale = _rb.gravityScale;
            // reduce gravity while in water so buoyancy can work smoothly
            _rb.gravityScale = Mathf.Max(0f, _rb.gravityScale * 0.25f);
        }
    }

    private void FixedUpdate()
    {
        if (_rb == null || _water == null)
            return;

        // compute depth below the water surface in local water space
        Vector3 localPos = _water.transform.InverseTransformPoint(_rb.position);
        float halfHeight = _water.Height * 0.5f;
        float depth = halfHeight - localPos.y; // positive if below surface

        if (depth <= 0f)
        {
            // reached surface, small damp to settle and remove component
            // restore gravity and destroy this behaviour
            _rb.gravityScale = _originalGravityScale;
            Destroy(this);
            return;
        }

        // Buoyancy force proportional to depth (and mass) but clamped
        float force = depth * _water.ForceMultiplier * _rb.mass;
        force = Mathf.Clamp(force, 0f, _water.MaxForce * _rb.mass);

        Vector2 up = _water.transform.up;
        _rb.AddForce(up * force, ForceMode2D.Force);

        // gentle damping of vertical velocity to avoid overshoot
        float verticalVel = Vector2.Dot(_rb.linearVelocity, up);
        Vector2 velCorrection = up * (verticalVel * 0.1f);
        _rb.linearVelocity = _rb.linearVelocity - velCorrection;
    }

    private void OnDestroy()
    {
        if (_rb != null)
        {
            _rb.gravityScale = _originalGravityScale;
        }
    }
}
