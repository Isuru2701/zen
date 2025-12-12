using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//attach this script to the room object
public class EnemySpawner : MonoBehaviour
{
    private Collider2D roomCollider;
    private List<GameObject> spawnMarkers = new List<GameObject>();
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    private void Awake()
    {
        if (roomCollider == null) roomCollider = GetComponent<Collider2D>();
        CollectMarkers();
    }

    private void CollectMarkers()
    {

        spawnMarkers = this.GetComponentsInChildren<Transform>(true)
            .Where(t => t.CompareTag("Enemy"))
            .Select(t => t.gameObject)
            .ToList();

        Debug.Log("Collected " + spawnMarkers.Count + " spawn markers in " + gameObject.name);
    }

    public void SpawnEnemies()
    {

        foreach (var marker in spawnMarkers)
        {
            GameObject enemy = Instantiate(marker, marker.transform.position, marker.transform.rotation);
            spawnedEnemies.Add(enemy);
        }

        Debug.Log("Spawned " + spawnedEnemies.Count + " enemies in " + gameObject.name);

    }

    public void DespawnEnemies()
    {
        foreach (var enemy in spawnedEnemies)
        {
            
        }

        spawnedEnemies.Clear();

        Debug.Log("Despawned all enemies in " + gameObject.name);

    }

}