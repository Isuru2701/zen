using UnityEngine;



public class Collectible : MonoBehaviour
{

    public enum CollectibleType
    {
        WaterLily,
        GhostOrchid,
        Talisman,
        Key

    }

    [SerializeField] CollectibleType type;

    [SerializeField] private Modal modal;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            switch (type)
            {
                case CollectibleType.WaterLily:
                    Items.Lily = true;
                    break;
                case CollectibleType.GhostOrchid:
                    Items.Orchid = true;
                    break;
                case CollectibleType.Talisman:
                    Items.Talisman = true;
                    break;
                case CollectibleType.Key:
                    Items.Key = true;
                    break;
            }

            Destroy(gameObject);


            if(modal != null)
                modal.ShowItemModal();
        }
    }

}
