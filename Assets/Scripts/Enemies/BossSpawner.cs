using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//attach this script to the room object
public class BossSpawner : EnemySpawner
{
    Collider2D roomCollider;

    [SerializeField] private Transform spawnPoint;

    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private UIValueBar healthUIBar;


    private bool spawned = false;

    private void Awake()
    {
        healthUIBar.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (spawned) return;

        if (collision.CompareTag("BossEnemy"))
        {
            DespawnBoss();
        }

        if (!collision.CompareTag("Player")) return;
        SpawnBoss();
        spawned = true;


    }

    private void OnTriggerExit2D(Collider2D collision)
    {

        DespawnBoss();

        spawned = false;
    }


    private GameObject currentBoss = null;
    public void SpawnBoss()
    {
        currentBoss = Instantiate(bossPrefab, spawnPoint.position, Quaternion.identity);
        currentBoss.GetComponent<BossEnemy>().HealthBar = healthUIBar.GetComponent<UIValueBar>();
        healthUIBar.gameObject.SetActive(true);
    }

    public void DespawnBoss()
    {
        if (currentBoss != null)
        {
            Destroy(currentBoss);
            currentBoss = null;
            healthUIBar.gameObject.SetActive(false);
        }

    }

}