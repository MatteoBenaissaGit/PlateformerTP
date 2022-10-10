using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGeneration : MonoBehaviour
{
    [SerializeField] private List<GameObject> _spawnPoints = null;

    private int _nextSpawnPointIndex;
    public int SpawnPointsCount => _spawnPoints.Count;
    [SerializeField] private GameObject PrefabEnemy;
    [SerializeField] private GameObject SpawnPointEnemy;
    [SerializeField] private int choosedSpawnPoint;

    [Header("Alarm")]
    [SerializeField] private float enemySpawnAlarm = 0f;
    [SerializeField] private float enemySpawnCooldown = 5f;

    [Header("Security")]
    [SerializeField] private bool enemyCanSpawn = true;
    private void Start()
    {
        SetupAlarm();
    }

    private void SetupAlarm()
    {
        enemySpawnAlarm = enemySpawnCooldown;
    }

    private void Update()
    {
        EnemySpawning();
    }

    private void EnemySpawning()
    {
        //alarm countdown
        enemySpawnAlarm -= Time.deltaTime;

        if(enemySpawnAlarm <= 0)
        {
            GetRandomSpawnPoint();
            Instantiate(PrefabEnemy, SpawnPointEnemy.transform.position, Quaternion.identity);
            SetupAlarm();
        }
    }
    private void GetRandomSpawnPoint()
    {
        choosedSpawnPoint = Random.Range(0, _spawnPoints.Count);
        SpawnPointEnemy = _spawnPoints[choosedSpawnPoint];
    }
}
