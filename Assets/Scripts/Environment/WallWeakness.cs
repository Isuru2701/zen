using UnityEngine;

public class WallWeakness : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private float health = 1f;
    [SerializeField] private DamageReceiver receiver;
    private SpriteRenderer spriteRenderer;

    void Start()
    {

        spriteRenderer = GetComponent<SpriteRenderer>();
        receiver = GetComponent<DamageReceiver>();
        receiver.onHurt += Hit;

    }

    public void Hit(DamageInfo info)
    {

        if (GameManager.CurrentGameMode == GameManager.GameMode.Clarity)        
        {
            Destroy(gameObject);
        }
    }


}
