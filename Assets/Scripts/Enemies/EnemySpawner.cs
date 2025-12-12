using UnityEngine;



//attach this script to the room object
public class EnemySpawner : MonoBehaviour
{
    [SerializeField]private GameObject[] enemies;


    public void SpawnEnemies()
    {
        foreach (GameObject enemy in enemies)
        {
            Instantiate(enemy, enemy.transform.position, Quaternion.identity);
        }
    }

    public void DespawnEnemies()
    {
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }
    }

    
}