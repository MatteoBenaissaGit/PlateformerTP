using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeGeneration : MonoBehaviour
{
    [SerializeField] private List<GameObject> _spawnPoints = null;

    private int _nextSpawnPointIndex;
    public int SpawnPointsCount => _spawnPoints.Count;
    [SerializeField] private GameObject PrefabLife;
    [SerializeField] private GameObject SpawnPointLife;
    [SerializeField] private int choosedSpawnPoint;

    [Header("Alarm")]
    [SerializeField] private float lifeSpawnAlarm = 0f;
    [SerializeField] private float lifeSpawnCooldown = 5f;

    [Header("Security")]
    [SerializeField] private bool lifeCanSpawn = true;
    private void Start()
    {
        SetupAlarm();
    }

    private void SetupAlarm()
    {
        lifeSpawnAlarm = lifeSpawnCooldown;
    }

    private void Update()
    {
        EnemySpawning();
    }

    private void EnemySpawning()
    {
        //alarm countdown
        lifeSpawnAlarm -= Time.deltaTime;

        if(lifeSpawnAlarm <= 0)
        {
            GetRandomSpawnPoint();
            Instantiate(PrefabLife, SpawnPointLife.transform.position, Quaternion.identity);
            SetupAlarm();
        }
    }
    private void GetRandomSpawnPoint()
    {
        choosedSpawnPoint = Random.Range(0, _spawnPoints.Count);
        SpawnPointLife = _spawnPoints[choosedSpawnPoint];
    }
}
