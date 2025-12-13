using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//attach this script to the room object
public class EnemySpawner : MonoBehaviour
{
    Collider2D roomCollider;
    List<EnemyController> spawnMarkers = new List<EnemyController>();
    List<EnemyController> spawnedEnemies = new List<EnemyController>();

    void Awake()
    {
        if (roomCollider == null) roomCollider = GetComponent<Collider2D>();
        CollectMarkers();
    }

    void CollectMarkers()
    {

        spawnMarkers = this.GetComponentsInChildren<EnemyController>(true)
            .Where(t => t.CompareTag("Enemy"))
            .ToList();

        Debug.Log("Collected " + spawnMarkers.Count + " spawn markers in " + gameObject.name);
    }

    public void SpawnEnemies()
    {

        foreach (var marker in spawnMarkers)
        {
            var enemy = marker.GetComponent<EnemyController>();
            enemy.Respawn();
            spawnedEnemies.Add(enemy);
        }

        Debug.Log("Spawned " + spawnedEnemies.Count + " enemies in " + gameObject.name);

    }

    public void DespawnEnemies()
    {
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                enemy.Deactivate();
            }
        }

        spawnedEnemies.Clear();

        Debug.Log("Despawned all enemies in " + gameObject.name);

    }

}