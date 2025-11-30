using System;
using System.Collections;
using UnityEngine;



public class Knockback : MonoBehaviour
{

    public float knockbackTime = 0.2f;
    public float hitDirectionForce = 10f;
    public float constForce = 5f;

    public Action<bool> knockbackAction;
    

    private Rigidbody2D rb;

    private Coroutine knockbackCoroutine;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }


    public IEnumerator KnockbackAction(Vector2 hitDirection, Vector2 constantForceDirection)
    {

        Debug.Log("knockedback");

        knockbackAction?.Invoke(true);

        Vector2 _hitforce;
        Vector2 _constantForce;
        Vector2 _knockbackForce;

        _hitforce = hitDirection * hitDirectionForce;
        _constantForce = constantForceDirection * constForce;


        float _elapsedTime = 0f;
        while (_elapsedTime < knockbackTime)
        {
            _elapsedTime += Time.fixedDeltaTime;

            _knockbackForce = _hitforce + _constantForce;

            Debug.Log("force: " + _knockbackForce);

            // if (inputDirection != Vector2.zero)
            // {
            //     _combinedForce = _knockbackForce + inputDirection;

            // }
            // else
            // {
            //     _combinedForce = _knockbackForce;
            // }

            //apply to body
            rb.linearVelocity = _knockbackForce;


            yield return new WaitForFixedUpdate();
        }

        knockbackAction?.Invoke(false);
        Debug.Log("knockback routine complete");
    }


    public void CallKnockback(Vector2 hitDirection, Vector2 constantForceDirection)
    {
        knockbackCoroutine = StartCoroutine(KnockbackAction(hitDirection, constantForceDirection));
    }
}
