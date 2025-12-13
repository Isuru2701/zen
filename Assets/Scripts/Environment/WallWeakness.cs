using UnityEngine;

public class WallWeakness : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private DamageReceiver receiver;

    [SerializeField]private GameObject parent;

    void Start()
    {
        receiver = GetComponent<DamageReceiver>();
        receiver.onHurt += Hit;

    }

    public void Hit(DamageInfo info)
    {
        Debug.Log("Wall weakness hit " + GameManager.CurrentGameMode);

        Destroy(parent);
    }


}
