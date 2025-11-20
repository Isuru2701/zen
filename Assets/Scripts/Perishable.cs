using UnityEngine;

public class Perishable : MonoBehaviour
{
    [Header("Time before destroyed")]
    public float lifetime = 5f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

}